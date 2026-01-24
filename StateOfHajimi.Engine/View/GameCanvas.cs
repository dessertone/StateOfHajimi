using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Serilog;
using SkiaSharp;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Rendering;
using StateOfHajimi.Engine.Rendering.Renderers;
using StateOfHajimi.Engine.Utils;

namespace StateOfHajimi.Engine.View;

    public class GameCanvas : Control, IGameView
    {
        #region 核心属性

        public IGameContext GameContext
        {
            get => _gameContext;
            set => SetAndRaise(GameContextProperty, ref _gameContext, value);
        }

        private IGameContext _gameContext;
        public static readonly DirectProperty<GameCanvas, IGameContext> GameContextProperty = 
            AvaloniaProperty.RegisterDirect<GameCanvas, IGameContext>(
                nameof(IGameContext), o => o.GameContext, (o, v) => o.GameContext = v);
        
        
        private IController _inputController;
        public static readonly DirectProperty<GameCanvas, IController> InputControllerProperty = AvaloniaProperty.RegisterDirect<GameCanvas, IController>(
            nameof(InputController), o => o.InputController, (o, v) => o.InputController = v);
        public IController InputController
        {
            get => _inputController;
            set => SetAndRaise(InputControllerProperty, ref _inputController, value);
        }
        private IRenderer _worldRenderer;
        public static readonly DirectProperty<GameCanvas, IRenderer> WorldRendererProperty = AvaloniaProperty.RegisterDirect<GameCanvas, IRenderer>(
            nameof(WorldRenderer), o => o.WorldRenderer, (o, v) => o.WorldRenderer = v);
        public IRenderer WorldRenderer
        {
            get => _worldRenderer;
            set => SetAndRaise(WorldRendererProperty, ref _worldRenderer, value);
        }
        
        private readonly GameCamera _camera = new();
        
        #endregion

        #region 循环控制
        private DispatcherTimer _gameScheduler;
        private readonly Stopwatch _gameTimer = new();
        #endregion

        #region 选中框状态 (UI Layer)
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
            
            var map = GameContext.Map;
            _camera.Position = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);
            _camera.ViewportSize = Bounds.Size;
            _camera.Zoom = 0.85f;
            
            _worldRenderer.Initialize(GameContext.RenderContext.GetForegroundRenderFrame());

            _inputController.Initialize(this, GameContext.Bridge);
            
            MousePosition = new Point(Bounds.Width / 2, Bounds.Height / 2);
            
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
            
            InvalidateVisual();
        }

        private void GameTick(object? sender, EventArgs e)
        {
            // 更新边界
            
            // 计算实际时间
            var deltaTime = (float)_gameTimer.Elapsed.TotalSeconds;
            _gameTimer.Restart();
            
            // 鼠标状态更新
            _inputController.Update(deltaTime);
            
            // 游戏逻辑更新
            GameContext.Update(deltaTime);
            
            // 获取当前渲染帧
            GameContext.RenderContext.SwapFrame();
            _worldRenderer.Frame = GameContext.RenderContext.GetForegroundRenderFrame();
            _worldRenderer.Frame.Bounds = GetWorldBounds(Bounds, _camera.Zoom, _camera.Position);
            
            // 相机裁剪
            var mapW = GameContext.Map.Width * GameContext.Map.TileSize;
            var mapH = GameContext.Map.Height * GameContext.Map.TileSize;
            _camera.ClampToMap(mapW, mapH);
            
            // 渲染重绘
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            
            context.FillRectangle(Brushes.Black, Bounds); 
            context.Custom(new GameDrawOperation(_worldRenderer, _camera.Zoom, _camera.Position, Bounds));

            // UI
            RenderSelectionUI(context);         
            base.Render(context);
        }
        /// <summary>
        /// 获取渲染上下文
        /// </summary>
        /// <param name="bounds">屏幕边界</param>
        /// <param name="zoom">缩放</param>
        /// <param name="cameraPos">相机位置</param>
        /// <returns></returns>
        private SKRect GetWorldBounds(Rect bounds, float zoom, Vector2 cameraPos)
        {
            var viewH = bounds.Height / zoom;
            var viewW = bounds.Width / zoom;
            var viewTop = cameraPos.Y - viewH / 2;
            var viewLeft = cameraPos.X - viewW / 2;
            
            return new SKRect
            (
                (float)viewLeft - 200f, 
                (float)viewTop - 200f, 
                (float)(viewLeft + viewW + 200f), 
                (float)(viewTop + viewH + 200f)
            );
        }
        private void RenderSelectionUI(DrawingContext context)
        {
            if (!_isSelectingArea) return;
            
            var x = Math.Min(_selectStart.X, _selectEnd.X);
            var y = Math.Min(_selectStart.Y, _selectEnd.Y);
            var w = Math.Abs(_selectStart.X - _selectEnd.X);
            var h = Math.Abs(_selectStart.Y - _selectEnd.Y);
            var rect = new Rect(x, y, w, h);

            var brush = new SolidColorBrush(Colors.Goldenrod, 0.2);
            var pen = new Pen(Brushes.Gray, 0.8);
            context.DrawRectangle(brush, pen, rect);
        }

        #region 输入事件
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var zoomSpeed = 0.1f * _camera.Zoom;
            _camera.ZoomAt((float)e.Delta.Y * zoomSpeed, e.GetPosition(this));
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _inputController.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _inputController.OnKeyUp(e);
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
        
        #region IGameView 接口实现
        public Rect ViewportSize => Bounds;
        public Point MousePosition { get; set; }
        public PointerPoint Pointer { get; set; }
        public Vector2 ScreenToWorld(Point pos) => _camera.ScreenToWorld(pos);
        public (Point, PointerPoint) GetRelativeInfo(PointerEventArgs e) => (e.GetPosition(this), e.GetCurrentPoint(this));

        public void SetCursor(CursorType type) => Cursor = AssetsManager.GetCursor(type) ?? AssetsManager.GetCursor(CursorType.Default);
        public void MoveCamera(Vector2 velocity) => _camera.Move(velocity);
        public void MoveCameraByPixel(Vector2 delta) => _camera.MoveCameraByPixel(delta);
        public bool ContainPoint(Point point) => Bounds.Contains(point);
        
        public void NotifyDrawSelection(Point start, Point end, bool isSelectingArea)
        {
            _isSelectingArea = isSelectingArea;
            _selectStart = start;
            _selectEnd = end;
        }
        public void ToggleFullScreen()
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
        #endregion

    }