using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BalticAmadeus.Container.Tests
{
    [TestClass]
    public class ContainerTests
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void Setup()
        {
            _containerBuilder = new ContainerBuilder();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Container_Resolve_CannotCallUseMoreThanOnce()
        {
            _containerBuilder.For<ISimple>().Use<Simple>().Use<Simple>();
            Assert.Fail("Should not reach here");
        }

        [TestMethod]
        public void Container_Resolve_CanResolveSimpleAbstractions()
        {
            //ARRANGE
            _containerBuilder.For<ISimple>().Use<Simple>();
            
            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();

            //ASSERT
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Container_Resolve_CanResolveSimpleTypesWithoutRegistering()
        {
            //ARRANGE
            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<Simple>();

            //ASSERT
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Container_Resolve_CanResolveTreeForInterface()
        {
            //ARRANGE
            _containerBuilder.For<ISimple>().Use<Simple>();
            _containerBuilder.For<INested>().Use<Nested>();
            
            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<INested>();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Simple);
        }

        [TestMethod]
        public void Container_Resolve_CanUseFactories()
        {
            //ARRANGE
            var wasFactoryCalled = false;

            _containerBuilder.For<ISimple>().Use(c =>
            {
                wasFactoryCalled = true;
                return new Simple();
            });
            _containerBuilder.For<INested>().Use<Nested>();
            
            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<INested>();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(wasFactoryCalled);
        }

        [TestMethod]
        public void Container_Resolve_CanUseFactoriesThatUseResolve()
        {
            //ARRANGE
            var wasNestedFactoryCalled = false;

            _containerBuilder.For<ISimple>().Use<Simple>();
            _containerBuilder.For<INested>().Use(c =>
            {
                wasNestedFactoryCalled = true;
                return new Nested(c.Resolve<ISimple>());
            });

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<INested>();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(wasNestedFactoryCalled);
        }

        [TestMethod]
        public void Container_Resolve_CanUseExternalParameters()
        {
            //ARRANGE
            var externalSimpleObject = new Simple();

            _containerBuilder.For<INested>().Use<Nested>()
                .WithParameter<ISimple>(externalSimpleObject);

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<INested>();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreSame(result.Simple, externalSimpleObject);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithProxy()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>().DecorateWithProxy<ListLogProxy>();

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithConcreteClass()
        {
            //ARRANGE
            _containerBuilder.For<ISimple>().Use<Simple>().DecorateWith<SimpleDecorator>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(((SimpleDecorator)result).DecoratorWasCalled);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithProxyChain()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>()
                .DecorateWithProxy<ListLogProxy>().DecorateWithProxy<OtherListLogProxy>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(3, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateFactoriesWithProxy()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>(() => new Simple()).DecorateWithProxy<ListLogProxy>();

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithDIResolvedParameters()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>();
            _containerBuilder.For<INested>().Use<Nested>().DecorateWithProxy<ProxyWithAdditionalArguments>();

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<INested>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(2, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithDIResolvedParametersOfSameInstance()
        {
            //ARRANGE
            ProxyWithAdditionalArguments.LastSimple = null;
            ProxyWithSameAdditionalArguments.LastSimple = null;
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>();
            _containerBuilder.For<INested>().Use<Nested>()
                .DecorateWithProxy<ProxyWithAdditionalArguments>().DecorateWithProxy<ProxyWithSameAdditionalArguments>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<INested>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(4, ListLogProxy.LoggedMessages.Count);
            Assert.AreSame(ProxyWithAdditionalArguments.LastSimple, ProxyWithSameAdditionalArguments.LastSimple);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithDefaultDecorator()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.WithDefaultDecorator<ISimple, ListLogProxy>();
            _containerBuilder.For<ISimple>().Use<Simple>();

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_DefaultDecoratorsAreAppliedByConcreteType()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.WithDefaultDecorator<ISimpleOnlyInConcrete, ListLogProxy>();
            _containerBuilder.For<ISimple>().Use<Simple>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithDefaultAndExplicitDecorator()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.WithDefaultDecorator<ISimple, ListLogProxy>();
            _containerBuilder.For<ISimple>().Use<Simple>().DecorateWithProxy<OtherListLogProxy>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(2, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithUniqueDefaultDecorator()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.WithDefaultDecorator<ISimple, ListLogProxy>();
            _containerBuilder.WithDefaultDecorator<ISimple, ListLogProxy>();
            _containerBuilder.For<ISimple>().Use<Simple>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanDecorateWithUniqueDecorator()
        {
            //ARRANGE
            ListLogProxy.LoggedMessages.Clear();

            _containerBuilder.For<ISimple>().Use<Simple>().DecorateWithProxy<ListLogProxy>();

            var container = _containerBuilder.Build();

            //ACT
            var result = container.Resolve<ISimple>();
            result.Test();

            //ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ListLogProxy.LoggedMessages.Count);
        }

        [TestMethod]
        public void Container_Resolve_CanRegisterSingleton()
        {
            //ARRANGE
            _containerBuilder.For<ISingleton>().Use<Singleton>().AsSingleton();

            var container = _containerBuilder.Build();
            
            //ACT
            var result1 = container.Resolve<ISingleton>();
            var result2 = container.Resolve<ISingleton>();

            Assert.AreSame(result1, result2);
        }

        [TestMethod]
        public void Container_Resolve_CanRegisterSingletonInstance()
        {
            //ARRANGE
            var singletonInstance = new Singleton();
            _containerBuilder.For<ISingleton>().Use<Singleton>(() => singletonInstance);

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<ISingleton>();

            //ASSERT
            Assert.AreSame(singletonInstance, result);
        }

        [TestMethod]
        public void Container_Resolve_CanRelease()
        {
            //ARRANGE
            bool wasCalled = false;

            _containerBuilder.For<ISimple>().Use<Simple>().OnRelease(s =>
            {
                wasCalled = true;
            });
            _containerBuilder.For<INested>().Use<Nested>();

            var container = _containerBuilder.Build();
            
            //ACT
            var result = container.Resolve<ISimple>();
            container.Release(result);

            //ASSERT
            Assert.IsTrue(wasCalled);
        }
    }
}
