namespace StateOfHajimi.Engine.Enums;

public enum RenderStyle
{
    None,
    Sprite,
    // Debug 类
    DebugColliderBox,       // 绿色空心框
    DebugColliderCircle,    // 绿色空心圆
    DebugFlowArrow,         // 深蓝色箭头

    // UI 类
    HealthBarBackground,    // 血条背景（黑半透）
    HealthBarHigh,          // 高血量（绿）
    HealthBarMedium,        // 中血量（橙）
    HealthBarLow,           // 低血量（红）
    
    // 特效类
    HoverMask,              // 鼠标悬停的高亮渐变
    FloatingText,
    SelectionCircle,        // 选中圈
    RallyFlagLine           // 集结点连线
}