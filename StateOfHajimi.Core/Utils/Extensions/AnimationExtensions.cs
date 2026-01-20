using Serilog;
using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Data;
using StateOfHajimi.Core.Data.Config;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Utils.Extensions;

public static class AnimationExtensions
{
    /// <summary>
    /// 切换动画状态
    /// </summary>
    /// <param name="unitName">单元名称</param>
    /// <param name="anim">单元拥有的动画引用传递</param>
    /// <param name="type">想要切换的动画类型 </param>
    public static void Switch(this ref AnimationState anim, AnimationStateType type, Direction facing = Direction.None)
    {
        
        if (anim.Type == type && facing == anim.CurrentDirection) return;
        anim.CurrentDirection = facing;
        var config = GameConfig.GetUnitAnimation(anim.AnimationTarget);
        if (config == null) return;
        if(config.TryGetInfo(new AnimationKey(type, facing), out var stateInfo))
        {
            anim.Type = type;
            anim.StartFrame = stateInfo.StartFrame;
            anim.EndFrame = stateInfo.EndFrame;
            anim.FrameDuration = stateInfo.FrameDuration;
            anim.Offset = 0;
            anim.FrameTimer = 0;
            anim.IsLoop = stateInfo.IsLoop;
            anim.IsActive = true;
        }
        else
        {
            /*Log.Warning($"unit {anim.AnimationTarget}'s animation type {type} facing {facing} not found!");*/
        }
    }
}