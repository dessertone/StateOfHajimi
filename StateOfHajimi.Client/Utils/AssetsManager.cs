using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Client.Utils;

public static class AssetsManager
{
    private static readonly Dictionary<string, Bitmap> _textures = new();
    private static readonly Dictionary<TileType, IBrush> _tileBrushes = new();

    public static void Initialize()
    {
        LoadTexture("LightFactory", "Assets/Factories/量子猫窝.png");
        LoadTexture("LightFactory-red", "Assets/Factories/量子猫窝-red.png");
        LoadTexture("LightweightMechanicalCat", "Assets/Entities/轻型机械豪猫");
        _tileBrushes[TileType.Grass] = Brushes.DarkGreen;
        _tileBrushes[TileType.Dirt] = Brush.Parse("#5e4d33");
        _tileBrushes[TileType.Water] = Brushes.DeepSkyBlue;
        _tileBrushes[TileType.Wall] = Brushes.WhiteSmoke;
        
        Log.Information("AssetManager initialized.");
    }

    private static void LoadTexture(string key, string path)
    {
        try
        {
            var uri = new Uri($"avares://StateOfHajimi.Client/{path}");
            _textures[key] = new Bitmap(AssetLoader.Open(uri));
            Log.Information($"loaded {key} from {path}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load texture {path}: {ex.Message}");
        }
    }

    public static Bitmap? GetTexture(string key) => _textures.TryGetValue(key, out var bmp) ? bmp : null;
    
    public static IBrush GetTileBrush(TileType type) => _tileBrushes.TryGetValue(type, out var brush) ? brush : Brushes.Magenta;


    public static string GetTextureKey(UnitType type) => type switch
    {
        UnitType.HeavyweightMechanicalCat => "HeavyweightMechanicalCat",
        UnitType.LightweightMechanicalCat => "LightweightMechanicalCat",
        _ => "HeavyweightMechanicalCat"
    };
}