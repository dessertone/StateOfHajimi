using Arch.Core;
using StateOfHajimi.Core.AI.Base;

namespace StateOfHajimi.Core.AI.Nodes;

public class SuccessNode: BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        return NodeStatus.Success;
    }
    
}