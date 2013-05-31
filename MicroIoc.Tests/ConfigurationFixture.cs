using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroIoc.Tests
{
    [TestClass]
    public class ConfigurationFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainerCannotBeNull()
        {
            new FakeContainer().GetConfiguration();
        }

        [TestMethod]
        public void ConfigurationHoldsReferenceToContainer()
        {
            var container = new MicroIocContainer();

            var configuration = container.GetConfiguration();

            var configType = configuration.GetType();
            var fieldInfo = configType.GetField("_container", BindingFlags.NonPublic|BindingFlags.Instance);

            Assert.AreSame(container, fieldInfo.GetValue(configuration));
        }

        [TestMethod]
        public void ConfigurationIsFluent()
        {
            var container = new MicroIocContainer();

            var config1 = container.GetConfiguration();
            var config2 = config1.Configure<Bar>(new InjectedProperty<Foo>("Foo", new Foo()));

            Assert.AreSame(config1, config2);
        }

        [TestMethod]
        public void ConfiguredPropertyInjectionGetsResolved()
        {
            const string customerName = "Bloggs & Co";
            var container = new MicroIocContainer()
                .Register<IFirst, First>()
                .Register<ISecond, Second>();

            container.GetConfiguration()
                .Property<TestClassWithProperties, string>(c => c.CustomerName, customerName);

            var instance = container.Resolve<TestClassWithProperties>();

            Assert.AreEqual(customerName, instance.CustomerName);
        }

        [TestMethod]
        public void ContainerResolvesConfiguredPropertyIfSpecified()
        {
            var first = new First();

            var container = new MicroIocContainer()
                .Register<IFirst, First>()
                .Register<ISecond, Second>();

            container.GetConfiguration()
                .Configure<TestClassWithProperties>(new InjectedProperty<string>("CustomerName", "Anything"))
                .Configure<TestClassWithProperties>(new InjectedProperty<First>("FirstProperty", first));

            container.GetConfiguration()
                .Configure<TestClassWithPropertiesAndConstructor>(new InjectedProperty<string>("CustomerName",
                                                                                                "Whatever"));

            var instance = container.Resolve<TestClassWithProperties>();
            Assert.AreSame(first, instance.FirstProperty);

            var otherInstance = container.Resolve<TestClassWithPropertiesAndConstructor>();
            Assert.AreNotSame(first, otherInstance.FirstProperty);
        }

        [TestMethod]
        public void ConfiguredConstructorParameterInjectionGetsResolved()
        {
            const string connectionString =
                "Data Source=myServerAddress;Initial Catalog=myDataBase;User Id=myUsername;Password=myPassword;";

            var container = new MicroIocContainer();

            container.GetConfiguration()
                .ConstructorParam<SampleRepository, string>("connectionString", connectionString);

            var instance = container.Resolve<SampleRepository>();

            Assert.AreEqual(connectionString, instance.ConnectionString);
        }
    }



    class FakeContainer : IMicroIocContainer
    {
        public IMicroIocContainer Register<T>(string key, bool isSingleton)
        {
            throw new NotImplementedException();
        }

        public IMicroIocContainer Register<T>(Type type, string key = null, bool isSingleton = false)
        {
            throw new NotImplementedException();
        }

        public IMicroIocContainer Register<TFrom, TTo>(string key, bool isSingleton) where TTo : TFrom
        {
            throw new NotImplementedException();
        }

        public IMicroIocContainer RegisterInstance<TInterface>(TInterface instance, string key)
        {
            throw new NotImplementedException();
        }

        public IMicroIocContainer RegisterInstance(Type getType, object instance, string key)
        {
            throw new NotImplementedException();
        }

        public IMicroIocContainer RegisterAllViewModels(Assembly assembly = null, bool isSingleton = false)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>(string key)
        {
            throw new NotImplementedException();
        }

        public T TryResolve<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type, string key)
        {
            throw new NotImplementedException();
        }

        public object TryResolve(Type type, string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            throw new NotImplementedException();
        }

        public IConfiguration GetConfiguration()
        {
            return new ContainerConfiguration(null);
        }

        public void BuildUp(object instance)
        {
            throw new NotImplementedException();
        }
    }


    class SampleRepository
    {
        public SampleRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }
    }
}
