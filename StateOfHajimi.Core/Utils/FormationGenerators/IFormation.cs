using System.Numerics;

namespace StateOfHajimi.Core.Utils.FormationGenerators;

public interface IFormation
{
    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing = 20);
}