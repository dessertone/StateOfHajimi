using System;
using System.Diagnostics;
using System.Numerics;
using Arch.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using WarInPalace.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Data;
using WarInPalace.Core.Enums;

namespace WarInPalace.Client.Controls;

public class GameCanvas:Control
{

    private GameEngine _gameEngine = new();

    private readonly InputState _inputState = InputState.Instance;
    public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
        nameof(GameEngine),
        o => o.GameEngine,
        (o, v) => o.GameEngine = v,
        defaultBindingMode: BindingMode.TwoWay);

    public GameEngine GameEngine
    {
        get => _gameEngine;
        set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
    }
    
    
    private readonly DispatcherTimer _gameScheduler;
    private readonly Stopwatch _gameTimer = new ();
    private bool _isDragging;
    public GameCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
        _gameScheduler = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameScheduler.Tick += GameTick;
        _gameScheduler.Start();
    }

    private void GameTick(object? sender, EventArgs e)
    {
        var deltaTime = (float)_gameTimer.Elapsed.TotalMilliseconds;
        _gameTimer.Restart();
        GameEngine.Update(deltaTime);
        InvalidateVisual();
    }


    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Transparent, Bounds);
        base.Render(context);
        GameEngine.GameWorld.Query(new QueryDescription().WithAll<Position, Velocity, BodyCollider>(), (ref Position position,
          ref Velocity velocity, ref BodyCollider b) =>
        {
            if(b.Type is BodyType.Circle)
                context.DrawEllipse(new SolidColorBrush(Colors.White), null, new Point(position.Value.X, position.Value.Y), b.Size.X,b.Size.X);
            else if (b.Type is BodyType.AABB)
                context.DrawRectangle(new SolidColorBrush(Colors.Brown), null, new Rect(position.Value.X, position.Value.Y, b.Size.X, b.Size.Y));
        });
        
    }


    #region 鼠标交互事件

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            var pos = e.GetPosition(this);
            _inputState.IsRightMousePressed = true;
            _inputState.MousePosition = new Vector2((float)pos.X,(float)pos.Y);
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var pos = e.GetPosition(this);
            
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var pos = e.GetPosition(this);
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            _inputState.IsRightMousePressed = true;
            _inputState.MousePosition = new Vector2((float)pos.X,(float)pos.Y);
        }
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
           _inputState.DragStartPosition = new Vector2((float)pos.X,(float)pos.Y);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    
        // 获取当前松开的是哪个键
        var button = e.InitialPressMouseButton;

        // 获取位置
        var pos = e.GetPosition(this);
        var vecPos = new Vector2((float)pos.X, (float)pos.Y);

        if (button == MouseButton.Left)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _inputState.DragEndPosition = vecPos;
                _inputState.IsNewSelectionTriggered = true; 
            }
        }
        else if (button == MouseButton.Right)
        {
            _inputState.IsRightMousePressed = false;
        }
        else if (button == MouseButton.Middle)
        {
            
        }
    }
    #endregion
}