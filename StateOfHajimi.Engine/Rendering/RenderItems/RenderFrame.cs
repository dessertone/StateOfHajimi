using System.Numerics;
using SkiaSharp;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Rendering.RenderItems;

public class RenderFrame
{
    public List<RenderCommand> RenderCommands { get; } = new(20000);
    public List<RenderSprite> Sprites { get; } = new(10000);
    public List<RenderText> Texts { get; } = new(10000);
    public List<RenderGeometry> Geometries { get; } = new(10000);

    private readonly Lock _lock = new();

    public SKRect Bounds { get; set; }

    public void AddSprite(RenderStyle style, RenderLayer layer, float sortZ, SKImage image, SKRect srcRect, SKRect destRect)
    {
        lock (_lock)
        {
            Sprites.Add(new RenderSprite(style, image, srcRect, destRect));
            RenderCommands.Add(new(sortZ, layer, RenderType.Sprite, Sprites.Count - 1));
        }
    }

    public void AddGeometry(RenderStyle style, RenderLayer layer, float sortZ, SKRect rect, GeometryType type, params Vector2[] extraParams)
    {
        lock (_lock)
        {
            Geometries.Add(new(style, rect, type, extraParams));
            RenderCommands.Add(new(sortZ, layer, RenderType.Geometry, Geometries.Count - 1));
        }
    }

    public void AddText(RenderStyle style, RenderLayer layer, float sortZ, string content, Vector2 position,params Vector2[] extraParams)
    {
        lock (_lock)
        {
            Texts.Add(new(style, content, position, extraParams));
            RenderCommands.Add(new(sortZ, layer, RenderType.Text, Texts.Count - 1));
        }
    }

    public void SortCommand()
    {
        RenderCommands.Sort();
    }

    public void Clear()
    {
        RenderCommands.Clear();
        Sprites.Clear();
        Texts.Clear();
        Geometries.Clear();
    }
}