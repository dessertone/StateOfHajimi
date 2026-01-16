using Arch.Core;
using Arch.Core.Extensions;
using Serilog;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.AI.Nodes;

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
            targetHealth.Current -= stats.AttackDamage;
            stats.CurrentCooldown = stats.AttackSpeed;
            anim.Switch(AnimationStateType.Attacking);
            if (targetHealth.IsDead)
            {
                targetEntity.Target = Entity.Null;
            }
            // 攻击完成
            return NodeStatus.Success;
        }
        // 还在冷却
        return NodeStatus.Running;
    }
}