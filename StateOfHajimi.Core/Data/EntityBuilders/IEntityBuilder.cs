using System.Numerics;
using Arch.Core;
using StateOfHajimi.Core.Components.ProductComponents;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public interface IEntityBuilder
{
    void Create(World world, Vector2 position, int teamId,ref RallyPoint rally);
}