using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Systems.AI.Base;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.Systems.AI.Nodes;

public class MoveToAttackRangeNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float dt)
    {
        ref var target = ref entity.Get<AttackTarget>();
        if (target.Target == Entity.Null || !target.Target.IsAlive() || target.Target.Has<IsDying>() || !target.Target.Has<Position>() || !entity.Has<Destination,Position,Velocity,CombatStats,AnimationState,MoveSpeed>()) return NodeStatus.Failure;
        ref var pos = ref entity.Get<Position>();
        ref var dest = ref entity.Get<Destination>();
        ref var velocity = ref entity.Get<Velocity>();
        ref var stats = ref entity.Get<CombatStats>();
        ref var anim = ref entity.Get<AnimationState>();
        ref var moveSpeed = ref entity.Get<MoveSpeed>();
        var targetPos = target.Target.Get<Position>().Value;
        var distSq = Vector2.DistanceSquared(pos.Value, targetPos);
        var rangeSq = stats.AttackRange * stats.AttackRange;
        if (distSq <= rangeSq)
        {
            velocity.Value = Vector2.Zero; 
            dest.IsActive = false;
            return NodeStatus.Success;
        }

        dest.Value = targetPos;
        dest.IsActive = true;
        anim.Switch(AnimationStateType.Running);
        return NodeStatus.Running;
    }
}