namespace StateOfHajimi.Core.Components.GlobalComponents;

public struct DebugSettings(bool showFlowField, bool showColliderBox)
{
    public bool ShowFlowField = showFlowField;
    public bool ShowColliderBox = showColliderBox;
}