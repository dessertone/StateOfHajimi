using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using Serilog;
using StateOfHajimi.Client.Input.Core;
using StateOfHajimi.Core.Components.StateComponents;

namespace StateOfHajimi.Client.Utils;

public static class AssetsManager
{
    private static readonly Dictionary<string, SpriteSheet> _sheets = new();
    private static readonly Dictionary<CursorType, Cursor> _cursors = new();
    private const int TargetCursorSize = 64;
    public static async Task InitializeAsync()
    {
        LoadSpriteSheet("LittleHajimiFactory_blue", "Assets/Factories/LightFactory/LightFactory_sheet.png", 2050, 2050);
        LoadSpriteSheet("LittleHajimiFactory_red", "Assets/Factories/LightFactory/LightFactory-red_sheet.png", 2050, 2050);
        LoadSpriteSheet("GroundTexture", "Assets/GroundTexture.jpg", 1024, 1024);
        LoadSpriteSheet("CrystalCluster", "Assets/CrystalCluster.png", 1024, 1024);
        LoadSpriteSheet("CatStatue", "Assets/CatStatue.png", 1024, 1024);
        LoadSpriteSheet("LittleHajimi_blue", "Assets/Entities/LittleHajimi.png", 256, 279);
        LoadSpriteSheet("LittleHajimi_red", "Assets/Entities/LittleHajimi.png", 256, 279);
        
        
        LoadCursorStyle(CursorType.Default, "Assets/Cursors/default.png", new PixelPoint(220, 220));
        LoadCursorStyle(CursorType.Hand, "Assets/Cursors/hover.png", new PixelPoint(220, 220));
        LoadCursorStyle(CursorType.Flag, "Assets/Cursors/flag.png", new PixelPoint(300, 800));
        
        Log.Information("Assets initialized.");
    }

    private static void LoadSpriteSheet(string key, string path, int frameW, int frameH)
    {
        try 
        {
            var uri = new Uri($"avares://StateOfHajimi.Client/{path}");
            
            using var stream1 = AssetLoader.Open(uri);
            var avaloniaBitmap = new Bitmap(stream1);
            
            using var stream2 = AssetLoader.Open(uri); 
            using var tempBitmap = SKBitmap.Decode(stream2);


            if (tempBitmap == null)
            {
                Log.Error($"Failed to decode Bitmap for {key}");
                return;
            }
            var skiaImage = SKImage.FromBitmap(tempBitmap);
            
            Log.Information($"Load {key} from {path} successfully. (Skia Size: {skiaImage.Width}x{skiaImage.Height})");
            
            if (_sheets.ContainsKey(key))
            {
                _sheets[key].Dispose();
            }
            _sheets[key] = new SpriteSheet(avaloniaBitmap, skiaImage, frameW, frameH);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load sheet {path}: {ex.Message}");
        }
    }


    private static void LoadCursorStyle(CursorType key, string path, PixelPoint originalHotSpot)
    {
        try
        {
            var uri = new Uri($"avares://StateOfHajimi.Client/{path}");
            using var stream = AssetLoader.Open(uri);
            using var originalBitmap = SKBitmap.Decode(stream);
            if (originalBitmap == null)
            {
                Log.Error($"Failed to decode cursor bitmap for {path}");
                return;
            }
            
            var scale = (float)TargetCursorSize / originalBitmap.Width;
            
            var newWidth = TargetCursorSize;
            var newHeight = (int)(originalBitmap.Height * scale);
            
            var info = new SKImageInfo(newWidth, newHeight);
            using var resizedBitmap = originalBitmap.Resize(info, SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var avaloniaBitmap = new Bitmap(ms);
            
            var newHotSpot = new PixelPoint(
                (int)(originalHotSpot.X * scale), 
                (int)(originalHotSpot.Y * scale)
            );
            _cursors[key] = new Cursor(avaloniaBitmap, newHotSpot);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to load cursor {path}: {ex.Message}");
        }
    }

    public static SpriteSheet? GetSheet(string key, int teamId)
    {
        if(teamId == -1)
            return _sheets.TryGetValue(key, out var s) ? s : null;
        if(teamId == 0)
            return _sheets.TryGetValue(key + "_blue", out var s0) ? s0 : null;
        if(teamId == 1)
            return _sheets.TryGetValue(key + "_red", out var s1) ? s1 : null;
        return null;
    }

    public static Cursor? GetCursor(CursorType key) => _cursors.TryGetValue(key, out var c) ? c : null;
    
    public static void Dispose()
    {
        foreach (var sheet in _sheets.Values)
        {
            sheet.Dispose();
        }
        _sheets.Clear();
    }
}