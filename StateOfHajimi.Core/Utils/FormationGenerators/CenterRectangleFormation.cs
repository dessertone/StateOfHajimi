using System.Numerics;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Utils.FormationGenerators;


[FormationStrategy(FormationType.CenterRectangle)]
public class CenterRectangleFormation:IFormation
{
    public int RowCount { get; set; } = 10;
    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing = 30)
    {
        var row = 0;
        while (true)
        {
            var col = - RowCount / 2;
            for (var i = 0; i < RowCount / 2; i++)
            {
                for (var j = 0; j <= 1 ; j++)
                {
                    i = j == 0 ? i : -i;
                    yield return new Vector2(center.X + i * spacing, center.Y + row * spacing);
                    if (i == 0) break;
                }
            }
            row++;
        }
    }
}