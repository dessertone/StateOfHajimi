using Arch.Core;

namespace StateOfHajimi.Core.Systems.AI.Base;

public class Selector:BehaviorNode
{
    private readonly List<BehaviorNode> _children;
    
    public Selector(List<BehaviorNode> children) => _children = children;
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        foreach (var child in _children)
        {
            var result = child.Execute(entity, deltaTime);
            if(result != NodeStatus.Failure) return result;
        }
        return NodeStatus.Failure;
    }
}