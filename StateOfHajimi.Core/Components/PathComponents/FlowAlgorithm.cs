using StateOfHajimi.Core.Maths;

namespace StateOfHajimi.Core.Components.PathComponents;

public struct FlowAlgorithm(FlowField? flowAlgorithm, bool isActive = true)
{
    public FlowField? FlowField = flowAlgorithm;
    public bool IsActive = isActive;
}