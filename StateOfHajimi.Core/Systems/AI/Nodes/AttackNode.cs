using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Systems.AI.Base;
using StateOfHajimi.Core.Utils.Extensions;
using StateOfHajimi.Engine.Utils;

namespace StateOfHajimi.Core.Systems.AI.Nodes;

public class AttackNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        ref var targetEntity = ref entity.Get<AttackTarget>();
        if (targetEntity.Target == Entity.Null || !targetEntity.Target.IsAlive() || targetEntity.Target.Has<IsDying>()) return NodeStatus.Failure;
        if (!entity.Has<AnimationState, CombatStats>()) return NodeStatus.Failure;
        ref var anim = ref entity.Get<AnimationState>();
        ref var stats = ref entity.Get<CombatStats>();
        stats.CurrentCooldown -= deltaTime;
        if (stats.CurrentCooldown <= 0)
        {
            ref var targetHealth = ref targetEntity.Target.Get<Health>();
            if (targetHealth.IsDead)
            {
                targetEntity.Target = Entity.Null;
                return NodeStatus.Failure;
            }
            targetHealth.Current -= stats.AttackDamage;
            stats.CurrentCooldown = stats.AttackSpeed;
            anim.Switch(AnimationStateType.Attacking);
            if(!targetEntity.Target.Has<Position>() || !entity.Has<Position>()) return NodeStatus.Failure;
            // 攻击完成
            return NodeStatus.Success;
        }
        // 还在冷却
        return NodeStatus.Running;
    }
}