using Avalonia;
using Avalonia.Input;
using Serilog;
using StateOfHajimi.Engine.Input.Commands;
using StateOfHajimi.Engine.Input.Core;

namespace StateOfHajimi.Client.Input.States;

public class SelectState: InputStateBase
{
    private readonly Point _startPos;
    private Point _curPos => GameView.MousePosition;
    private const float MoveThreshold = 16f;
    public SelectState(Point startPos)
    {
        _startPos = startPos;  
    }
    
    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
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
            
            Bridge.SendCommand(new SelectCommand(startWorld, endWorld, false));
            GameView.NotifyDrawSelection(_startPos, _curPos, false);
            Controller.TransitionTo(new IdleState());
        }
    }
}