using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Systems.AI.Base;
using StateOfHajimi.Core.Utils.Extensions;

namespace StateOfHajimi.Core.Systems.AI.Nodes;

public class IdleNode:BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        if (!entity.Has<AnimationState,Velocity>()) return NodeStatus.Success;
        ref var velocity = ref entity.Get<Velocity>();
        ref var anim = ref entity.Get<AnimationState>();
        anim.Switch(AnimationStateType.Idle, Direction.South);
        velocity.Value = Vector2.Zero; 
        return NodeStatus.Success; 
    }
}