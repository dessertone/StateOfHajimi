using Arch.Buffer;
using Arch.Core;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Utils.Attributes;
using StateOfHajimi.Engine.Input.Commands;

namespace StateOfHajimi.Core.Systems.Input.CommandHandlers;

[CommandType(nameof(SetRallyCommand))]
public class SetRallyCommandHandler:ICommandHandler
{
    private static readonly QueryDescription _factoryQuery = new QueryDescription()
        .WithAll<AutoProduction, IsSelected>()
        .WithNone<Disabled>();
    
    public void Handle(CommandBuffer buffer, GameCommand command, World world, float deltaTime)
    {
        if (command is not SetRallyCommand setRallyCommand) return;
        world.Query(in _factoryQuery, (ref AutoProduction prod) =>
        {
            prod.Rally.Target = setRallyCommand.position;
            prod.Rally.IsSet = true;
        });
    }
}