using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.StateComponents;

namespace StateOfHajimi.Core.AI.Nodes;

public class AttackNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        ref var targetEntity = ref entity.Get<AttackTarget>();
        if (targetEntity.Target == Entity.Null || !targetEntity.Target.IsAlive()) return NodeStatus.Failure;
        /*ref var anim = ref entity.Get<AnimationState>();*/
        ref var stats = ref entity.Get<CombatStats>();
        stats.CurrentCooldown -= deltaTime;
        if (stats.CurrentCooldown <= 0)
        {
            ref var targetHealth = ref entity.Get<Health>();
            targetHealth.Current -= stats.AttackDamage;
            stats.CurrentCooldown = stats.AttackSpeed;
            if (targetHealth.IsDead)
            {
                // TODO 死亡动画
                targetEntity.Target = Entity.Null;
            }
            // 攻击完成
            return NodeStatus.Success;
        }
        // 还在冷却
        return NodeStatus.Running;
    }
}