using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Arch.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using WarInPalace.Client.Input;
using WarInPalace.Client.Utils;
using WarInPalace.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Data;
using WarInPalace.Core.Enums;

namespace WarInPalace.Client.Controls;

public class GameCanvas : Control
{
    #region 资源定义
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
    private GameEngine _gameEngine = new();
    /// <summary>
    /// 输入状态桥接
    /// </summary>
    private readonly InputState _inputState = InputState.Instance;

    private readonly InputContext _inputContext;
    /// <summary>
    /// 当前鼠标状态
    /// </summary>
    private IInputState _state;
    /// <summary>
    /// 摄像机
    /// </summary>
    private readonly GameCamera _camera = new();
    /// <summary>
    /// 直接属性用于前端绑定
    /// </summary>
    public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
        nameof(GameEngine),
        o => o.GameEngine,
        (o, v) => o.GameEngine = v,
        defaultBindingMode: BindingMode.TwoWay);
    /// <summary>
    /// 封装
    /// </summary>
    public GameEngine GameEngine
    {
        get => _gameEngine;
        set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
    }
    #endregion

    #region 循环与状态
    /// <summary>
    /// 决定游戏更新频率
    /// </summary>
    private readonly DispatcherTimer _gameScheduler;
    /// <summary>
    /// 游戏计时器
    /// </summary>
    private readonly Stopwatch _gameTimer = new();
    
    // 框选状态
    public bool _isSelecting; 
    /// <summary>
    /// 存世界坐标
    /// </summary>
    public Vector2 _selectionStartWorld; 
    /// <summary>
    /// 存世界坐标
    /// </summary>
    public Vector2 _selectionEndWorld;   

    // 拖拽移动状态
    /// <summary>
    /// 是否拖拽
    /// </summary>
    public bool _isPanning;
    /// <summary>
    /// 存屏幕坐标，用于计算差值
    /// </summary>
    public Point _lastMousePosition;

    public Point _startMousePosition;

    // 边缘移动配置
    /// <summary>
    /// 鼠标距离边缘多少像素开始移动
    /// </summary>
    private const double EdgeThreshold = 20.0; 
    /// <summary>
    /// 移动速度
    /// </summary>
    private const float CameraScrollSpeed = 500.0f; 
    /// <summary>
    /// 鼠标实时位置
    /// </summary>
    private Point _currentMousePosition; 
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    public GameCanvas()
    {
        ClipToBounds = true;
        Focusable = true;
        _state = new IdleState();
        _inputContext = new InputContext(_camera, this, s =>
        {
            _state.Exit();
            _state = s;
            Console.WriteLine($"State Changed to: {_state.GetType().Name}");
        });
        _state.Enter(_inputContext);
        _gameScheduler = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameScheduler.Tick += GameTick;
        _gameScheduler.Start();
    }

    /// <summary>
    /// 初始化初始指针
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var map = GameEngine.CurrentMap;
        _camera.ViewportSize = Bounds.Size;
        _camera.Position = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);
        _currentMousePosition = new Point(Bounds.Width /2, Bounds.Height /2);
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
        
        _state.Update(deltaTime);
        UpdateCamera(deltaTime);

        GameEngine.Update(deltaTime);

        InvalidateVisual();
    }

    /// <summary>
    /// 处理摄像机的边缘移动逻辑
    /// </summary>
    private void UpdateCamera(float deltaTime)
    {
        _camera.ViewportSize = Bounds.Size;
        if (_isPanning) return;

        var movement = Vector2.Zero;
        if (Bounds.Contains(_currentMousePosition))
        {
            // 向左
            if (_currentMousePosition.X < EdgeThreshold)
                movement.X -= 1;
            // 向右
            else if (_currentMousePosition.X > Bounds.Width - EdgeThreshold)
                movement.X += 1;

            // 向上
            if (_currentMousePosition.Y < EdgeThreshold)
                movement.Y -= 1;
            // 向下
            else if (_currentMousePosition.Y > Bounds.Height - EdgeThreshold)
                movement.Y += 1;
        }
        if (movement == Vector2.Zero) return;
        movement = Vector2.Normalize(movement) * CameraScrollSpeed * deltaTime;
        _camera.Position += movement;
        // _camera.ClampToMap(GameEngine.CurrentMap.Width * GameEngine.CurrentMap.TileSize, GameEngine.CurrentMap.Height * GameEngine.CurrentMap.TileSize);
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Black, Bounds); // 黑色背景
        
        RenderMap(context);
        base.Render(context);


        GameEngine.GameWorld.Query(new QueryDescription().WithAll<Position, Velocity, BodyCollider>(), 
            (ref Position position, ref Velocity velocity, ref BodyCollider b) =>
        {
            var screenPos = _camera.WorldToScreen(position.Value);
            
            if (screenPos.X < -100 || screenPos.Y < -100 || 
                screenPos.X > Bounds.Width + 100 || screenPos.Y > Bounds.Height + 100)
                return;
            if(b.Type is BodyType.Circle)
            {
                context.DrawEllipse(new SolidColorBrush(Colors.White), null, screenPos, b.Size.X, b.Size.X);
            }

        });

        if (_isSelecting)
        {
            var p1 = _camera.WorldToScreen(_selectionStartWorld);
            var p2 = _camera.WorldToScreen(_selectionEndWorld);

            var x = Math.Min(p1.X, p2.X);
            var y = Math.Min(p1.Y, p2.Y);
            var w = Math.Abs(p1.X - p2.X);
            var h = Math.Abs(p1.Y - p2.Y);

            var rect = new Rect(x, y, w, h);
            
            var brush = new SolidColorBrush(Colors.LightBlue, 0.3);
            var pen = new Pen(Brushes.LightBlue, 1.0);
            context.DrawRectangle(brush, pen, rect);
        }
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
                    tileSize * _camera.Zoom, 
                    tileSize * _camera.Zoom
                );
                
                context.FillRectangle(brush, rect);
            }
        }
    }

    #region 鼠标交互事件

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _state.OnPointerMoved(e);
        
        /*var currentPoint = e.GetCurrentPoint(this);
        _currentMousePosition = e.GetPosition(this); // 更新给边缘移动用

        if (currentPoint.Properties.IsRightButtonPressed)
        {
            _isPanning = !(Vector2.DistanceSquared(new Vector2((float)_startMousePosition.X, (float)_startMousePosition.Y),
                new Vector2((float)_currentMousePosition.X, (float)_currentMousePosition.Y)) < 1024f);
        
            if (_isPanning)
            {
                Cursor = new Cursor(StandardCursorType.Hand);
                var deltaX = _currentMousePosition.X - _lastMousePosition.X;
                var deltaY = _currentMousePosition.Y - _lastMousePosition.Y;
                _camera.Position -= new Vector2((float)deltaX, (float)deltaY);
            
                _lastMousePosition = _currentMousePosition;
            }
            else
            {
                var worldPos = _camera.ScreenToWorld(_currentMousePosition);
                _inputState.MousePosition = worldPos;
            }
        }

        if (_isSelecting)
        {
            _selectionEndWorld = _camera.ScreenToWorld(_currentMousePosition);
        }*/
        
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _state.OnPointerPressed(e);
        /*var point = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isSelecting = true;
            _selectionStartWorld = _camera.ScreenToWorld(pos);
            _selectionEndWorld = _selectionStartWorld; // 初始宽高为0

           
            _inputState.DragStartPosition = _selectionStartWorld;
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _inputState.IsRightMousePressed = true;
            _startMousePosition = pos;
            _lastMousePosition = pos;
            
        }*/
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _state.OnPointerReleased(e);
        /*var pos = e.GetPosition(this);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                _selectionEndWorld = _camera.ScreenToWorld(e.GetPosition(this));
                _inputState.DragEndPosition = _selectionEndWorld;
                _inputState.IsNewSelectionTriggered = true; 
            }
        }
        else if (e.InitialPressMouseButton == MouseButton.Right)
        {
            if (_isPanning)
            {
                _isPanning = false;
                Cursor = Cursor.Default;
                return;
            }
            if (!_inputState.IsRightMousePressed) return;
            _inputState.IsRightMousePressed = false;
            _inputState.IsMoveActive = true;
        }*/
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _currentMousePosition = new Point(-1000, -1000);
    }
    #endregion
}