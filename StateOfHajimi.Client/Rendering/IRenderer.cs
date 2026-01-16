using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;
using StateOfHajimi.Client.Utils;

namespace StateOfHajimi.Client.Rendering;

public interface IRenderer : IDisposable
{
    // 初始化资源（加载纹理到显存等）
    void Initialize();
    
    // 核心渲染方法，传入 Skia 画布
    void Render(SKCanvas canvas, Rect bounds, float zoom, Vector2 cameraPos);
    
}