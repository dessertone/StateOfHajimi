namespace StateOfHajimi.Core.Data.Config;

public class UnitStateConfig
{
    public int MaxHp { get; set; }
    public float MoveSpeed { get; set; } // 对应 Velocity
    public float BuildTime { get; set; } // 生产需要的时间
    public float Size { get; set; }      // 碰撞体积大小
    public int AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float AttackSpeed { get; set; }
    public float VisionRange { get; set; }
}