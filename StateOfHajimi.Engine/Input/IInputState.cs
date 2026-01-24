using Avalonia.Input;

namespace StateOfHajimi.Engine.Input;

public interface IInputState
{
    void Enter(IController controller);
    void Exit();
    void Update(float deltaTime);
    void OnPointerPressed(PointerPressedEventArgs e);
    void OnPointerMoved(PointerEventArgs e);
    void OnPointerReleased(PointerReleasedEventArgs e);
    void OnKeyUp(KeyEventArgs e);
    void OnKeyDown(KeyEventArgs e);
}