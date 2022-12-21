using DependencyInjectionContainer.Core;

namespace DependencyInjectionContainer.Tests
{
    public class Tests
    {
        DependenciesConfiguration configuration;
        DependencyProvider provider;
        [SetUp]
        public void Setup()
        {
            configuration = new DependenciesConfiguration();
        }

        [Test]
        public void TestCorrectRegistration()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.Singleton);
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.Singleton);
            var elements = configuration.ConfigElements.FindAll(e =>
            {
                return e.DependencyType == typeof(IInterface) &&
                e.ImplementationType == typeof(ClassFromInterface) &&
                e.LifeTime == LifeTime.Singleton;
            });
            Assert.That(elements, Has.Count.EqualTo(1));
        }

        [Test]
        public void TestCreateClassFromInterface()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ClassFromInterface);

            var obj = provider.Resolve<IInterface>();
            var actualType = obj.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
        }
        [Test]
        public void TestCreateClassFromAbstractClass()
        {
            configuration.Register<AbstractClass, ClassFromAbstractClass>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ClassFromAbstractClass);

            var obj = provider.Resolve<AbstractClass>();
            var actualType = obj.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
        }
        [Test]
        public void TestCreateClassAsSelf()
        {
            configuration.Register<Class, Class>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(Class);

            var obj = provider.Resolve<Class>();
            var actualType = obj.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
        }
        [Test]
        public void TestCreateSingleton()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.Singleton);
            provider = new DependencyProvider(configuration);

            var obj1 = provider.Resolve<IInterface>();
            var obj2 = provider.Resolve<IInterface>();

            Assert.That(obj1, Is.EqualTo(obj2));
        }
        [Test]
        public void TestCreateInstancePerDependency()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);

            var obj1 = provider.Resolve<IInterface>();
            var obj2 = provider.Resolve<IInterface>();

            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }
        [Test]
        public void TestRecursionCreation()
        {
            configuration.Register<IService, ServiceImpl>(LifeTime.InstancePerDependency);
            configuration.Register<IRepository, RepositoryImpl>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);

            var expectedServiceType = typeof(ServiceImpl);
            var expectedRepositoryType = typeof(RepositoryImpl);

            var service = provider.Resolve<IService>();

            Assert.That(service.GetType(), Is.EqualTo(expectedServiceType));
            Assert.That(service.Repository.GetType(), Is.EqualTo(expectedRepositoryType));
        }
        [Test]
        public void TestTemplateType()
        {
            configuration.Register<IRepository, RepositoryImpl>(LifeTime.InstancePerDependency);
            configuration.Register<IService<IRepository>, ServiceImpl<IRepository>>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ServiceImpl<IRepository>);
            var expectedReposytoryType = typeof(RepositoryImpl);

            var actual = provider.Resolve<IService<IRepository>>();
            var actualType = actual.GetType();
            var actualReposytoryType = actual.Repository.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
            Assert.That(actualReposytoryType, Is.EqualTo(expectedReposytoryType));
        }
        [Test]
        public void TestTemplateTypeOpenGeneric()
        {
            configuration.Register<IRepository, RepositoryImpl>(LifeTime.InstancePerDependency);
            configuration.Register(typeof(IService<>), typeof(ServiceImpl<>), LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ServiceImpl<IRepository>);
            var expectedReposytoryType = typeof(RepositoryImpl);

            var actual = provider.Resolve<IService<IRepository>>();
            var actualType = actual.GetType();
            var actualReposytoryType = actual.Repository.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
            Assert.That(actualReposytoryType, Is.EqualTo(expectedReposytoryType));
        }
        [Test]
        public void TestManyImplementations()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.InstancePerDependency);
            configuration.Register<IInterface, ClassFromInterface2>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType1 = typeof(ClassFromInterface);
            var expectedType2 = typeof(ClassFromInterface2);

            IEnumerable<IInterface> list = provider.Resolve<IEnumerable<IInterface>>();
            var actualType1 = list.ElementAt(0).GetType();
            var actualType2 = list.ElementAt(1).GetType();

            Assert.That(actualType1, Is.EqualTo(expectedType1));
            Assert.That(actualType2, Is.EqualTo(expectedType2));
        }
        [Test]
        public void TestResolveParametersGeneric()
        {
            configuration.Register<IInterface, ClassFromInterface>(LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ClassFromInterface);

            var obj = provider.Resolve<IInterface>();
            var actualType = obj.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
        }
        [Test]
        public void TestResolveParametersTypeOf()
        {
            configuration.Register(typeof(IInterface), typeof(ClassFromInterface), LifeTime.InstancePerDependency);
            provider = new DependencyProvider(configuration);
            var expectedType = typeof(ClassFromInterface);

            var obj = provider.Resolve<IInterface>();
            var actualType = obj.GetType();

            Assert.That(actualType, Is.EqualTo(expectedType));
        }
        [Test]
        public void TestIncorrectDependency()
        {
            DependenciesConfiguration config1 = new DependenciesConfiguration();
            DependenciesConfiguration config2 = new DependenciesConfiguration();
            DependenciesConfiguration config3 = new DependenciesConfiguration();
            DependenciesConfiguration config4 = new DependenciesConfiguration();
            DependenciesConfiguration config5 = new DependenciesConfiguration();

            config1.Register<IInterface, IInterface>(LifeTime.Singleton);
            config3.Register<IInterface, ClassFromAbstractClass>(LifeTime.Singleton);
            config5.Register<AbstractClass, ClassFromInterface>(LifeTime.Singleton);
            config2.Register<AbstractClass, AbstractClass>(LifeTime.Singleton);
            config4.Register<ClassFromAbstractClass, AbstractClass>(LifeTime.Singleton);

            Assert.Throws<Exception>(() => new DependencyProvider(config1));
            Assert.Throws<Exception>(() => new DependencyProvider(config2));
            Assert.Throws<Exception>(() => new DependencyProvider(config3));
            Assert.Throws<Exception>(() => new DependencyProvider(config4));
            Assert.Throws<Exception>(() => new DependencyProvider(config5));
        }
    }
}