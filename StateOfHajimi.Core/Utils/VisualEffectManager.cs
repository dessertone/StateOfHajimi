using System.Numerics;
using SkiaSharp;

namespace StateOfHajimi.Core.Utils;

public class VisualEffectManager
{
    private static readonly Lazy<VisualEffectManager> Lazy  =new ();
    
    public static VisualEffectManager Instance => Lazy.Value;

    private struct FloatingText
    {
        public string Text;
        public Vector2 Position;
        public float LifeTime;
        public float MaxLifeTime;
        public SKColor Color;
    }
    
    private struct LaserLine
    {
        public Vector2 Start;
        public Vector2 End;
        public float LifeTime;
        public SKColor Color;
    }

    private readonly List<FloatingText> _texts = new();
    private readonly List<LaserLine> _lasers = new();
    private readonly Lock _lock = new();

    private static readonly SKPaint _textPaint = new() 
    { 
        TextSize = 50, 
        IsAntialias = true, 
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
        TextAlign = SKTextAlign.Center 
    };
    
    private static readonly SKPaint _laserPaint = new() 
    { 
        StrokeWidth = 7, 
        IsAntialias = true,
        Style = SKPaintStyle.Stroke
    };
    

    public void SpawnDamageText(Vector2 pos, int damage)
    {
        lock(_lock)
        {
            _texts.Add(new FloatingText
            {
                Text = damage.ToString(),
                Position = pos,
                LifeTime = 1.0f,
                MaxLifeTime = 1.0f,
                Color = SKColors.OrangeRed
            });
        }
    }

    public void SpawnLaser(Vector2 start, Vector2 end)
    {
        lock(_lock)
        {
            _lasers.Add(new LaserLine
            {
                Start = start,
                End = end,
                LifeTime = 0.15f,
                Color = SKColors.Cyan
            });
        }
    }
    

    public void Update(float dt)
    {
        lock(_lock)
        {
            for (int i = _texts.Count - 1; i >= 0; i--)
            {
                var t = _texts[i];
                t.LifeTime -= dt;
                t.Position -= new Vector2(0, 30 * dt);
                _texts[i] = t;
                if (t.LifeTime <= 0) _texts.RemoveAt(i);
            }

            for (int i = _lasers.Count - 1; i >= 0; i--)
            {
                var l = _lasers[i];
                l.LifeTime -= dt;
                _lasers[i] = l;
                if (l.LifeTime <= 0) _lasers.RemoveAt(i);
            }
        }
    }
    

    public void Render(SKCanvas canvas)
    {
        lock(_lock)
        {
            foreach (var laser in _lasers)
            {
                _laserPaint.Color = laser.Color.WithAlpha((byte)(255 * (laser.LifeTime / 0.15f)));
                canvas.DrawLine(laser.Start.X, laser.Start.Y, laser.End.X, laser.End.Y, _laserPaint);
            }
            
            foreach (var text in _texts)
            {
                var alphaPct = text.LifeTime / text.MaxLifeTime;
                _textPaint.Color = text.Color.WithAlpha((byte)(255 * alphaPct));
                
                var oldStyle = _textPaint.Style;
                var oldColor = _textPaint.Color;
                
                _textPaint.Style = SKPaintStyle.Stroke;
                _textPaint.StrokeWidth = 2;
                _textPaint.Color = SKColors.Black.WithAlpha((byte)(200 * alphaPct));
                canvas.DrawText(text.Text, text.Position.X, text.Position.Y, _textPaint);
                
                _textPaint.Style = oldStyle; 
                _textPaint.Color = oldColor;
                _textPaint.StrokeWidth = 0;
                canvas.DrawText(text.Text, text.Position.X, text.Position.Y, _textPaint);
            }
        }
    }
}