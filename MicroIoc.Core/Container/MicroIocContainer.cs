using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroIoc;

namespace MicroIoc
{
    public class MicroIocContainer : IMicroIocContainer
    {
        private readonly Dictionary<Tuple<Type, string>, Func<object>> _resolverDictionary
            = new Dictionary<Tuple<Type, string>, Func<object>>();
        private readonly IList<Type> _registeredSingletons
            = new List<Type>();
        private readonly Dictionary<Type, object> _singletonInstances
            = new Dictionary<Type, object>();

        private static readonly string CollectionDefaultKey = Guid.NewGuid().ToString();

        #region Register

        /// <summary>
        /// Register a type within the container. Useful when registering a type by key, so calls to Resolve(null, key) will work
        /// </summary>
        /// <typeparam name="T">The type of class being registered</typeparam>
        /// <param name="key">If specified, will associate a specific instance for this type</param>
        /// <param name="isSingleton">Indicates if the registration should yield a singleton object when resolved</param>
        /// <returns>The container, complete with new registration</returns>
        public IMicroIocContainer Register<T>(string key = null, bool isSingleton = false)
        {
            return Register(typeof(T), typeof(T), key, isSingleton);
        }

        public IMicroIocContainer Register<T>(Type type, string key = null, bool isSingleton = false)
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new RegistrationException(string.Format("{0} must implement {1}", type.Name, typeof(T).Name));
            return Register(typeof(T), type, key, isSingleton);
        }

        /// <summary>
        /// Register an implementation type against an interface or class
        /// </summary>
        /// <typeparam name="TFrom">The type of interface or class to be registered</typeparam>
        /// <typeparam name="TTo">The type of concrete class to be instantiated when <see cref="TFrom" /> is resolved from the container.</typeparam>
        /// <returns>The container, complete with new registration</returns>
        public IMicroIocContainer Register<TFrom, TTo>(string key = null, bool isSingleton = false) where TTo : TFrom
        {
            return Register(typeof(TFrom), typeof(TTo), key, isSingleton);
        }
      
        /// <summary>
        /// Register a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <typeparam name="TInterface">The type of interface or class to be registered</typeparam>
        /// <param name="instance">The instance to register in the container</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The container, complete with new registration</returns>
        public IMicroIocContainer RegisterInstance<TInterface>(TInterface instance, string key = null)
        {
            return RegisterInstance(typeof (TInterface), instance, key);
        }

        public IMicroIocContainer RegisterInstance(Type type, object instance, string key=null)
        {
            key = ValueOrDefault(key);

            _resolverDictionary[new Tuple<Type, string>(type, key)] = () => instance;
            return this;
        }

