using Arch.Core;

namespace StateOfHajimi.Core.AI.Base;

public class Sequence:BehaviorNode
{
    private readonly List<BehaviorNode> _children;
    public Sequence(List<BehaviorNode> children) => _children = children;
    public override NodeStatus Execute(Entity entity, float deltaTime)
    {
        foreach (var child in _children)
        {
            var result = child.Execute(entity, deltaTime);
            if (result != NodeStatus.Success) return result;
        }
        return NodeStatus.Success;
    }
}