namespace StateOfHajimi.Core.Data.Config;

// 单元的某个状态动画配置
public class StateAnimationInfo
{
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }
    public float FrameDuration { get; set; }
    public bool IsLoop { get; set; }
}

// 单个单元的动画配置
public class UnitAnimationConfig
{
    public int TotalFrames { get; set; }
    public Dictionary<string, StateAnimationInfo> StateAnimations { get; set; } = new();
}