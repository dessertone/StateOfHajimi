using System;
using System.Resources;
using Avalonia;
using Avalonia.Input;
using Serilog;
using WarInPalace.Client.Input.Core;
using WarInPalace.Client.Input.States;
using WarInPalace.Core.Input;

namespace WarInPalace.Client.Input;

public class SelectState: InputStateBase
{
    private readonly Point _startPos;
    private Point _curPos => GameView.MousePosition;
    private const float MoveThreshold = 16f;
    public SelectState(Point startPos)
    {
        _startPos = startPos;  
    }
    
    public override void Enter(InputController controller)
    {
        base.Enter(controller);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
    }

    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // 没有事件
        GameView.NotifyDrawSelection(_startPos, _curPos, false);
        Controller.TransitionTo(new IdleState());
    }

    public override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if(Point.Distance(_startPos, _curPos) >= MoveThreshold)
            GameView.NotifyDrawSelection(_startPos, _curPos, true);
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            var startWorld = GameView.ScreenToWorld(_startPos);
            var endWorld = GameView.ScreenToWorld(_curPos);

            Log.Debug($"Selection command activated, position from {startWorld}  to {endWorld}");
            Bridge.AddCommand(new SelectCommand(startWorld, endWorld, false));
            GameView.NotifyDrawSelection(_startPos, _curPos, false);
            Controller.TransitionTo(new IdleState());
        }
    }
}