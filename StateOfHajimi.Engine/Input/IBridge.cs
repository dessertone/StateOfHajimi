using System.Numerics;
using Arch.Core;
using SkiaSharp;
using StateOfHajimi.Engine.Enums;
using StateOfHajimi.Engine.Input.Commands;

namespace StateOfHajimi.Engine.Input;

public interface IBridge
{
    public InputSnapshot CurSnapshot { get; init; }
    void UpdateMousePosition(Vector2 pos);
    public void SendCommand(GameCommand command);
    public void CaptureCurrentShot();
    bool IsHovering { get; set; }
    HoverType CursorHoverType { get; set; }
    Entity HoveredEntity { get; set; }
}