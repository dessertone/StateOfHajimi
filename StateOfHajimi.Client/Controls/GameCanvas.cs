using System;
using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Serilog;
using StateOfHajimi.Client.Input;
using StateOfHajimi.Client.Input.Core;
using StateOfHajimi.Client.Input.States;
using StateOfHajimi.Client.Models.Enums;
using StateOfHajimi.Client.Rendering; // 引用渲染器命名空间
using StateOfHajimi.Client.Utils;
using StateOfHajimi.Core;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Client.Controls;

public class GameCanvas : Control, IGameView
{
    #region 核心属性
    // 游戏引擎
    public GameEngine GameEngine
    {
        get => _gameEngine;
        set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
    }
    private GameEngine _gameEngine = new();
    
    public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = 
        AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
            nameof(GameEngine), o => o.GameEngine, (o, v) => o.GameEngine = v);

    // 摄像机
    private readonly GameCamera _camera = new();
    
    // 输入管理
    private InputController _inputController;
    private readonly InputMap _inputMap = new();
    
    // 渲染器 (新!)
    private WorldRenderer _worldRenderer;
    #endregion

    #region 循环控制
    private DispatcherTimer _gameScheduler;
    private readonly Stopwatch _gameTimer = new();
    #endregion

    #region 选中框状态 (UI Layer)
    // 选中框属于 UI 层的逻辑，虽然可以在 WorldRenderer 画，但留在 View 层处理交互反馈也行
    // 这里演示将绘制逻辑也放进 View，或者你可以传给 Renderer
    private Point _selectStart;
    private Point _selectEnd;
    private bool _isSelectingArea;
    #endregion

    public GameCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await AssetsManager.InitializeAsync();
        
        if (GameEngine.CurrentMap != null)
        {
            var map = GameEngine.CurrentMap;
            _camera.Position = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);
        }
        _camera.ViewportSize = Bounds.Size;

        // 3. 初始化渲染器
        _worldRenderer = new WorldRenderer(GameEngine, _camera);

        // 4. 初始化输入
        MousePosition = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _inputController = new InputController(this, GameEngine.Bridge);
        _inputController.TransitionTo(new IdleState());

        // 5. 启动循环
        _gameTimer.Start();
        _gameScheduler = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameScheduler.Tick += GameTick;
        _gameScheduler.Start();
        
        Log.Information("Game Loop Started.");
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _camera.ViewportSize = e.NewSize;
        // 触发重绘
        InvalidateVisual();
    }

    private void GameTick(object? sender, EventArgs e)
    {
        var deltaTime = (float)_gameTimer.Elapsed.TotalSeconds;
        _gameTimer.Restart();

        // 1. 更新输入状态
        _inputController.Update(deltaTime);

        // 2. 更新游戏逻辑 (ECS)
        GameEngine.Update(deltaTime);

        // 3. 限制相机
        if (GameEngine.CurrentMap != null)
        {
            var mapW = GameEngine.CurrentMap.Width * GameEngine.CurrentMap.TileSize;
            var mapH = GameEngine.CurrentMap.Height * GameEngine.CurrentMap.TileSize;
            _camera.ClampToMap(mapW, mapH);
        }

        // 4. 请求重绘
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // 背景
        context.FillRectangle(Brushes.Black, Bounds);

        // 核心游戏画面 (委托给 WorldRenderer)
        _worldRenderer?.Render(context, Bounds);

        // UI 层绘制 (比如鼠标拖拽框，这个属于 View 的瞬时状态，可以不放进 WorldRenderer)
        RenderSelectionUI(context);
        
        // 调试信息 (FPS等) 可以画在这里
        base.Render(context);
    }

    private void RenderSelectionUI(DrawingContext context)
    {
        if (!_isSelectingArea) return;
        
        var x = Math.Min(_selectStart.X, _selectEnd.X);
        var y = Math.Min(_selectStart.Y, _selectEnd.Y);
        var w = Math.Abs(_selectStart.X - _selectEnd.X);
        var h = Math.Abs(_selectStart.Y - _selectEnd.Y);
        var rect = new Rect(x, y, w, h);

        var brush = new SolidColorBrush(Colors.LightGreen, 0.3);
        var pen = new Pen(Brushes.LightGreen, 1.0);
        context.DrawRectangle(brush, pen, rect);
    }

    #region 输入事件转发
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var zoomSpeed = 0.1f * _camera.Zoom;
        _camera.ZoomAt((float)e.Delta.Y * zoomSpeed, e.GetPosition(this));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var action = _inputMap.GetAction(e.Key);
        if (action == GameAction.ToggleFullscreen)
        {
            ToggleFullScreen();
        }
        // 其他按键逻辑可以交给 InputController 或者这里处理
    }

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
    #endregion

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

    #region IGameView 接口实现
    public Rect ViewportSize => Bounds;
    public Point MousePosition { get; set; }
    public PointerPoint Pointer { get; set; }
    public Vector2 CameraPosition => _camera.Position;
    
    public Vector2 ScreenToWorld(Point pos) => _camera.ScreenToWorld(pos);
    public Point WorldToScreen(Vector2 pos) => _camera.WorldToScreen(pos);
    public (Point, PointerPoint) GetRelativeInfo(PointerEventArgs e) => (e.GetPosition(this), e.GetCurrentPoint(this));
    
    public void SetCursor(CursorType type) { /* 设置鼠标光标样式 */ }
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