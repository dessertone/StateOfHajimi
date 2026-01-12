namespace StateOfHajimi.Core.Components.StateComponents;

public struct Health
{
    public int MaxHp;
    public int Current;
    public bool IsDead => Current <= 0;
}