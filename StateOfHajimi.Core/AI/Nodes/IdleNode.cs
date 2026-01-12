using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.AI.Base;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.AI.Nodes;

public class IdleNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        ref var velocity = ref entity.Get<Velocity>();
        if (!entity.Has<AnimationState>()) return NodeStatus.Success;
        ref var anim = ref entity.Get<AnimationState>();
        AnimationHelper.PlayAnimation(ref anim, AnimationStateType.Idle);
        velocity.Value = Vector2.Zero; 
        return NodeStatus.Success; 
    }
}