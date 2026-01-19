using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Components.RenderComponents;

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
    public EntityType AnimationTarget;
    public Direction CurrentDirection;
    public bool IsActive;
    public bool IsLoop;
}