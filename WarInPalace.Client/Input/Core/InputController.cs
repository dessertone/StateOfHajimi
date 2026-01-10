using System;
using Avalonia.Input;
using Serilog;

namespace WarInPalace.Client.Input.Core;

public class InputController
{
    public IGameView GameView { get; init; }
    public InputBridge InputBridge { get; init; }
    
    private InputStateBase? _curState;

    public InputController(IGameView gameView, InputBridge inputBridge)
    {
        GameView = gameView;
        InputBridge = inputBridge;
    }

    public void TransitionTo(InputStateBase newState)
    {
        _curState?.Exit();
        _curState = newState; 
        newState.Enter(this);
    }
    
    public void Update(float deltaTime) => _curState?.Update(deltaTime);
    public void OnPointerPressed(PointerPressedEventArgs e) => _curState?.OnPointerPressed(e);
    public void OnPointerMoved(PointerEventArgs e) => _curState?.OnPointerMoved(e);
    public void OnPointerReleased(PointerReleasedEventArgs e) => _curState?.OnPointerReleased(e);
}