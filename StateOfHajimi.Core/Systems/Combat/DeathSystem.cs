using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.Systems.Combat;

public partial class DeathSystem:BufferBaseSystem
{
    public DeathSystem(World world) : base(world) { }
    
    [Query]
    [All<Health, EntityClass>, None<Disabled, IsDying>]
    private void DeathAnimation(Entity entity, ref Health hp, ref AnimationState anim)
    {
        if (hp.IsDead)
        {   
            Buffer.Add<IsDying>(entity);
            anim.Switch(AnimationStateType.Dying);
        }
    }

    [Query]
    [All<IsDying, EntityClass>, None<Disabled>]
    private void ReturnEntities(Entity entity, ref AnimationState anim, ref EntityClass entityClass)
    {
        if ( anim.IsActive == false)
        {
            Buffer.Remove<IsDying>(entity);
            EntityPool.Despawn(Buffer, entity, entityClass.Type);
        }
    }
    
}