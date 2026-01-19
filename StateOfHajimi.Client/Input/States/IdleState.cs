using System;
using Avalonia;
using Avalonia.Input;
using Serilog;
using StateOfHajimi.Client.Input.Core;
using StateOfHajimi.Core.Systems.Input.Commands;

namespace StateOfHajimi.Client.Input.States;

public class IdleState: InputStateBase
{
    
    private Point _rightMouseDownPos;
    private bool _isRightMouseDown;
    private bool _isSelectedOld;
    private Point _curPos => GameView.MousePosition;
    private const float PanSwitchThreshold = 50f;
    public override void Enter(InputController controller)
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
        // TODO 检查是否悬浮在可选中物体上方
        
        // 判断是否转移到拖拽地图状态
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
        Bridge.AddCommand(new NavigateCommand(pos, false, _isSelectedOld));
        _isSelectedOld = true;
    }
}