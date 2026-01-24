using Avalonia;
using Avalonia.Input;
using Serilog;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Input.Commands;
using StateOfHajimi.Engine.Input.Core;

namespace StateOfHajimi.Editor.Input.States;

public class IdleState: InputStateBase
{
    
    private Point _rightMouseDownPos;
    private bool _isRightMouseDown;
    private bool _isSelectedOld;
    private bool _isDefault;
    private Point _curPos => GameView.MousePosition;
    
    private const float PanSwitchThreshold = 50f;
    public override void Enter(IController controller)
    {
        base.Enter(controller);
        GameView.SetCursor(CursorType.Default);
    }
    

    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var (pos, point) = GameView.GetRelativeInfo(e);
        if (point.Properties.IsLeftButtonPressed)
        {
            Controller.TransitionTo(new SelectState(pos));
        }
        
        if (point.Properties.IsRightButtonPressed)
        {
            _isRightMouseDown = true;
            _rightMouseDownPos = pos;
        }
    }
    

    public override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        // 检查是否悬浮在可选中物体上方
        if (Bridge.IsHovering)
        {
            if (Bridge.CursorHoverType == HoverType.Friend)
            {
                GameView.SetCursor(CursorType.Hand);
                _isDefault = false;
            }
            if (Bridge.CursorHoverType == HoverType.Opponent)
            {
                GameView.SetCursor(CursorType.Attack);
                _isDefault = false;
            }
        }
        else
        {
            if (!_isDefault)
            {
                GameView.SetCursor(CursorType.Default);
                _isDefault = true;
            }
        }
        if (!_isRightMouseDown) 
        {
            return;
        }
        if (Point.Distance(_rightMouseDownPos, GameView.MousePosition) >= PanSwitchThreshold)
        {
            _isSelectedOld = false;
            Controller.TransitionTo(new PanState(GameView.MousePosition));
        }
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isRightMouseDown = false;
        var pos = GameView.ScreenToWorld(_curPos);
        Bridge.SendCommand(new NavigateCommand(pos, false, _isSelectedOld));
        _isSelectedOld = true;
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        var action = InputMap.GetAction(e.Key, KeyModifiers.None, HoldKeys);
        switch (action)
        {
            case GameAction.ToggleDebug:
                Log.Information("Switching Debug mode");
                Bridge.SendCommand(new SwitchDebugCommand(true));
                break;
            case GameAction.ToggleFullscreen:
                GameView.ToggleFullScreen();
                break;
        }
        base.OnKeyDown(e);
    }
    
}