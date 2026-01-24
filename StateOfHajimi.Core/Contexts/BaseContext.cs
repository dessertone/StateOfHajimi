using Arch.Core;
using StateOfHajimi.Engine.Data;
using StateOfHajimi.Engine.Input;
using StateOfHajimi.Engine.Rendering.RenderItems;
using StateOfHajimi.Engine.View;

namespace StateOfHajimi.Core.Contexts;

public class BaseContext:IGameContext
{
    public TileMap Map { get; set; }
    public RenderContext RenderContext { get; set; }
    public World GameWorld { get; init; }
    public IBridge Bridge { get; init; }
    public void Update(float deltaTime)
    {
        throw new NotImplementedException();
    }
}