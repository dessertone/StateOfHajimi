using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Maths;

namespace StateOfHajimi.Core.AI.Nodes;

public class CheckTargetNode: BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        if (!entity.Has<AttackTarget, CombatStats, Position, TeamId>()) return NodeStatus.Failure;

        ref var attackTarget = ref entity.Get<AttackTarget>();
        if (attackTarget.Target != Entity.Null && !attackTarget.Target.Has<IsDying>() && !attackTarget.Target.Has<Disabled>()) return NodeStatus.Failure;
        
        ref var CombatStats = ref entity.Get<CombatStats>();
        ref var position = ref entity.Get<Position>();
        ref var team = ref entity.Get<TeamId>().Value;
        
        var entities = SpatialGrid.Instance.Retrieve(position.Value, CombatStats.AttackRange);
        var attackRangeSq = CombatStats.AttackRange * CombatStats.AttackRange;
        var bestTarget = Entity.Null;
        foreach (var otherEntity in entities)
        {
            if (!otherEntity.Has<TeamId,Health,Position>()
                || otherEntity.Has<Disabled>()
                || otherEntity.Has<IsDying>()
                || otherEntity == entity) continue;
            if (otherEntity.Get<TeamId>().Value == team) continue;

            var otherPos = otherEntity.Get<Position>().Value;
            var dstSq = Vector2.DistanceSquared(position.Value, otherPos);
            if (dstSq <= attackRangeSq)
            {
                bestTarget = otherEntity;
            }
        }
        if (bestTarget != Entity.Null)
        {
            attackTarget.Target = bestTarget;
            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}