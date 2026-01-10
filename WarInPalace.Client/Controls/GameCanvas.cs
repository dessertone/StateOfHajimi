using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Serilog;
using WarInPalace.Client.Input;
using WarInPalace.Client.Input.Core;
using WarInPalace.Client.Input.States;
using WarInPalace.Client.Models.Enums;
using WarInPalace.Client.Utils;
using WarInPalace.Core;
using WarInPalace.Core.Components.MoveComponents;
using WarInPalace.Core.Components.Tags;
using WarInPalace.Core.Data;
using WarInPalace.Core.Enums;

namespace WarInPalace.Client.Controls;

public class GameCanvas : Control, IGameView
{
    #region 资源定义
    private readonly InputMap _inputMap = new();
    private static readonly Dictionary<TileType, IBrush> TileBrushes = new()
    {
        { TileType.Grass, Brushes.DarkGreen },
        { TileType.Dirt,  Brush.Parse("#5e4d33") }, 
        { TileType.Water, Brushes.DeepSkyBlue }, 
        { TileType.Wall,  Brushes.WhiteSmoke }        
    };
    private static readonly IBrush DefaultTileBrush = Brushes.Magenta;
    #endregion

    #region 核心属性
    /// <summary>
    /// 游戏引擎
    /// </summary>
    public GameEngine GameEngine
    {
        get => _gameEngine;
        set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
    }
    private GameEngine _gameEngine = new();
    /// <summary>
    /// 直接属性用于前端绑定
    /// </summary>
    public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
        nameof(GameEngine),
        o => o.GameEngine,
        (o, v) => o.GameEngine = v,
        defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// 摄像机
    /// </summary>
    private readonly GameCamera _camera = new();
    #endregion

    #region 循环与状态
    /// <summary>
    /// 决定游戏更新频率
    /// </summary>
    private DispatcherTimer _gameScheduler;



    /// <summary>
    /// 游戏计时器
    /// </summary>
    private readonly Stopwatch _gameTimer = new();

