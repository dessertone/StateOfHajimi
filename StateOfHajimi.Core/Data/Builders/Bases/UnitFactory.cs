using Arch.Buffer;
using Serilog;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Data.Builders.Bases;

public class UnitFactory
{
    private static readonly Dictionary<EntityType, IEntityBuilder> _builders = AttributeHelper.EntityBuilders; 
    
    public static void CreateEntity(CommandBuffer buffer, EntityType type,ref BuildContext context)
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
            builder.Build(buffer, entity,ref context);
        }
        else
        {
            Log.Error($"[UnitFactory] {type}'s Builder not found, Please check UnitTypeAttribute is set correctly!");
        }
    }
    
}