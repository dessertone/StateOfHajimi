using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.Tags;

namespace StateOfHajimi.Core.Systems.Movement;

public partial class MovementSystem:BaseSystem<World, float>
{
    public MovementSystem(World world) : base(world) { }

    [Query]
    [All<Position, Velocity>, None<Disabled,IsDying>]
    private void Move([Data] in float deltaTime,ref Position p, ref Velocity v)
    {
        p.Value += v.Value * deltaTime;
    }
}