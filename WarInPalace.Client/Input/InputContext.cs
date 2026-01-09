using System;
using WarInPalace.Client.Controls;
using WarInPalace.Client.Utils;

namespace WarInPalace.Client.Input;

public class InputContext
{
     public GameCamera Camera { get; set; }
     public GameCanvas Canvas { get; set; }
     
     public InputBridge Bridge => Canvas.GameEngine.Bridge;
     
     
     private readonly Action<IInputState>? _onInputStateChanged;

     public InputContext(GameCamera camera, GameCanvas canvas, Action<IInputState> onInputStateChanged)
     {
          _onInputStateChanged = onInputStateChanged;
          Camera = camera;
          Canvas = canvas;
     }

     public void TransitionTo(IInputState newState)
     {
          newState.Enter(this);
          _onInputStateChanged?.Invoke(newState);
     }
}