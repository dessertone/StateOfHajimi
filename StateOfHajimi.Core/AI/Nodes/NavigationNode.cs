using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.PathComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Navigation;

namespace StateOfHajimi.Core.AI.Nodes;

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
        if(!entity.Has<Velocity,Destination,Position,MoveSpeed>()) return NodeStatus.Failure;
        ref var destination = ref entity.Get<Destination>();
        ref var position =  ref entity.Get<Position>();
        ref var velocity = ref entity.Get<Velocity>();
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

            if (distance <= 300 * 300f)
            {
                velocity.Value = Vector2.Normalize(dir) * moveSpeed;
                return NodeStatus.Success;
            }
            if (entity.Has<FlowAlgorithm>())
            {
                ref var flowAlgorithm = ref entity.Get<FlowAlgorithm>();
                if (flowAlgorithm.FlowField != null)
                {
                    var d =  flowAlgorithm.FlowField.GetFlowDirection(ref position.Value);
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