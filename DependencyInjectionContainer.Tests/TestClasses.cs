using DependencyInjectionContainer.Core;

namespace DependencyInjectionContainer.Tests
{
    interface IInterface { }
    class ClassFromInterface : IInterface { }
    class ClassFromInterface2 : IInterface { }
    abstract class AbstractClass { }
    class ClassFromAbstractClass : AbstractClass { }
    class Class { }

    interface IService
    {
        public IRepository Repository { get; set; }
    }
    class ServiceImpl : IService
    {
        public IRepository Repository { get; set; }
        public ServiceImpl(IRepository repository)
        {
            Repository = repository;
        }
    }

    interface IRepository { }
    class RepositoryImpl : IRepository
    {
        public RepositoryImpl() { }
    }

    interface IService<RepositoryImpl> where RepositoryImpl : IRepository
    {
        public IRepository Repository { get; set; }
    }

    class ServiceImpl<TRepository> : IService<TRepository>
        where TRepository : IRepository
    {
        public IRepository Repository { get; set; }
        public ServiceImpl(IRepository repository)
        {
            Repository = repository;
        }
    }
}
