using System.Collections.Generic;
using System.Linq;

public static class ListExtentions
{
    public static T Random<T>(this IList<T> collection)
    {
        return collection[UnityEngine.Random.Range(0, collection.Count)];
    }

    public static T Random<T>(this IEnumerable<T> collection)
    {
        return collection.ElementAt(UnityEngine.Random.Range(0, collection.Count()));
    }

    public static T RandomNullable<T>(this IList<T> collection)
    {
        int count = collection.Count;
        if (count == 0) return default;

        return collection[UnityEngine.Random.Range(0, count)];
    }

    public static bool AddUnique<T>(this IList<T> collection, T item)
    {
        if (collection.IndexOf(item) >=0)
            return false;

        collection.Add(item);
        return true;
    }

    public static T FirstOrDefault<T>(this IList<T> collection)
    {
        if (collection == null || collection.Count == 0) return default;
        return collection[0];
    }

    public static T FirstOrDefault<T>(this IList<T> collection, System.Func<T, bool> predicate)
    {
        if (collection == null || collection.Count == 0) return default;

        foreach (T item in collection)
            if (predicate(item)) return item;

        return default;
    }
}
