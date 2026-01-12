using System.Numerics;
using Arch.Core;
using Serilog;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public class UnitFactory
{
    private static readonly Dictionary<EntityType, IEntityBuilder> _builders = AttributeHelper.EntityBuilders; 
    
    public static void CreateUnit(World world, EntityType type, Vector2 position, int teamId, ref RallyPoint rally)
    {
        if (_builders.TryGetValue(type, out var builder))
        {
            builder.Create(world, position, teamId, ref rally);
            Log.Debug($"[UnitFactory] create unit: {type}");
        }
        else
        {
            Log.Error($"[UnitFactory] {type}'s Builder not found, Please check UnitTypeAttribute is set correctly!");
        }
    }
}