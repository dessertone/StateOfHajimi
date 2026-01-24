using System.Reflection;
using Serilog;
using StateOfHajimi.Core.Data.Builders.Bases;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Systems.Input.CommandHandlers;
using StateOfHajimi.Core.Utils.FormationGenerators;

namespace StateOfHajimi.Core.Utils.Attributes;

public static class AttributeHelper
{
    public static readonly Dictionary<string, ICommandHandler> CommandHandlers = new();
    public static readonly Dictionary<FormationType, IFormation> Strategies = new();
    public static readonly Dictionary<EntityType, IEntityBuilder> EntityBuilders = new();

    private static Type[] types = Assembly.GetExecutingAssembly().GetTypes();

    public static void Initialize()
    {
        RegisterCommandHandlers();
        RegisterFormationStrategies();
        RegisterEntityBuilders();
    }
    public static void RegisterCommandHandlers()
    {
        var targetTypes = types.Where(t => t.IsAssignableTo(typeof(ICommandHandler)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in targetTypes)
        {
            if (type.GetCustomAttribute<CommandTypeAttribute>() is { } attr)
            {
                CommandHandlers[attr.Name] = (ICommandHandler)Activator.CreateInstance(type)!;
            }
        }
        Log.Information("CommandHandlers Registered");
    }

    public static void RegisterFormationStrategies()
    {
        var targetTypes = types
            .Where(t => t.IsAssignableTo(typeof(IFormation)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in targetTypes)
        {
            if (type.GetCustomAttribute<FormationStrategyAttribute>() is { } strategy)
            {
                Strategies[strategy.Type] = (IFormation)Activator.CreateInstance(type)!;
            }
        }
        Log.Information("FormationStrategies Registered");
    }

    public static void RegisterEntityBuilders()
    {
        var targetTypes = types
            .Where(t => t.IsAssignableTo(typeof(IEntityBuilder)) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in targetTypes)
        {
            if (type.GetCustomAttribute<BuildUnitTypeAttribute>() is { } attr)
            {
                EntityBuilders[attr.EntityType] =  (IEntityBuilder)Activator.CreateInstance(type)!;
            }
        }
        Log.Information("Builders Registered");
    }
}