using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.AI.Nodes;

public class CheckTargetNode: BehaviorNode
{
    
    
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        ref var target = ref entity.Get<AttackTarget>();
        ref var myPos = ref entity.Get<Position>();
        ref var myTeam = ref entity.Get<TeamId>();
        ref var stats = ref entity.Get<CombatStats>();
        if (target.Target != Entity.Null && target.Target.IsAlive())
        {
            return NodeStatus.Success; 
        }
        var nearbyEntities = SpatialGrid.Instance.Retrieve(myPos.Value); 
        
        var minDstSq = stats.VisionRange * stats.VisionRange; 
        var bestTarget = Entity.Null;

        foreach (var other in nearbyEntities)
        {
            if (other == entity) continue;
            if (!other.Has<TeamId>() || !other.Has<Health>()) continue;
            
            if (other.Get<TeamId>().Value == myTeam.Value) continue;

            var otherPos = other.Get<Position>().Value;
            var dstSq = Vector2.DistanceSquared(myPos.Value, otherPos);
            if (dstSq < minDstSq)
            {
                minDstSq = dstSq;
                bestTarget = other;
            }
        }
        if (bestTarget != Entity.Null)
        {
            target.Target = bestTarget;
            return NodeStatus.Success;
        }
        return NodeStatus.Failure; 
    }
}