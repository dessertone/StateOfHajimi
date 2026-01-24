using System.Numerics;
using StateOfHajimi.Engine.Enums;

namespace StateOfHajimi.Engine.Rendering.RenderItems;

public record struct RenderText(RenderStyle Style,string Content, Vector2 Position, Vector2[] ExtraParams);