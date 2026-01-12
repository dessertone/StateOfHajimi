using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;

namespace StateOfHajimi.Core.Systems.AI;

public class AISystem: BaseSystem
{
    private static readonly QueryDescription _AIQuery = new QueryDescription()
        .WithAll<AIController, Position, Health>();
    public AISystem(World world) : base(world)
    {
        
    }
    public override void Update(float deltaTime)
    {
        GameWorld.Query(in _AIQuery, (Entity entity, ref AIController ai, ref Health hp) =>
        {
            if (hp.IsDead) return; 
            ai.RootNode.Execute(entity, deltaTime);
        });
    }
}