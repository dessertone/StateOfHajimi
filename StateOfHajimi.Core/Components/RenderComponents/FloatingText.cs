using System.Numerics;

namespace StateOfHajimi.Core.Components.RenderComponents;

public struct Text(string content)
{
    public string Content = content;
}

public struct Life(float lifeTime)
{
    public float Age;
    public float LifeTime = lifeTime;
}