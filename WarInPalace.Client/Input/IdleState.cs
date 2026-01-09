using System.Net.Mime;
using Avalonia.Input;
using WarInPalace.Core.Input;

namespace WarInPalace.Client.Input;

public class IdleState:BaseInputState
{

    private bool _isRightMouseDown;
    public override void Update(float deltaTime)
    {
    }

    public override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var pos = e.GetPosition(Context?.Canvas);
        var prop = e.GetCurrentPoint(Context?.Canvas).Properties;
        
        if (prop.IsLeftButtonPressed)
        {
            Context.Canvas._isSelecting = true;
            Context.TransitionTo(new SelectState(pos));
        }
        else if (prop.IsRightButtonPressed)
        {
            _isRightMouseDown = true;
            
        }
    }
    

    public override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
    }
}