using System.Numerics;
using Arch.Core;
using Serilog;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public class UnitFactory
{
    private static readonly Dictionary<UnitType, IEntityBuilder> _builders = AttributeHelper.EntityBuilders; 
    
    public static void CreateUnit(World world, UnitType type, Vector2 position, int teamId)
    {
        if (_builders.TryGetValue(type, out var builder))
        {
            builder.Create(world, position, teamId);
            Log.Debug($"[UnitFactory] create unit: {type}");
        }
        else
        {
            Log.Error($"[UnitFactory] {type}'s Builder not found, Please check UnitTypeAttribute is set correctly!");
        }
    }
}