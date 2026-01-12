using Arch.Core;

namespace StateOfHajimi.Core.AI.Base;

public abstract class BehaviorNode
{
    public abstract NodeStatus Execute(Entity entity, float deltaTime);
}