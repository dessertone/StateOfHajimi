using Arch.Core;
using WarInPalace.Core.Components;
using WarInPalace.Core.Data;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Utils.FormationGenerators;

namespace WarInPalace.Core.Systems;


public class InputSystem:BaseSystem
{
    
    
    private static readonly QueryDescription _queryDescription = new QueryDescription().WithAll<Position, Destination, IsSelected>();
    
    public InputSystem(World world) : base(world)
    {
    }

    /// <summary>
    /// 更新已选中目标的目的地
    /// </summary>
    /// <param name="deltaTime"></param>
    public override void Update(float deltaTime)
    {
        var input = InputState.Instance;
        if (input.IsRightMousePressed)
        {
            var formation = FormationResolver.Get(FormationType.Spiral);
            var pos = input.MousePosition;
            using var generator = formation.Spawn(pos, spacing:40).GetEnumerator();
            generator.MoveNext();
            GameWorld.Query(in _queryDescription, (ref Position position, ref Destination targetPos) =>
            {
                targetPos.Value = generator.Current;
                targetPos.IsActive = true;
                generator.MoveNext();
            });
            input.IsRightMousePressed = false;
        }
    }
}