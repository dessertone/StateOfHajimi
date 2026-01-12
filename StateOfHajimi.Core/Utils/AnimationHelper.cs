using Serilog;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils;

public static class AnimationHelper
{
    /// <summary>
    /// 切换动画状态
    /// </summary>
    /// <param name="unitName">单元名称</param>
    /// <param name="anim">单元拥有的动画引用传递</param>
    /// <param name="type">想要切换的动画类型 </param>
    public static void PlayAnimation(ref AnimationState anim, AnimationStateType type)
    {
        if (anim.Type == type) return;
        var config = GameConfig.GetUnitAnimation(anim.AnimationKey);
        if (config == null) return;
        
        if (config.StateAnimations.TryGetValue(type.ToString(), out var stateInfo))
        {
            anim.Type = type;
            anim.StartFrame = stateInfo.StartFrame;
            anim.EndFrame = stateInfo.EndFrame;
            anim.FrameDuration = stateInfo.FrameDuration;
            anim.Offset = 0;
            anim.FrameTimer = 0;
        }
        else
        {
             Log.Warning($"unit {anim.AnimationKey}'s animation type {type} not found!");
        }
    }
}