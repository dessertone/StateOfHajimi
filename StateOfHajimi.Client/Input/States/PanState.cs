using System.Numerics;
using Avalonia;
using Avalonia.Input;
using StateOfHajimi.Client.Input.Core;

namespace StateOfHajimi.Client.Input.States;

public class PanState: InputStateBase
{
    private Point _lastPos;
    private Point _curPos => GameView.MousePosition;
    
    public PanState(Point lastPos)
    {
        _lastPos = lastPos;
    }


    public override void Update(float deltaTime)
    {
        
    }

    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Controller.TransitionTo(new IdleState());
    }

    public override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var deltaX = _curPos.X - _lastPos.X;
        var deltaY = _curPos.Y - _lastPos.Y;
        GameView.MoveCameraByPixel(new Vector2(- (float)deltaX, - (float)deltaY));
            
        _lastPos = _curPos;
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Controller.TransitionTo(new IdleState());
    }
}