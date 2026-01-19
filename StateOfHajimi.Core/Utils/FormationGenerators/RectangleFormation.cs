using System.Numerics;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Utils.FormationGenerators;

[FormationStrategy(FormationType.Rectangle)]
public class RectangleFormation : IFormation
{
    /// <summary>
    /// 每行显示多少个单位
    /// </summary>
    public int Columns { get; set; } = 10;

    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing = 100)
    {
        var cols = Math.Max(1, Columns);
        var currentIndex = 0;

        while (true)
        {
            var row = currentIndex / cols;
            var colIndex = currentIndex % cols;
            float xOffset = 0;
            if (colIndex > 0)
            {
                var pairCount = (colIndex + 1) / 2;
                var direction = colIndex % 2 == 1 ? 1 : -1;
                xOffset = pairCount * spacing * direction;
            }
            var yOffset = row * spacing;
            var pos = new Vector2(
                center.X + xOffset,
                center.Y + yOffset
            );
            yield return pos;
            currentIndex++;
        }
    }
}