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
    using StateOfHajimi.Client.Input.Core;
    using StateOfHajimi.Client.Input.States;
    using StateOfHajimi.Client.Models.Enums;
    using StateOfHajimi.Client.Rendering; 
    using StateOfHajimi.Client.Utils;
    using StateOfHajimi.Core;

    namespace StateOfHajimi.Client.Controls;

    public class GameCanvas : Control, IGameView
    {
        #region 核心属性

        public GameEngine GameEngine
        {
            get => _gameEngine;
            set => SetAndRaise(GameEngineProperty, ref _gameEngine, value);
        }
        private GameEngine _gameEngine = new();
        
        public static readonly DirectProperty<GameCanvas, GameEngine> GameEngineProperty = 
            AvaloniaProperty.RegisterDirect<GameCanvas, GameEngine>(
                nameof(GameEngine), o => o.GameEngine, (o, v) => o.GameEngine = v);

        private readonly GameCamera _camera = new();

        private InputController _inputController;
        private readonly InputMap _inputMap = new();
        

        private IRenderer _worldRenderer;
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
            
            
            
            
            if (GameEngine.CurrentMap != null)
            {
                var map = GameEngine.CurrentMap;
                _camera.Position = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);
            }
            _camera.ViewportSize = Bounds.Size;
            
            _worldRenderer = new SkiaWorldRenderer(GameEngine);
            _worldRenderer.Initialize();
            
            MousePosition = new Point(Bounds.Width / 2, Bounds.Height / 2);
            _inputController = new InputController(this, GameEngine.Bridge);
            _inputController.TransitionTo(new IdleState());
            
            _gameTimer.Start();
            _gameScheduler = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(15)
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
            var deltaTime = (float)_gameTimer.Elapsed.TotalSeconds;
            _gameTimer.Restart();

            
            _inputController.Update(deltaTime);

            
            GameEngine.Update(deltaTime);
            
            if (GameEngine.CurrentMap != null)
            {
                var mapW = GameEngine.CurrentMap.Width * GameEngine.CurrentMap.TileSize;
                var mapH = GameEngine.CurrentMap.Height * GameEngine.CurrentMap.TileSize;
                _camera.ClampToMap(mapW, mapH);
            }
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.Black, Bounds);
            if(_worldRenderer != null)
                context.Custom(new GameDrawOperation(_worldRenderer, _camera.Zoom, _camera.Position, Bounds));

            // UI
            RenderSelectionUI(context);         
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
            var action = _inputMap.GetAction(e.Key);
            if (action == GameAction.ToggleFullscreen)
            {
                ToggleFullScreen();
            }
            
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