using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Serilog;
using StateOfHajimi.Core.Components.ProductComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;

namespace StateOfHajimi.Core.Data.EntityBuilders;

public class UnitFactory
{
    private static readonly Dictionary<EntityType, IEntityBuilder> _builders = AttributeHelper.EntityBuilders; 
    
    public static void Create(CommandBuffer buffer, EntityType type, Vector2 position, int teamId, ref RallyPoint rally)
    {
        if (_builders.TryGetValue(type, out var builder))
        {
            if (EntityPool.TryPop(type, out var entity))
            {
                buffer.Remove<Disabled>(entity);
            }
            else
            {
                    entity = buffer.Create(builder.Archetype);
            }
            builder.Build(buffer, entity, position, teamId, ref rally);
        }
        else
        {
            Log.Error($"[UnitFactory] {type}'s Builder not found, Please check UnitTypeAttribute is set correctly!");
        }
    }
}