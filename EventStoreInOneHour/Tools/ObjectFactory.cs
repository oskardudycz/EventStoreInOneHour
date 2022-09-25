using System.Runtime.CompilerServices;

namespace EventStoreInOneHour.Tools;

public static class ObjectFactory<T>
{
    public static T GetEmpty() =>
        (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
