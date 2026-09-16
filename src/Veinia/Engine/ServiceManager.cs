using System.Collections.Generic;

public class ServiceManager
{
    private static List<IService> services = new List<IService>();

    public static T1 Fetch<T1>() where T1 : IService
    {
        T1 result = default;
        foreach (var item in services)
            if (item is T1) result = (T1)item;

        return result;
    }

    public static void Append(IService service) => services.Add(service);

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