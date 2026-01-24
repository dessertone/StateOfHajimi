using Arch.Core;
using StateOfHajimi.Core.Systems.AI.Base;

namespace StateOfHajimi.Core.Systems.AI.Nodes;

public class SuccessNode: BehaviorNode
{
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        return NodeStatus.Success;
    }
    
}