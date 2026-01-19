using System;
using Avalonia;
using Avalonia.Media.Imaging;
using SkiaSharp; // 必须引入

namespace StateOfHajimi.Client.Utils;

public class SpriteSheet : IDisposable 
{
    public Bitmap Texture { get; }
    
    public SKImage SkiaImage { get; } 

    public int FrameWidth { get; }
    public int FrameHeight { get; }
    
    private readonly int _columns; 

    public SpriteSheet(Bitmap avaloniaBitmap, SKImage skiaImage, int frameWidth, int frameHeight)
    {
        Texture = avaloniaBitmap;
        SkiaImage = skiaImage;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        
        _columns = skiaImage.Width / frameWidth;
    }
    
    public Rect GetTextureRect(int index)
    {
        var row = index / _columns;
        var col = index % _columns;
        return new Rect(col * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
    }
    
    public SKRect GetSkRect(int index)
    {
        var row = index / _columns;
        var col = index % _columns;
        
        float x = col * FrameWidth;
        float y = row * FrameHeight;
        
        return new SKRect(x, y, x + FrameWidth, y + FrameHeight);
    }

    public void Dispose()
    {
        Texture?.Dispose();
        SkiaImage?.Dispose();
    }
}