        /// <summary>
        /// Examines the calling assembly for classes that end with 'ViewModel'
        /// and registers them in the container, with their own name as a key
        /// </summary>
        /// <returns>The container, complete with all new registrations</returns>
        public IMicroIocContainer RegisterAllViewModels(Assembly assembly=null, bool isSingleton=false)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            foreach (var type in assembly.GetTypes().Where(t => t.Name.EndsWith("ViewModel", StringComparison.CurrentCultureIgnoreCase)))
            {
                Register(type, type, type.Name, isSingleton);
            }
            return this;
        }

        #endregion

        #region Resolve

        /// <summary>
        /// Resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The registered instance if key is specified, or a dynamically instantiated instance, if not</returns>
        public T Resolve<T>(string key = null)
        {
            return (T) Resolve(typeof (T), key);
        }

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>An instance of <typeparamref name="T"/> if registered, or null</returns>
        public T TryResolve<T>(string key) where T : class
        {
            return (T) TryResolve(typeof (T), key);
        }

        /// <summary>
        /// Resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The registered instance if key is specified, or a dynamically instantiated instance, if not</returns>
        public object Resolve(Type type, string key = null)
        {
            var result = ResolveCore(type, key);
            BuildUp(result);
            return result;
        }

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>An instance of  if registered, or null</returns>
        public object TryResolve(Type type, string key)
        {
            try
            {
                return Resolve(type, key);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resolve all registered instances of a specified type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        public IEnumerable<T> ResolveAll<T>()
        {
            var allObjects = ResolveAll(typeof (T));

            return allObjects.Select(obj => (T) obj);
        }

        /// <summary>
        /// Resolve all registered instances of a specified type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        public IEnumerable<object> ResolveAll(Type type)
        {
            return _resolverDictionary.Keys
                .Where(key => key.Item1 == type)
                .Select(t => _resolverDictionary[t]());
        }

        #endregion

        /// <summary>
        /// Create an instance with properties from the container.
        /// Only properties attributed [Inject] will be set.
        /// </summary>
        /// <param name="instance"></param>
        public void BuildUp(object instance)
        {
            var propertyInfoCollection = instance.GetType().GetProperties();

            foreach (var propertyInfo in propertyInfoCollection)
            {
                var info = propertyInfo;
                if (!info.GetCustomAttributes(typeof(InjectAttribute), false).Any()) continue;

                object property = null;
                try
                {
                    var fullPropertyName = string.Format("{0}.{1}", instance.GetType().FullName, info.Name);
                    property = Resolve(null, fullPropertyName);
                }
                catch (Exception)
                {
                    property = Resolve(info.PropertyType);
                }
                finally
                {
                    info.SetValue(instance, property, null);
                }
            }
        }


        public IConfiguration GetConfiguration()
        {
            return new ContainerConfiguration(this);
        }


        #region Private helper methods

        private IMicroIocContainer Register(Type fromType, Type toType, string key, bool isSingleton)
        {
            key = ValueOrDefault(key);

            if (isSingleton)
                _registeredSingletons.Add(toType);

            _resolverDictionary[new Tuple<Type, string>(fromType, key)] = () => BuildFromType(toType);
            return this;
        }

        private object ResolveCore(Type type, string key)
        {
            if (type == null)
            {
                type = DeriveType(key);
                if (type == null)
                    throw new ResolutionException("Failed to Derive type for " + key);
            }

            key = ValueOrDefault(key);

            return _resolverDictionary.ContainsKey(new Tuple<Type, string>(type, key))
                       ? _resolverDictionary[new Tuple<Type, string>(type, key)]()
                       : BuildFromType(type);
        }

        private Type DeriveType(string key)
        {
            var result = GetTypeFromContainer(key);
            if (result != null) return result;

            //  Not in the container? Try the Assembly
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .SingleOrDefault(t => t.Name == key);
        }

        private Type GetTypeFromContainer(string key)
        {
            var tuple = _resolverDictionary
                .Keys
                .FirstOrDefault(t => t.Item2 == key);

            return (tuple == null)
                       ? null
                       : tuple.Item1;
        }

        private object BuildFromType(Type type)
        {
            if (_registeredSingletons.Contains(type))
            {
                object instance;
                if (_singletonInstances.TryGetValue(type, out instance))
                    return instance;
                instance = InstantiateInstance(type);

                _singletonInstances[type] = instance;
                return instance;
            }

            return InstantiateInstance(type);
        }

        private object InstantiateInstance(Type type)
        {
            var constructor = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
            if (constructor == null)
                throw new ResolutionException("Could not locate a constructor for " + type.FullName);

            var constructorParams = new List<object>(constructor.GetParameters().Length);
            foreach (var parameterInfo in constructor.GetParameters())
            {
                object parameter=null;
                try
                {
                    string key = type.ConstructorParamPattern(parameterInfo.Name);
                    parameter = Resolve(null, key);
                }
                catch (Exception)
                {
                    parameter = Resolve(parameterInfo.ParameterType);
                }
                finally
                {
                    constructorParams.Add(parameter);    
                }
            }

            try
            {
                return constructor.Invoke(constructorParams.ToArray());
            }
            catch (Exception exception)
            {
                throw new ResolutionException("Failed to resolve " + type.Name, exception);
            }
        }

        private static string ValueOrDefault(string key)
        {
            if (key != null) key = key.Trim();
            return string.IsNullOrEmpty(key)
                ? CollectionDefaultKey
                : key;
        }

        #endregion
    }
}