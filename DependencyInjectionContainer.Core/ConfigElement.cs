namespace DependencyInjectionContainer.Core
{
    public class ConfigElement
    {
        public Type DependencyType { get; private set; }
        public Type ImplementationType { get; private set; }
        public LifeTime LifeTime { get; private set; }
        public ConfigElement(Type dependency, Type implementation, LifeTime lifeTime)
        {
            DependencyType = dependency;
            ImplementationType = implementation;
            LifeTime = lifeTime;
        }
    }
}
