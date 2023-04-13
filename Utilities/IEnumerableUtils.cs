using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class IEnumerableUtils
    {
        public static T FirstOrSpecified<T>(this IEnumerable<T> enumerable, T def)
        {
            T ret = def;

            enumerable.Any(x =>
            {
                ret = x;
                return true;
            });

            return ret;
        }
    }
}
