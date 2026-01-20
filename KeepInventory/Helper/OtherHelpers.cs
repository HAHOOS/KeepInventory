using System;
using System.Collections.Generic;

namespace KeepInventory.Helper
{
    public static class OtherHelpers
    {
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action)
        {
            foreach (var pair in dictionary)
            {
                action?.Invoke(pair);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action?.Invoke(item);
            }
        }

        public static void TryAdd<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }
    }
}