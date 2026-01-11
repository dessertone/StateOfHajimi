using Arch.Core;
using StateOfHajimi.Core.Components.MoveComponents;

namespace StateOfHajimi.Core.Systems.Movement;

public class MovementSystem:BaseSystem
{
    private static readonly QueryDescription _movementQuery = new QueryDescription()
        .WithAll<Position, Velocity>();


    public MovementSystem(World world) : base(world)
    {
    }
    

    public override void Update(float deltaTime)
    {
        GameWorld.Query(in _movementQuery, (ref Position p, ref Velocity v) =>
        {
            p.Value += v.Value * deltaTime;
        });
    }
}