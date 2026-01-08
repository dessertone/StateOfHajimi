using System.Numerics;

namespace WarInPalace.Core.Utils.FormationGenerators;

public interface IFormation
{
    public IEnumerable<Vector2> Spawn(Vector2 center, float spacing = 20);
}