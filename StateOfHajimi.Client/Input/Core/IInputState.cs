using Avalonia.Input;
using StateOfHajimi.Client.Input.Core;

namespace StateOfHajimi.Client.Input.Core;

public interface IInputState
{
    void Enter(InputController controller);
    void Exit();
    void Update(float deltaTime);
    void OnPointerPressed(PointerPressedEventArgs e);
    void OnPointerMoved(PointerEventArgs e);
    void OnPointerReleased(PointerReleasedEventArgs e);
}