using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.StateComponents;

public struct AnimationState
{
    public int Offset;
    public int StartFrame;
    public int EndFrame;
    public float FrameTimer;
    public float FrameDuration;
    // 当前的状态类型
    public AnimationStateType Type;
    // 单元名称
    public string AnimationKey;
    public bool IsActive;
    public bool IsLoop;
}