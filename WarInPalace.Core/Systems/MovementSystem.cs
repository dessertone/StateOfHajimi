using System.Numerics;
using Arch.Core;
using WarInPalace.Core.Components;

namespace WarInPalace.Core.Systems;

public class MovementSystem:BaseSystem
{
    private static readonly QueryDescription _movementQuery = new QueryDescription().WithAll<Position, Velocity>();


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