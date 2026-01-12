using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Client.Utils;

public static class AssetsManager
{
    // 改存 SpriteSheet
    private static readonly Dictionary<string, SpriteSheet> _sheets = new();

    public static async Task InitializeAsync()
    {
        LoadSpriteSheet("LightFactory", "Assets/Factories/LightFactory/LightFactory_sheet.png", 2050, 2050);
        LoadSpriteSheet("LightFactory-red", "Assets/Factories/LightFactory/LightFactory-red_sheet.png", 2050, 2050);
        LoadSpriteSheet("GroundTexture", "Assets/GroundTexture.jpg", 1024, 1024);
        LoadSpriteSheet("CrystalCluster", "Assets/CrystalCluster.png", 1024, 1024);
        LoadSpriteSheet("CatStatue", "Assets/CatStatue.png", 1024, 1024);
        Log.Information("Assets initialized.");
    }

    private static void LoadSpriteSheet(string key, string path, int frameW, int frameH)
    {
        try
        {
            var uri = new Uri($"avares://StateOfHajimi.Client/{path}");
            var bitmap = new Bitmap(AssetLoader.Open(uri));
            Log.Information(bitmap.ToString());
            _sheets[key] = new SpriteSheet(bitmap, frameW, frameH);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load sheet {path}: {ex.Message}");
        }
    }
    public static SpriteSheet? GetSheet(string key) => _sheets.TryGetValue(key, out var s) ? s : null;
}