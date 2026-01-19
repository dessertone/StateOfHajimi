using Arch.Buffer;
using Arch.Core;

namespace StateOfHajimi.Core.Data.Builders.Bases;

public interface IEntityBuilder
{
    ComponentType[] Archetype { get; }
    void Build(CommandBuffer buffer, Entity entity,ref BuildContext context);
}