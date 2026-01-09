using Avalonia.Input;

namespace WarInPalace.Client.Input;

public abstract class BaseInputState:IInputState
{
    protected InputContext? Context { get; private set; }
    public virtual void Enter(InputContext context) => Context = context;
    public virtual void Exit() { }
    public virtual void Update(float deltaTime) { }
    public virtual void OnPointerPressed(PointerPressedEventArgs e) { }
    public virtual void OnPointerMoved(PointerEventArgs e) { }
    public virtual void OnPointerReleased(PointerReleasedEventArgs e) { }
}