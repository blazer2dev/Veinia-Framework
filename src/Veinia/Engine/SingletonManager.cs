using System.Collections.Generic;

public class SingletonManager
{
    private static List<ISingleton> services = new List<ISingleton>();

    public static T1 Fetch<T1>() where T1 : ISingleton
    {
        T1 result = default;
        foreach (var item in services)
            if (item is T1) result = (T1)item;

        return result;
    }
    public static void Remove<T1>() where T1 : ISingleton
    {
        foreach (var item in services)
            if (item is T1) services.Remove(item);
    }
    public static void Remove(ISingleton singleton) => services.Remove(singleton);

    public static void Append(ISingleton service)
    {
        services.Add(service);
        service.Initialize();
    }

    public static void Update()
    {
        foreach (var item in services)
        {
            item.Update();
        }
    }

    public static void LateUpdate()
    {
        foreach (var item in services)
        {
            item.LateUpdate();
        }
    }
}