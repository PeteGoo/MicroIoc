using System;
using System.Collections.Generic;
using System.Reflection;

namespace MicroIoc
{
    public interface IMicroIocContainer
    {
        /// <summary>
        /// Register a type within the container. Useful when registering a type by key, so calls to Resolve(null, key) will work
        /// </summary>
        /// <typeparam name="T">The type of class being registered</typeparam>
        /// <param name="key">If specified, will associate a specific instance for this type</param>
        /// <param name="isSingleton">Indicates if the registration should yield a singleton object when resolved</param>
        /// <returns>The container, complete with new registration</returns>
        IMicroIocContainer Register<T>(string key = null, bool isSingleton = false);

        /// <summary>
        /// Register a type within the container.
        /// </summary>
        /// <typeparam name="T">The interface type being registered</typeparam>
        /// <param name="type">The type of the implementation to resolve. Must implement <see cref="T" /></param>
        /// <param name="key">If specified, will associate a specific instance for this type</param>
        /// <param name="isSingleton">Indicates if the registration should yield a singleton object when resolved</param>
        /// <returns>The container, complete with new registration</returns>
        IMicroIocContainer Register<T>(Type type, string key = null, bool isSingleton = false);

        /// <summary>
        /// Register an implementation type against an interface or class
        /// </summary>
        /// <typeparam name="TFrom">The type of interface or class to be registered</typeparam>
        /// <typeparam name="TTo">The type of concrete class to be instantiated when <see cref="TFrom" /> is resolved from the container.</typeparam>
        /// <param name="key">If specified, will associate a specific instance for this type</param>
        /// <param name="isSingleton">Indicates if the registration should yield a singleton object when resolved</param>
        /// <returns>The container, complete with new registration</returns>
        IMicroIocContainer Register<TFrom, TTo>(string key = null, bool isSingleton = false) where TTo : TFrom;

        /// <summary>
        /// Register a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <typeparam name="TInterface">The type of interface or class to be registered</typeparam>
        /// <param name="instance">The instance to register in the container</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The container, complete with new registration</returns>
        IMicroIocContainer RegisterInstance<TInterface>(TInterface instance, string key = null);

        /// <summary>
        /// Register a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <param name="type">The type of interface or class to be registered</param>
        /// <param name="instance">The instance to register in the container</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The container, complete with new registration</returns>
        IMicroIocContainer RegisterInstance(Type type, object instance, string key = null);

        /// <summary>
        /// Examines the calling assembly for classes that end with 'ViewModel'
        /// and registers them in the container, with their own name as a key
        /// </summary>
        /// <param name="assembly">The assembly in which to seek out ViewModels to register</param>
        /// <param name="isSingleton">Indicates if the registration should yield a singleton object when resolved</param>
        /// <returns>The container, complete with all new registrations</returns>
        IMicroIocContainer RegisterAllViewModels(Assembly assembly = null, bool isSingleton = false);

        /// <summary>
        /// Resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The registered instance if key is specified, or a dynamically instantiated instance, if not</returns>
        T Resolve<T>(string key = null);

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>An instance of <typeparamref name="T"/> if registered, or null</returns>
        T TryResolve<T>(string key = null) where T : class;

        /// <summary>
        /// Resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>The registered instance if key is specified, or a dynamically instantiated instance, if not</returns>
        object Resolve(Type type, string key = null);

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <param name="key">(Optional) a key to specify the instance within the container</param>
        /// <returns>An instance of  if registered, or null</returns>
        object TryResolve(Type type, string key = null);

        /// <summary>
        /// Resolve all registered instances of a specified type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>
        /// Resolve all registered instances of a specified type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        IEnumerable<object> ResolveAll(Type type);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IConfiguration GetConfiguration();

        /// <summary>
        /// Create an instance with properties from the container.
        /// Only properties attributed [Inject] will be set.
        /// </summary>
        /// <param name="instance"></param>
        void BuildUp(object instance);
    }
}
