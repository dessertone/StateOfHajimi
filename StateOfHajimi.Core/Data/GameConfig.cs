using Microsoft.Extensions.Configuration;
using Serilog;
using StateOfHajimi.Core.Data.Config;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Data;

public static class GameConfig
{
    private static IConfigurationRoot _configurationRoot;
    private static GameSettings _settings;
    
    private static Dictionary<UnitType, UnitStateConfig> _unitStatsCache = new();

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

    private static void OnConfigChanged(object state)
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
        _unitStatsCache.Clear();
        foreach (var kvp in _settings.Units)
        {
            if (Enum.TryParse<UnitType>(kvp.Key, true, out var type))
            {
                _unitStatsCache[type] = kvp.Value;
            }
            else
            {
                Log.Warning($"Unknown unit type: {kvp.Key}");
            }
        }
    }

    /// <summary>
    /// 获取指定兵种的配置数据
    /// </summary>
    public static UnitStateConfig GetUnitState(UnitType type)
    {
        if (_unitStatsCache.TryGetValue(type, out var state))
        {
            return state;
        }
        Log.Error($"Unknown unit type: {type}, create default value");
        return new UnitStateConfig { MaxHp = 1, Size = 10 };
    }
}