using System.Numerics;
using Avalonia.Input;
using WarInPalace.Core.Input;

namespace WarInPalace.Client.Input;

public class SelectState:BaseInputState
{
    private Vector2 _startPos;
    public SelectState(Vector2 pos)
    {
        _startPos = pos;
    }

    public override void Up··date(float deltaTime)
    {
        base.Update(deltaTime);
    }

    public override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(Context?.Canvas);
        Context.Canvas._selectionEndWorld = Context.Camera.ScreenToWorld(pos);
    }

    public override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var pos = Context.Camera.ScreenToWorld(e.GetPosition(Context?.Canvas));
        Context.Bridge.AddCommand(new SelectCommand(_startPos, pos, false));
    }
}