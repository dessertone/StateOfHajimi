
using Avalonia.Input;
using StateOfHajimi.Editor.Input.States;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Editor.Input;

public class EditorController:IController
{
    public IGameView GameView { get; set; }
    public IBridge Bridge { get; set; }
    public IInputState? State { get; set; }
    

    public void TransitionTo(IInputState newState)
    {
        State?.Exit();
        State = newState; 
        newState.Enter(this);
    }
    public void Update(float deltaTime) => State?.Update(deltaTime);
    public void Initialize(IGameView gameView, IBridge Bridge)
    {
        GameView = gameView;
        this.Bridge = Bridge;
        TransitionTo(new IdleState());
    }

    public void OnPointerPressed(PointerPressedEventArgs e) => State?.OnPointerPressed(e);
    public void OnPointerMoved(PointerEventArgs e) => State?.OnPointerMoved(e);
    public void OnPointerReleased(PointerReleasedEventArgs e) => State?.OnPointerReleased(e);
    public void OnKeyUp(KeyEventArgs e) => State?.OnKeyUp(e);
    public void OnKeyDown(KeyEventArgs e) => State?.OnKeyDown(e);
}