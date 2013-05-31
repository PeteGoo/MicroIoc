using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroIoc.Tests
{
    [TestClass]
    public class ContainerFixture
    {

        #region Registration tests

        [TestMethod]
        public void CanRegisterWithoutExceptions()
        {
            var threw = false;

            try
            {
                var iocContainer = new MicroIocContainer();
                iocContainer.Register<IFoo, Foo>();
            }
            catch (Exception)
            {
                threw = true;
            }
            finally
            {
                Assert.IsFalse(threw, "Failed to register without an exception");
            }
        }

        [TestMethod]
        public void RegisterIsFluent()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var newContainer = iocContainer.Register<IFoo, Foo>();

            Assert.AreSame(iocContainer, newContainer);
        }

        [TestMethod]
        public void RegisterInstanceIsFluent()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var newContainer = iocContainer.RegisterInstance<IFoo>(new Foo());

            Assert.AreSame(iocContainer, newContainer);
        }

        [TestMethod]
        public void CanRegisterByType()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var newContainer = iocContainer.Register<IFoo>(typeof(Foo));

            Assert.AreSame(iocContainer, newContainer);
        }

        [TestMethod]
        public void RegistrationOfInvalidTypeThrowsException()
        {
            bool wasThrown = false;
            try
            {
                IMicroIocContainer iocContainer = new MicroIocContainer()
                    .Register<IFoo>(typeof(Bar), "key", false);
            }
            catch (RegistrationException)
            {
                wasThrown = true;
            }
            Assert.IsTrue(wasThrown, "The registration should have thrown an exception");
        }


        [TestMethod]
        public void RegisteringSubsequentImplentationTypesOverwritesPrevious()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();
            Assert.IsInstanceOfType(iocContainer.Resolve<IFoo>(), typeof(Foo));

            iocContainer.Register<IFoo, Foo2>();
            Assert.IsInstanceOfType(iocContainer.Resolve<IFoo>(), typeof(Foo2));
        }

        [TestMethod]
        public void RegisteringSubsequentInstancesOfTypeWithSameKeyOverwritesPrevious()
        {
            const string key = "fooKey"; // Nell
            var foo1 = new Foo();
            var foo2 = new Foo();

            var iocContainer = new MicroIocContainer()
                .RegisterInstance<IFoo>(foo1, key);

            Assert.AreSame(foo1, iocContainer.Resolve<IFoo>(key));

            iocContainer.RegisterInstance<IFoo>(foo2, key);

            Assert.AreSame(foo2, iocContainer.Resolve<IFoo>(key));
        }




        [TestMethod]
        public void RegisterAllViewModelsCreatesRegistrationByNameForVmsInAssembly()
        {
            var iocContainer = new MicroIocContainer()
                .RegisterAllViewModels();

            var threw = false;
            try
            {
                var foo = iocContainer.Resolve(null, "FooViewModel");
                Assert.IsInstanceOfType(foo, typeof(FooViewModel));

                var bar = iocContainer.Resolve(null, "BarViewModel");
                Assert.IsInstanceOfType(bar, typeof(BarViewModel));
            }
            catch (Exception)
            {
                threw = true;
            }
            finally
            {
                Assert.IsFalse(threw);
            }
        }

        [TestMethod]
        public void RegisterAllViewModelsDoesNotRegisterUnknownVms()
        {
            var iocContainer = new MicroIocContainer()
                .RegisterAllViewModels();

            var threw = false;
            try
            {
                // There is no such object - this should throw a ResolutionException
                var foo = iocContainer.Resolve(null, "BazViewModel");
            }
            catch (ResolutionException)
            {
                threw = true;
            }
            finally
            {
                Assert.IsTrue(threw);
            }
        }

        #endregion


        #region Resolution tests

        [TestMethod]
        [ExpectedException(typeof(ResolutionException))]
        public void ResolvingUnregisteredInterfaceThrowsException()
        {
            new MicroIocContainer()
                .Resolve<IFoo>();
        }

        [TestMethod]
        [ExpectedException(typeof(ResolutionException))]
        public void ContainerCannotResolveTypeIfParameterIsUnregistered()
        {
            new MicroIocContainer()
                .Resolve<Bar>();
        }

        [TestMethod]
        public void ResoveCreatesItemOfRequiredType()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.Resolve<IFoo>();

            Assert.IsInstanceOfType(instance, typeof(Foo));
        }

        [TestMethod]
        public void ResolveCreatesInstancePerRequest()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance1 = iocContainer.Resolve<IFoo>();
            var instance2 = iocContainer.Resolve<IFoo>();

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void ResolveCreatesSingletonIfSpecified()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>(isSingleton: true);

            var instance1 = iocContainer.Resolve<IFoo>();
            var instance2 = iocContainer.Resolve<IFoo>();

            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void ResolveByKeyGetsCorrectInstance()
        {
            var foo1 = new Foo();
            var foo2 = new Foo();

            var iocContainer = new MicroIocContainer()
                .RegisterInstance<IFoo>(foo1)
                .RegisterInstance<IFoo>(foo2, "theKey");

            var instance = iocContainer.Resolve<IFoo>("theKey");

            Assert.AreNotSame(instance, foo1);
            Assert.AreSame(instance, foo2);
        }

        [TestMethod]
        public void ContainerCanResolveUnregisteredConcreteTypes()
        {
            var iocContainer = new MicroIocContainer();

            var instance = iocContainer.Resolve<Foo>();
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(Foo));
        }

        [TestMethod]
        public void ContainerCanResolveAConcreteTypeWithoutDefaultConstructor()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.Resolve<IFoo>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof (Foo));
        }

        [TestMethod]
        public void ContainerWillChainDependencyInjection()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.Resolve<Bar>();
            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.Foo);
        }

        [TestMethod]
        public void ContainerWillResolveSpecificInstancesInChainedResolution()
        {
            var foo1 = new Foo();
            var iocContainer = new MicroIocContainer()
                .RegisterInstance<IFoo>(foo1);

            var instance = iocContainer.Resolve<Bar>();
            Assert.IsNotNull(instance);
            Assert.AreSame(foo1, instance.Foo);
        }

        [TestMethod]
        public void ContainerInstantiatesConstructorWithMostParameters()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFirst, First>()
                .Register<ISecond, Second>()
                .Register<IThird, Third>();

            var instance = iocContainer.Resolve<OverloadedTestClass>();
            Assert.IsNotNull(instance.First);
            Assert.IsNotNull(instance.Second);
            Assert.IsNotNull(instance.Third);
        }

        [TestMethod]
        public void InjectionWorksOnProperties()
        {
            const string expectedName = "Fabrikam inc.";

            var iocContainer = new MicroIocContainer()
                .RegisterInstance(expectedName, "MicroIoc.Tests.TestClassWithProperty.CustomerName");

            var instance = iocContainer.Resolve<TestClassWithProperty>();
            Assert.AreEqual(expectedName, instance.CustomerName);
        }

        [TestMethod]
        public void ContainerHandlesMultipleProperties()
        {
            const string expectedName = "Fabrikam inc.";

            var iocContainer = new MicroIocContainer()
                .RegisterInstance(expectedName, "MicroIoc.Tests.TestClassWithProperties.CustomerName")
                .Register<IFirst, First>()
                .Register<ISecond, Second>();

            var instance = iocContainer.Resolve<TestClassWithProperties>();

            Assert.AreEqual(expectedName, instance.CustomerName);
            Assert.IsInstanceOfType(instance.FirstProperty, typeof(First));
            Assert.IsInstanceOfType(instance.SecondProperty, typeof(Second));
        }

        [TestMethod]
        public void ContainerHandlesMultipleInjectionMethods()
        {
            const string expectedName = "Fabrikam inc.";

            var iocContainer = new MicroIocContainer()
                .RegisterInstance(expectedName)
                .Register<IFirst, First>()
                .Register<ISecond, Second>();

            var instance = iocContainer.Resolve<TestClassWithPropertiesAndConstructor>();

            Assert.AreEqual(expectedName, instance.CustomerName);
            Assert.IsInstanceOfType(instance.First, typeof(First));
            Assert.IsInstanceOfType(instance.Second, typeof(Second));
        }

        [TestMethod]
        [ExpectedException(typeof(ResolutionException))]
        public void PropertyInjectionThrowsExceptionForUnregisteredDependency()
        {
            const string expectedName = "Fabrikam inc.";

            var iocContainer = new MicroIocContainer()
                .RegisterInstance(expectedName)
                .Register<IFirst, First>();

            // Do not register this one... 
            // iocContainer.Register<ISecond, Second>();

            iocContainer.Resolve<TestClassWithProperties>();
        }

        [TestMethod]
        public void ContainerRetrievesInstancesByKey()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>("Foo", true);

            var instance = iocContainer.Resolve(null, "Foo");
            Assert.IsInstanceOfType(instance, typeof(Foo));
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ContainerRetrievesInstancesByKeyWithSingleParameterTypeRegistration()
        {
            var iocContainer = new MicroIocContainer()
                .Register<Foo2>("Foo2", true);

            var instance = iocContainer.Resolve(null, "Foo2");
            Assert.IsInstanceOfType(instance, typeof (Foo2));
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public void GenericTryResolveUnregisteredInterfaceReturnsNull()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var instance = iocContainer.TryResolve<IFoo>();

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void TryResolveUnregisteredInterfaceReturnsNull()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var instance = iocContainer.TryResolve(typeof(IFoo));

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void GenericTryResolveUnregisteredInterfaceWithKeyReturnsNull()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var instance = iocContainer.TryResolve<IFoo>("SomeKey");

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void TryResolveUnregisteredInterfaceWithKeyReturnsNull()
        {
            IMicroIocContainer iocContainer = new MicroIocContainer();

            var instance = iocContainer.TryResolve(typeof(IFoo), "SomeKey");

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void GenericTryResolveRegisteredInterfaceWithInvalidKeyReturnsNull()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.TryResolve<IFoo>("SomeKey");

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void TryResolveRegisteredInterfaceWithInvalidKeyReturnsNull()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.TryResolve(typeof(IFoo), "SomeKey");

            Assert.IsNull(instance, "Instance should be null");
        }

        [TestMethod]
        public void GenericTryResolveRegisteredInterfaceReturnsInstance()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.TryResolve<IFoo>();

            Assert.IsNotNull(instance, "Instance should not be null");
            Assert.IsInstanceOfType(instance, typeof (IFoo));
        }

        [TestMethod]
        public void TryResolveRegisteredInterfaceReturnsInstance()
        {
            var iocContainer = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instance = iocContainer.TryResolve(typeof (IFoo));

            Assert.IsNotNull(instance, "Instance should not be null");
            Assert.IsInstanceOfType(instance, typeof(IFoo));
        }

        [TestMethod]
        public void GenericResolveAllUnregisteredTypeReturnsEmptyCollection()
        {
            var ioc = new MicroIocContainer();

            var instances = ioc.ResolveAll<Foo>();

            Assert.IsNotNull(instances);
            Assert.AreEqual(0, instances.Count());
        }


        [TestMethod]
        public void ResolveAllUnregisteredTypeReturnsEmptyCollection()
        {
            var ioc = new MicroIocContainer();

            var instances = ioc.ResolveAll(typeof (Foo));

            Assert.IsNotNull(instances);
            Assert.AreEqual(0, instances.Count());
        }

        [TestMethod]
        public void GenericResolveAllReturnsSingleRegisteredTypeInACollection()
        {
            var ioc = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instances = ioc.ResolveAll<IFoo>();

            Assert.IsNotNull(instances);
            Assert.AreEqual(1, instances.Count());
            Assert.IsInstanceOfType(instances.Single(), typeof(IFoo), "Registered instance should be an IFoo");
        }


        [TestMethod]
        public void ResolveAllReturnsSingleRegisteredTypeInACollection()
        {
            var ioc = new MicroIocContainer()
                .Register<IFoo, Foo>();

            var instances = ioc.ResolveAll(typeof(IFoo));

            Assert.IsNotNull(instances);
            Assert.AreEqual(1, instances.Count());
            Assert.IsInstanceOfType(instances.Single(), typeof(IFoo), "Registered instance should be an IFoo");
        }

        [TestMethod]
        public void GenericResolveAllReturnsAllRegisteredTypesInACollection()
        {
            var ioc = new MicroIocContainer()
                .Register<IFoo, Foo>("One")
                .RegisterInstance<IFoo>(new Foo(), "Two")
                .Register<IFoo, Foo>("Three", true);

            var instances = ioc.ResolveAll<IFoo>();

            Assert.IsNotNull(instances);
            Assert.AreEqual(3, instances.Count());
        }

        [TestMethod]
        public void ResolveAllReturnsAllRegisteredTypesInACollection()
        {
            var ioc = new MicroIocContainer()
                .Register<IFoo, Foo>("One")
                .RegisterInstance<IFoo>(new Foo(), "Two")
                .Register<IFoo, Foo>("Three", true);

            var instances = ioc.ResolveAll(typeof(IFoo));

            Assert.IsNotNull(instances);
            Assert.AreEqual(3, instances.Count());
        }

        #endregion


        #region BuildUp tests

        [TestMethod]

        public void BuildUpAppliesDependencyProperty()
        {
            var container = new MicroIocContainer();

            container.GetConfiguration()
                .Property<TestClassWithProperty, string>(x => x.CustomerName, "TestCustomerName");

            var obj = new TestClassWithProperty();

            container.BuildUp(obj);

            Assert.AreEqual("TestCustomerName", obj.CustomerName);
        }

        [TestMethod]
        public void BuildUpDoesNotApplyStandardProperty()
        {
            var container = new MicroIocContainer();

            container.GetConfiguration()
                .Property<TestClassWithProperty, string>(x => x.CustomerName, "TestCustomerName")
                .Property<TestClassWithProperty, string>(x => x.NotInjected, "NotInjected");

            var obj = new TestClassWithProperty();

            container.BuildUp(obj);

            Assert.IsNull(obj.NotInjected, string.Format("Should have been null, but was {0}", obj.NotInjected));
        }

        [TestMethod]
        public void BuildUpResolvesAllInjectedProperties()
        {
            var container = new MicroIocContainer()
                .Register<IFirst, First>()
                .Register<ISecond, Second>();

            container.GetConfiguration()
                .Property<TestClassWithProperties, string>(x => x.CustomerName, "TestCustomerName");

            var obj = new TestClassWithProperties();

            container.BuildUp(obj);

            Assert.AreEqual("TestCustomerName", obj.CustomerName);

            Assert.IsNotNull(obj.FirstProperty, "FirstProperty shouldn't be null");
            Assert.IsInstanceOfType(obj.FirstProperty, typeof(First), "FirstProperty should a 'First' object");

            Assert.IsNotNull(obj.SecondProperty, "SecondProperty shouldn't be null");
            Assert.IsInstanceOfType(obj.SecondProperty, typeof(Second), "SecondProperty should a 'Second' object");
        }

        #endregion
    }

    #region Artefacts


    interface IFoo { }
    class Foo : IFoo { }
    class Foo2 : IFoo { }

    class Bar
    {
        private readonly IFoo _foo;

        public Bar()
            : this(null) { }

        public Bar(IFoo foo)
        {
            _foo = foo;
        }

        public IFoo Foo
        {
            get { return _foo; }
        }
    }

    interface IFirst { }
    class First : IFirst { }

    interface ISecond { }
    class Second : ISecond { }

    interface IThird { }
    class Third : IThird { }

    class OverloadedTestClass
    {
        private readonly IFirst _first;
        private readonly ISecond _second;
        private readonly IThird _third;


        public OverloadedTestClass()
            : this(null, null, null) { }

        public OverloadedTestClass(IFirst first)
            : this(first, null, null) { }

        public OverloadedTestClass(IFirst first, ISecond second)
            : this(first, second, null) { }

        public OverloadedTestClass(IFirst first, ISecond second, IThird third)
        {
            _first = first;
            _second = second;
            _third = third;
        }

        public IFirst First { get { return _first; } }

        public ISecond Second { get { return _second; } }

        public IThird Third { get { return _third; } }
    }

    class TestClassWithProperty
    {
        public string NotInjected { get; set; }

        [Inject]
        public string CustomerName { get; set; }
    }

    class TestClassWithProperties
    {
        [Inject]
        public string CustomerName { get; set; }

        [Inject]
        public IFirst FirstProperty { get; set; }

        [Inject]
        public ISecond SecondProperty { get; set; }
    }

    class TestClassWithPropertiesAndConstructor
    {
        public string Name { get; private set; }
        public IFirst First { get; private set; }
        public ISecond Second { get; private set; }

        public TestClassWithPropertiesAndConstructor(ISecond second)
        {
            Second = second;
        }

        [Inject]
        public string CustomerName
        {
            get { return Name; }
            set { Name = value; }
        }

        [Inject]
        public IFirst FirstProperty
        {
            get { return First; }
            set { First = value; }
        }
    }


    class FooViewModel { }
    class BarViewModel { }

    #endregion

}
