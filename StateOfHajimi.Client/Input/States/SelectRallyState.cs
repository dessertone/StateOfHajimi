using Avalonia.Input;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Input.Commands;
using StateOfHajimi.Engine.Input.Core;

namespace StateOfHajimi.Client.Input.States;

public class SelectRallyState:InputStateBase
{
    public override void Enter(IController controller)
    {
        base.Enter(controller);
        GameView.SetCursor(CursorType.Flag);
    }

    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton.Equals(MouseButton.Left))
        {
            Bridge.SendCommand(new SetRallyCommand(GameView.ScreenToWorld(GameView.MousePosition)));
        }
        Controller.TransitionTo(new IdleState());
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if(e.Key == Key.Escape)
            Controller.TransitionTo(new IdleState());
    }
}