using System.Collections.Generic;
using System.Linq;

namespace NFRInvoke
{
    static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns the non-null elements of <paramref name="enumerable"/>
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) { return enumerable.Where(e => e != null); }

        /// <summary>
        /// Synonym for 
        /// <code>!<paramref name="enumerable"/>.<see cref="Enumerable.Any{TSource}(System.Collections.Generic.IEnumerable{TSource})"/></code>
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns><see cref="bool.True"/> if <paramref name="enumerable"/> is empty</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) { return !enumerable.Any(); }
    }
}