using Arch.Core;

namespace StateOfHajimi.Core.Utils.Extensions;

public static class ArchExtensions
{

    public static T GetSingleton<T>(this World world) where T : struct
    {
        var query = new QueryDescription().WithAll<T>();
        T result = default;
        bool found = false;
        
        world.Query(in query, (ref T component) => 
        {
            result = component;
            found = true;
        });

        if (!found) throw new Exception($"Singleton component {typeof(T).Name} not found in world.");
        return result;
    }

    public static void SetSingleton<T>(this World world, T newValue) where T : struct
    {
        var query = new QueryDescription().WithAll<T>();
        bool found = false;
        
        world.Query(in query, (ref T component) => 
        {
            component = newValue;
            found = true;
        });
        
        if (!found)
        {
            world.Create(newValue);
        }
    }
}