using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Systems.RenderSystem;

public partial class EffectSystem: BufferBaseSystem
{
    public EffectSystem(World world) : base(world)
    {
    }

    [Query]
    [All<Life, Position, Text, Velocity, MoveSpeed>, None<Disabled>]
    private void UpdateEffects([Data]in float deltaTime,Entity entity, ref Life life, ref Position pos, ref Velocity vel, ref MoveSpeed speed)
    {
        life.Age += deltaTime;
        if(life.LifeTime <= life.Age)
        {
            EntityPool.Despawn(Buffer, entity, EntityType.FloatingText);
            return;
        }
        vel.Value *= speed.Value;
    }
    
}