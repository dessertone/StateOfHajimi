using Avalonia.Input;
using WarInPalace.Client.Input.Core;

namespace WarInPalace.Client.Input.Core;

public interface IInputState
{
    void Enter(InputController controller);
    void Exit();
    void Update(float deltaTime);
    void OnPointerPressed(PointerPressedEventArgs e);
    void OnPointerMoved(PointerEventArgs e);
    void OnPointerReleased(PointerReleasedEventArgs e);
}