using StateOfHajimi.Core.Components.RenderComponents;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Data.Config;

// 单元的某个状态动画配置
public class StateAnimationInfo
{
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }
    public float FrameDuration { get; set; }
    public bool IsLoop { get; set; }
}

public record struct AnimationKey(AnimationStateType Type, Direction Facing);

// 单个单元的动画配置
public class UnitAnimationConfig
{
    public int TotalFrames { get; set; }
    public Dictionary<string, StateAnimationInfo> StateAnimations { get; set; } = new();
    
    private readonly Dictionary<AnimationKey, StateAnimationInfo> _quickCache = new();

    public void BakeCache()
    {
        _quickCache.Clear();
        foreach (var item in StateAnimations)
        {
            var key = item.Key;
            var parts = key.Split('_');
            if (Enum.TryParse<AnimationStateType>(parts[0], out var type))
            {
                var dir = Direction.None;
                if (parts.Length > 1 && Enum.TryParse<Direction>(parts[1], out var parseDir))
                    dir = parseDir;
                _quickCache[new AnimationKey(type, dir)] = item.Value;
            }
        }
    }
    
    public bool TryGetInfo(AnimationKey key, out StateAnimationInfo info)
    {
        return _quickCache.TryGetValue(key, out info);
    }
}