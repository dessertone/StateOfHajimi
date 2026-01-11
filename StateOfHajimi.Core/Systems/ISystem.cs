namespace StateOfHajimi.Core.Systems;

public interface ISystem
{
    void Initialize();
    void Update(float deltaTime);
}