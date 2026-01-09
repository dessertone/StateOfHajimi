using Avalonia.Input;

namespace WarInPalace.Client.Input;

public interface IInputState
{
    void Enter(InputContext context);
    void Exit();
    void Update(float deltaTime);
    void OnPointerPressed(PointerPressedEventArgs e);
    void OnPointerMoved(PointerEventArgs e);
    void OnPointerReleased(PointerReleasedEventArgs e);
}