    /// <summary>
    /// 鼠标状态机
    /// </summary>
    private InputController _inputController;
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    public GameCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
    }
    
    
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _camera.ViewportSize = Bounds.Size;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var map = GameEngine.CurrentMap;
        _camera.ViewportSize = Bounds.Size;
        _camera.Position = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);
        
        MousePosition = new Point(Bounds.Width /2, Bounds.Height /2);
        _inputController = new InputController(this, GameEngine.Bridge);
        _inputController.TransitionTo(new IdleState());
        Log.Information("Input state machine started");
        _gameScheduler = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameScheduler.Tick += GameTick;
        _gameScheduler.Start();
        Log.Information("Game scheduler started");
    }

    
    /// <summary>
    /// 每帧游戏更新逻辑
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameTick(object? sender, EventArgs e)
    {
        var deltaTime = (float)_gameTimer.Elapsed.TotalSeconds;
        _gameTimer.Restart();
        
        _inputController.Update(deltaTime);

        GameEngine.Update(deltaTime);
        
        if (GameEngine.CurrentMap != null)
        {
            var mapPixelWidth = GameEngine.CurrentMap.Width * GameEngine.CurrentMap.TileSize;
            var mapPixelHeight = GameEngine.CurrentMap.Height * GameEngine.CurrentMap.TileSize;
            _camera.ClampToMap(mapPixelWidth, mapPixelHeight);
        }
        InvalidateVisual();
    }
    
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var zoomSpeed = 0.1f * _camera.Zoom;
        var delta = (float)e.Delta.Y * zoomSpeed;
        var mousePos = e.GetPosition(this);
        
        _camera.ZoomAt(delta, mousePos);
        
        InvalidateVisual();
    }
    
    // 处理键盘按键
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var action = _inputMap.GetAction(e.Key);
        switch (action)
        {
            case GameAction.ToggleFullscreen:
                ToggleFullScreen();
                break;
        }
    }

    // 全屏切换逻辑
    private void ToggleFullScreen()
    {
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            if (window.WindowState == WindowState.FullScreen)
            {
                
                window.WindowState = WindowState.Normal;
                window.SystemDecorations = SystemDecorations.Full;
            }
            else
            {
                
                window.WindowState = WindowState.FullScreen;
                window.SystemDecorations = SystemDecorations.None;
            }
        }
    }
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Black, Bounds); // 黑色背景
        RenderMap(context);
        base.Render(context);
        RenderEntities(context);
        RenderSelectionView(context, _isSelectingArea);
    }

    private static readonly QueryDescription _renderUnitsQuery = new QueryDescription()
        .WithAll<Position, Velocity, BodyCollider>();
    
    private static readonly QueryDescription _renderSelectionQuery = new QueryDescription()
        .WithAll<Position, BodyCollider, IsSelected>(); 


    private void RenderEntities(DrawingContext context)
    {

        GameEngine.GameWorld.Query(in _renderUnitsQuery, 
            (ref Entity entity, ref Position position, ref Velocity velocity, ref BodyCollider b) =>
            {
                var screenPos = _camera.WorldToScreen(position.Value);
                if (screenPos.X < -100 || screenPos.Y < -100 || 
                    screenPos.X > Bounds.Width + 100 || screenPos.Y > Bounds.Height + 100)
                    return;

                if(b.Type is BodyType.Circle)
                {
                    
                    context.DrawEllipse(Brushes.White, null, screenPos, b.Size.X, b.Size.X);
                }
                // 区分红蓝方，应该读取 TeamComponent
            });
        GameEngine.GameWorld.Query(in _renderSelectionQuery, 
            (ref Position position, ref BodyCollider b) => 
            {
                var screenPos = _camera.WorldToScreen(position.Value);
                if (screenPos.X < -100 || screenPos.Y < -100 || 
                    screenPos.X > Bounds.Width + 100 || screenPos.Y > Bounds.Height + 100)
                    return;
                var pen = new Pen(Brushes.LightGreen, 2);
                context.DrawEllipse(null, pen, screenPos, b.Size.X + 2, b.Size.X + 2);
            });
    }

    private void RenderMap(DrawingContext context)
    {
        var map = GameEngine.CurrentMap;
        if (map == null) return;
        
        var tileSize = map.TileSize;
        var halfViewW = Bounds.Width / 2.0;
        var halfViewH = Bounds.Height / 2.0;
        
        var worldLeft = _camera.Position.X - (halfViewW / _camera.Zoom);
        var worldTop = _camera.Position.Y - (halfViewH / _camera.Zoom);

        var startCol = (int)Math.Floor(worldLeft / tileSize);
        var startRow = (int)Math.Floor(worldTop / tileSize);

        var renderCols = (int)Math.Ceiling(Bounds.Width / _camera.Zoom / tileSize) + 1;
        var renderRows = (int)Math.Ceiling(Bounds.Height / _camera.Zoom / tileSize) + 1;

        var endCol = startCol + renderCols;
        var endRow = startRow + renderRows;

        startCol = Math.Max(0, startCol);
        startRow = Math.Max(0, startRow);
        endCol = Math.Min(map.Width, endCol);
        endRow = Math.Min(map.Height, endRow);

        for (var y = startRow; y < endRow; y++)
        {
            for (var x = startCol; x < endCol; x++)
            {
                var tileType = map.GetTile(x, y);
                if (!TileBrushes.TryGetValue(tileType, out var brush)) brush = DefaultTileBrush;
                var worldPos = new Vector2(x * tileSize, y * tileSize);
                var screenPoint = _camera.WorldToScreen(worldPos);
                var rect = new Rect(
                    screenPoint.X, 
                    screenPoint.Y, 
                    (tileSize + 1)* _camera.Zoom, 
                    (tileSize + 1) * _camera.Zoom
                );
                context.FillRectangle(brush, rect);
            }
        }
    }

    #region 鼠标交互事件

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _inputController.OnPointerMoved(e);
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _inputController.OnPointerPressed(e);

    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _inputController.OnPointerReleased(e);

    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

    }
    #endregion


    private Point _selectStart;
    private Point _selectEnd;
    private bool _isSelectingArea;
    private void RenderSelectionView(DrawingContext context, bool isSelectingArea)
    {
        if (!isSelectingArea) return;
        var x = Math.Min(_selectStart.X, _selectEnd.X);
        var y = Math.Min(_selectStart.Y, _selectEnd.Y);
        var w = Math.Abs(_selectStart.X - _selectEnd.X);
        var h = Math.Abs(_selectStart.Y - _selectEnd.Y);
        var rect = new Rect(x, y, w, h);
        
        var brush = new SolidColorBrush(Colors.LightBlue, 0.3);
        var pen = new Pen(Brushes.LightBlue, 1.0);
        context.DrawRectangle(brush, pen, rect);
    }
    
    
    #region GameView实现

    public Rect ViewportSize => Bounds;
    public Point MousePosition { get; set; }
    
    public PointerPoint Pointer { get; set; }
    public Vector2 ScreenToWorld(Point pos) => _camera.ScreenToWorld(pos);
    public Point WorldToScreen(Vector2 pos) => _camera.WorldToScreen(pos);

    public (Point, PointerPoint) GetRelativeInfo(PointerEventArgs e) => (e.GetPosition(this),e.GetCurrentPoint(this));
    public Vector2 CameraPosition { get; }

    public void SetCursor(CursorType type)
    {
        
    }
    public void MoveCamera(Vector2 velocity) => _camera.Move(velocity);
    public void MoveCameraByPixel(Vector2 delta) => _camera.MoveCameraByPixel(delta);
    public bool ContainPoint(Point point) => Bounds.Contains(point);
    public void NotifyDrawSelection(Point start, Point end, bool isSelectingArea)
    {
        _isSelectingArea = isSelectingArea;
        _selectStart = start;
        _selectEnd = end;
    }
    #endregion

}