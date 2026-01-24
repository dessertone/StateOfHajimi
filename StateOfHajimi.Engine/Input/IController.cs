using Avalonia.Input;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Engine.Input;

public interface IController
{
    public IGameView GameView { get; set; }
    public IInputState? State { get; set; }
    
    public IBridge Bridge { get; set; }
    public void TransitionTo(IInputState newState);
    public void Update(float deltaTime);
    public void Initialize(IGameView gameView, IBridge Bridge);
    void OnPointerPressed(PointerPressedEventArgs e);
    void OnPointerMoved(PointerEventArgs e);
    void OnPointerReleased(PointerReleasedEventArgs e);
    void OnKeyUp(KeyEventArgs e);
    void OnKeyDown(KeyEventArgs e);
}