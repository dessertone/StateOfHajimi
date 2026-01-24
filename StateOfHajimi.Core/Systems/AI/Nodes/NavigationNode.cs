using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Systems.AI.Base;

namespace StateOfHajimi.Core.Systems.AI.Nodes;

public class NavigationNode:BehaviorNode
{
    /// <summary>
    /// 如果有目的地，向目的地走，否则失败让AI干其他事
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        if(!entity.Has<Velocity,Destination,Position,MoveSpeed, BodyCollider>()) return NodeStatus.Failure;
        ref var destination = ref entity.Get<Destination>();
        ref var position =  ref entity.Get<Position>();
        ref var velocity = ref entity.Get<Velocity>();
        ref var bodyCollider = ref entity.Get<BodyCollider>();
        var moveSpeed = entity.Get<MoveSpeed>().Value;
        if (destination.IsActive)
        {
            var dir = destination.Value - position.Value;
            var distance = dir.LengthSquared();
            if (distance <= destination.StopDistanceSquared)
            {
                position.Value = destination.Value;
                velocity.Value = Vector2.Zero;
                destination.IsActive = false;
                return NodeStatus.Success;
            }
            if (distance <= 70 * 70f)
            {
                velocity.Value = Vector2.Normalize(dir) * moveSpeed;
                return NodeStatus.Success;
            }
            if (entity.Has<FlowAlgorithm>())
            {
                ref var flowAlgorithm = ref entity.Get<FlowAlgorithm>();
                if (flowAlgorithm.FlowField != null)
                {
                    var realPos = bodyCollider.Offset + position.Value;
                    var d =  flowAlgorithm.FlowField.GetFlowDirection(ref realPos);
                    velocity.Value = d * moveSpeed;
                    return NodeStatus.Success;
                }
            }
            else
            {
                velocity.Value = Vector2.Normalize(dir) * moveSpeed;
                return NodeStatus.Success;
            }

        }
        return NodeStatus.Failure;
    }
}