using System.Numerics;

namespace WarInPalace.Core.Data;

public class InputState
{
    private static readonly Lazy<InputState> _lazy =  new (() => new InputState());

    public static InputState Instance { get; } = _lazy.Value;
    
    private InputState(){}
    
    public Vector2 MousePosition { get; set; }
    
    public bool IsRightMousePressed { get; set; }
    public bool IsMoveActive { get; set; }
    
    public Vector2 DragStartPosition { get; set; }
    
    public Vector2 DragEndPosition { get; set; }
    public bool IsNewSelectionTriggered { get; set; }
}