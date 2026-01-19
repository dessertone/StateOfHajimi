using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Serilog;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Maths;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.AI.Nodes;

public class AggressiveSearchTargetNode: BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        if(!entity.Has<AttackTarget,Position,TeamId,CombatStats,Velocity,AnimationState,MoveSpeed>())return NodeStatus.Failure;
        ref var target = ref entity.Get<AttackTarget>();
        ref var myPos = ref entity.Get<Position>();
        ref var myTeam = ref entity.Get<TeamId>();
        ref var stats = ref entity.Get<CombatStats>();
        if (target.Target != Entity.Null && !target.Target.Has<Disabled>() && !target.Target.Has<IsDying>())
        {
            return NodeStatus.Success; 
        }
        var nearbyEntities = SpatialGrid.Instance.Retrieve(myPos.Value, stats.VisionRange); 
        
        var minDstSq = stats.VisionRange * stats.VisionRange; 
        var bestTarget = Entity.Null;

        foreach (var other in nearbyEntities)
        {
            if (other == entity) continue;
            if (!other.Has<TeamId,Health>()|| other.Has<IsDying>()) continue;
            
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