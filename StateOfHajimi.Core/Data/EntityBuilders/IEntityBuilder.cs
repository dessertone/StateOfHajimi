using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.ProductComponents;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public interface IEntityBuilder
{
    ComponentType[] Archetype { get; }
    void Build(CommandBuffer buffer, Entity entity, Vector2 position, int teamId, ref RallyPoint rally);
}