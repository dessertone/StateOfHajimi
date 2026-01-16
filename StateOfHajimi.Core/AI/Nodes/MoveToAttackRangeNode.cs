using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Serilog;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.AI.Nodes;

public class MoveToAttackRangeNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float dt)
    {
        ref var target = ref entity.Get<AttackTarget>();
        if (target.Target == Entity.Null || !target.Target.IsAlive() || target.Target.Has<IsDying>()) return NodeStatus.Failure;
        if(!target.Target.Has<Position>()) return NodeStatus.Failure;
        ref var pos = ref entity.Get<Position>();
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
            return NodeStatus.Success;
        }
        
        // TODO 目前是直线移动，没有加入寻路逻辑
        var dir = Vector2.Normalize(targetPos - pos.Value);
        velocity.Value = dir * moveSpeed.Value;
        anim.Switch(AnimationStateType.Running);
        return NodeStatus.Running;
    }
}