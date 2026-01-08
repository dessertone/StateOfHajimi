using Arch.Core;

namespace WarInPalace.Core.Systems;

public abstract class BaseSystem:ISystem
{
    protected World GameWorld { get; }


    public BaseSystem(World world)
    {
        GameWorld = world;
    }
    
    
    public virtual void Initialize()
    {
        
    }

    public abstract void Update(float deltaTime);
}