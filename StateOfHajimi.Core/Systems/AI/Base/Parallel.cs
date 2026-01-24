using Arch.Core;

namespace StateOfHajimi.Core.Systems.AI.Base;

public class Parallel(List<BehaviorNode> children) : BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        foreach (var child in children)
        {
            child.Execute(entity, deltaTime);
        }
        return NodeStatus.Success;
    }
}