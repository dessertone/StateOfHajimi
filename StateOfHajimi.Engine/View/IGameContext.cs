using Arch.Core;
using StateOfHajimi.Engine.Data;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Rendering;
using StateOfHajimi.Engine.Rendering.RenderItems;

namespace StateOfHajimi.Engine.View;

public interface IGameContext
{
    public TileMap Map { get; set; }
    public RenderContext RenderContext { get; set; }
    public World GameWorld { get; init; }
    public IBridge Bridge { get; init; }
    void Update(float deltaTime);
}