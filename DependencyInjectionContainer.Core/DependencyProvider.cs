using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace DependencyInjectionContainer.Core
{
    public class DependencyProvider
    {
        private DependenciesConfiguration _config;
        private ConcurrentDictionary<ConfigElement, object> _singletones;
        private ConcurrentStack<Type> _dependensiesStack = new ConcurrentStack<Type>();
        public DependencyProvider(DependenciesConfiguration config)
        {
            _config = config;
            if (!ConfigurationIsCorrect())
            {
                throw new Exception("Dependencies in configuration is incorrect");
            }
            _singletones = new ConcurrentDictionary<ConfigElement, object>();
            foreach (var element in _config.ConfigElements)
            {
                if (element.LifeTime == LifeTime.Singleton)
                {
                    var singletone = Resolve(element.DependencyType);
                    while (!_singletones.ContainsKey(element))
                    {
                        _singletones.TryAdd(element, singletone);
                    }
                }
            }
        }
        private bool ConfigurationIsCorrect()
        {
            foreach (var element in _config.ConfigElements)
            {
                Type impl = element.ImplementationType;
                Type dep = element.DependencyType;
                if (impl.IsClass && !impl.IsAbstract)
                {
                    if (!(impl.IsSubclassOf(dep) ||
                        impl.GetInterfaces().Contains(dep) ||
                        impl == dep ||
                            (element.LifeTime == LifeTime.InstancePerDependency && 
                            dep.IsGenericType && 
                            impl
                            .GetInterfaces()
                            .ToList()
                            .Find(i => i.IsGenericType && i.GetGenericTypeDefinition() == dep.GetGenericTypeDefinition()) != null)))
                    {
                        return false;
                    }
                } else return false;
            }
            return true;
        }
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }
        private object Resolve(Type type)
        {
            if (_dependensiesStack.Contains(type))
                return default;
            _dependensiesStack.Push(type);

            object result;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genArgs = type.GetGenericArguments();
                var implementations = GetEnumerableWith(genArgs.First());
                var castMethod = typeof(Enumerable).GetMethod("Cast");
                var genericCastMethod = castMethod?.MakeGenericMethod(genArgs);
                result = genericCastMethod.Invoke(null, new object[] { implementations });
            }
            else
            {
                var element = _config.ConfigElements.Find(e => e.DependencyType.Name == type.Name);
                result = GetImplementation(element);
            }

            while (!_dependensiesStack.TryPop(out type)) { }
            return result;
        }

        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                ParameterInfo[] parametersInfo = constructor.GetParameters();
                List<object> parameters = new List<object>();
                foreach (ParameterInfo parameterInfo in parametersInfo)
                {
                    if (_config.ConfigElements.Any(e => e.DependencyType == parameterInfo.ParameterType))
                    {
                        parameters.Add(Resolve(parameterInfo.ParameterType));
                    }
                    else
                    {
                        object? obj = default;
                        try
                        {
                            obj = Activator.CreateInstance(parameterInfo.ParameterType);
                        }
                        catch { }
                        parameters.Add(obj);
                    }
                }
                try
                {
                    return constructors[0].Invoke(parameters.ToArray());
                }
                catch { }
            }
            return default;
        }
        private object GetImplementation(ConfigElement element)
        {
            if (element.ImplementationType.IsGenericTypeDefinition &&
                element.ImplementationType.IsGenericType &&
                element.ImplementationType.GetGenericArguments().First().FullName == null)
            {
                if (element.LifeTime == LifeTime.Singleton)
                {
                    if (!_singletones.ContainsKey(element))
                        _singletones.TryAdd(element, CreateInstance(element.ImplementationType));
                    return _singletones[element];
                }
                else
                {
                    var genericType = element.ImplementationType.MakeGenericType(element.DependencyType.GetGenericArguments());
                    return CreateInstance(genericType);
                }
            }
            if (element.LifeTime == LifeTime.Singleton)
            {
                if (!_singletones.ContainsKey(element))
                    while (!_singletones.TryAdd(element, CreateInstance(element.ImplementationType))) { }
                return _singletones[element];
            }
            else
            {
                return CreateInstance(element.ImplementationType);
            }
        }

        private IEnumerable<object> GetEnumerableWith(Type type)
        {
            IEnumerable<object> objects = new List<object>();
            var elements = _config.ConfigElements.FindAll(e => e.DependencyType == type);
            foreach (ConfigElement element in elements)
            {
                objects = objects.Append(GetImplementation(element));
            }
            return objects;
        }
    }
}
