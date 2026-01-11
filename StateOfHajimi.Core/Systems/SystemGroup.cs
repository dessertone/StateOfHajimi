namespace StateOfHajimi.Core.Systems;

public class SystemGroup:ISystem
{
    private readonly List<ISystem> _systems = new ();
    
    public void Add(ISystem system) => _systems.Add(system);
    
    
    public void Initialize()
    {
        foreach (var system in _systems) system.Initialize();
    }

    public void Update(float deltaTime)
    {
        foreach (var system in _systems) system.Update(deltaTime);
    }
}