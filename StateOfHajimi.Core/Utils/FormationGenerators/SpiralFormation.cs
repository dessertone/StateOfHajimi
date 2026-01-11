using System.Numerics;
using StateOfHajimi.Core.Enums;
using StateOfHajimi.Core.Utils.Attributes;

namespace StateOfHajimi.Core.Utils.FormationGenerators;


[FormationStrategy(FormationType.Spiral)]
public class SpiralFormation:IFormation
{
    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing)
    {
        yield return center;

        var index = 1;
        
        var goldenAngle = 137.508f * (MathF.PI / 18f);
        while (true)
        {
            var r = MathF.Sqrt(index) * spacing;
            var theta = index * goldenAngle;
            
            var x = center.X + r * MathF.Cos(theta);
            var y = center.Y + r * MathF.Sin(theta);
            yield return new Vector2(x, y);
            ++index;
            
            if(index < 0) yield break;
        }
    }
}