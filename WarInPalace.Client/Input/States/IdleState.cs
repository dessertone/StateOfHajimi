using System;
using Avalonia;
using Avalonia.Input;
using Serilog;
using WarInPalace.Client.Input.Core;
using WarInPalace.Core.Input;
using WarInPalace.Core.Input.Commands;

namespace WarInPalace.Client.Input.States;

public class IdleState: InputStateBase
{
    
    private Point _rightMouseDownPos;
    private bool _isRightMouseDown;
    private Point _curPos => GameView.MousePosition;
    private const float PanSwitchThreshold = 200f;
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
            Controller.TransitionTo(new PanState(GameView.MousePosition));
        }
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isRightMouseDown = false;
        var pos = GameView.ScreenToWorld(_curPos);
        Log.Debug($"Navigate command activated, position at {pos}");
        Bridge.AddCommand(new NavigateCommand(pos, false));
    }
}