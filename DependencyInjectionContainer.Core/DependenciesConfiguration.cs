namespace DependencyInjectionContainer.Core
{
    public class DependenciesConfiguration
    {
        public List<ConfigElement> ConfigElements { get; private set; }
        public DependenciesConfiguration()
        {
            ConfigElements = new List<ConfigElement>();
        }
        public void Register<D, I>(LifeTime lifeTime)
        {
            Register(typeof(D), typeof(I), lifeTime);
        }
        public void Register(Type dependency, Type implememntation, LifeTime lifeTime)
        {
            if (dependency.IsGenericType)
                dependency = dependency.GetGenericTypeDefinition();
            if (!ConfigElements.Any(e =>
            {
                return e.DependencyType == dependency && e.ImplementationType == implememntation;
            }))
                ConfigElements.Add(new ConfigElement(dependency, implememntation, lifeTime));
        }
    }
}
