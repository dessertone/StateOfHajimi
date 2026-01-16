using System.Text.RegularExpressions;
using Arch.Buffer;
using StateOfHajimi.Core.Components.CombatComponents;
using StateOfHajimi.Core.Components.MoveComponents;
using StateOfHajimi.Core.Components.StateComponents;
using StateOfHajimi.Core.Components.Tags;
using StateOfHajimi.Core.Enums;

namespace StateOfHajimi.Core.Data;

using Arch.Core;
using Arch.Core.Extensions;
using System.Collections.Generic;
using System.Numerics;

public static class EntityPool
{
    private static readonly Dictionary<EntityType, Stack<Entity>> _pool = new();
    
    public static bool TryPop(EntityType type, out Entity entity)
    {
        if (!_pool.TryGetValue(type, out var stack))
        {
            stack = new Stack<Entity>();
            _pool[type] = stack;
            entity = Entity.Null;
            return false;
        }
        if (stack.Count > 0)
        {
            entity = stack.Pop();
            return true;
        }
        entity = Entity.Null;
        return false;
    }

    /// <summary>
    /// 回收实体
    /// </summary>
    public static void Despawn(CommandBuffer buffer, Entity entity, EntityType type)
    {
        buffer.Add<Disabled>(entity);
        if (!_pool.TryGetValue(type, out var stack))
        {
            stack = new Stack<Entity>();
            _pool[type] = stack;
        }
        stack.Push(entity);
    }
}