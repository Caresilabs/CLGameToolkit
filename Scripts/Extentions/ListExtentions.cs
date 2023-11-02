using System.Collections.Generic;

public static class ListExtentions
{
    public static T Random<T>(this IList<T> collection)
    {
        return collection[UnityEngine.Random.Range(0, collection.Count)];
    }
}
