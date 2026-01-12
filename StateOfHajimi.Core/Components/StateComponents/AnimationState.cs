using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.StateComponents;

public struct AnimationState
{
    public int Offset;
    public int StartFrame;
    public int EndFrame;
    public float FrameTimer;
    public float FrameDuration;
    public AnimationStateType Type;
    public string AnimationKey;
    public bool IsActive;
}