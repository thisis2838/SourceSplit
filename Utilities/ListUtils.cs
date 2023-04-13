using System.Collections.Generic;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class ListUtils
    {
        public static void AddRange<T>(this List<T> l, params T[] values)
        {
            foreach (T value in values) { l.Add(value); }
        }
    }
}
