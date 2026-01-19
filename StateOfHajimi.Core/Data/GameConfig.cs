using Microsoft.Extensions.Configuration;
using Serilog;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Data.Config;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Data;

public static class GameConfig
{
    private static IConfigurationRoot _configurationRoot;
    private static GameSettings _settings;
    
    private static readonly Dictionary<EntityType, EntityStateConfig> _entityStatsCache = new();
    
    private static readonly Dictionary<EntityType, UnitAnimationConfig> _unitAnimation = new();
    
    
    public static void Initialize()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("GameSettings.json", optional: false, reloadOnChange: true);
        _configurationRoot = builder.Build();
        LoadSettings();
        _configurationRoot.GetReloadToken().RegisterChangeCallback(OnConfigChanged, null);
        
        Log.Information("GameSettings.json loaded");
    }

    private static void OnConfigChanged(object? state)
    {
        Log.Information("Configuration Changed...");
        LoadSettings();
        _configurationRoot.GetReloadToken().RegisterChangeCallback(OnConfigChanged!, null);
    }

    
    private static void LoadSettings()
    {
        _settings = new GameSettings();
        _configurationRoot.GetSection("GameSettings").Bind(_settings);
        
        RefreshUnitCache();
    }

    /// <summary>
    /// 根据Json数据将string转换为Enum
    /// </summary>
    private static void RefreshUnitCache()
    {
        _entityStatsCache.Clear();
        foreach (var kvp in _settings.Entities)
        {
            if (Enum.TryParse<EntityType>(kvp.Key, true, out var type))
            {
                _entityStatsCache[type] = kvp.Value;
            }
            else
            {
                Log.Warning($"Unknown unit type: {kvp.Key}");
            }
        }
        _unitAnimation.Clear();
        
        foreach (var kvp in _settings.UnitAnimations)
        {
            if (Enum.TryParse<EntityType>(kvp.Key, true, out var animType))
            {
                _unitAnimation[animType] = kvp.Value;
                _unitAnimation[animType].BakeCache();
            }
            else
            {
                Log.Warning($"Unknown unit animation type: {kvp.Key}");
            }
        }
    }

    /// <summary>
    /// 获取指定兵种的配置数据
    /// </summary>
    public static EntityStateConfig GetUnitState(EntityType type)
    {
        if (_entityStatsCache.TryGetValue(type, out var state))
        {
            return state;
        }
        Log.Error($"Unknown unit type: {type}, create default value");
        return new EntityStateConfig { MaxHp = 1, Size = 10 };
    }
    
    public static UnitAnimationConfig? GetUnitAnimation(EntityType key)
    {
        if (_unitAnimation.TryGetValue(key, out var animation))
        {
            return animation;
        }
        Log.Warning($"Unknown unit animation key: {key}, check your EntityType!");
        return null;
    }
    public static UnitAnimationConfig? GetEntityAnimation(EntityType type) => GetUnitAnimation(type);
    
    
    
}