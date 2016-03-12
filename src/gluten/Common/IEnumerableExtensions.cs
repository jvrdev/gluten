using System.Collections.Generic;

namespace Gluten.Common
{
    public static class IEnumerableExtensions
    {
        public static string ToCommaSeparatedStrings<T>(this IEnumerable<T> list)
        {
            return string.Join(",", list);
        }
    }
}
