using System.Numerics;
using Arch.Core;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public interface IEntityBuilder
{
    void Create(World world, Vector2 position, int teamId);
}