namespace StateOfHajimi.Core.Data.Config;

public class GameSettings
{
    public Dictionary<string, EntityStateConfig> Entities { get; set; } = new();
    
    public Dictionary<string, BuildingStateConfig> Buildings { get; set; } = new();
    public Dictionary<string, UnitAnimationConfig> UnitAnimations { get; set; } = new();
}