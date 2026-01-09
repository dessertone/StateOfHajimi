using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using WarInPalace.Core.Enums;
using WarInPalace.Core.Utils.Attributes;

namespace WarInPalace.Core.Utils.FormationGenerators;

[FormationStrategy(FormationType.Rectangle)]
public class RectangleFormation:IFormation
{

    public int RowCount { get; set; } = 10;
    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing = 20)
    {
        var col = - RowCount / 2;
        var row = 0;
        while (true)
        {
            for (var i = 0; i < RowCount; i++)
            {
                yield return new Vector2(center.X + i * spacing, center.Y + row * spacing);
            }
            row++;
        }
    }
}