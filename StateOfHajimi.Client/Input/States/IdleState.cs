using System;
using Avalonia;
using Avalonia.Input;
using Serilog;
using StateOfHajimi.Client.Input.Core;
using StateOfHajimi.Client.Utils;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Systems.Input.Commands;

namespace StateOfHajimi.Client.Input.States;

public class IdleState: InputStateBase
{
    
    private Point _rightMouseDownPos;
    private bool _isRightMouseDown;
    private bool _isSelectedOld;
    private bool _isDefault;
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
                GameView.SetCursor(CursorType.Flag);
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
        Bridge.AddCommand(new NavigateCommand(pos, false, _isSelectedOld));
        _isSelectedOld = true;
    }
}