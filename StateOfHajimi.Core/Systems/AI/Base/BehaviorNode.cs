using Arch.Core;

namespace StateOfHajimi.Core.Systems.AI.Base;

public abstract class BehaviorNode
{
    public abstract NodeStatus Execute(Entity entity, float deltaTime);
}