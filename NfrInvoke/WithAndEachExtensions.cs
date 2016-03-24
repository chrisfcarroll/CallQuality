using System;
using System.Collections.Generic;

namespace NfrInvoke
{
    public static class WithAndEachExtensions
    {
        /// <summary>
        /// Applies <paramref name="action"/> to <paramref name="this"/> and returns <paramref name="this"/>
        /// Enables a 'fluent' style syntax.
        /// </summary>
        /// <param name="this">An object to act on</param>
        /// <param name="action">The action to apply</param>
        /// <returns><paramref name="this"/></returns>
        public static T With<T>(this T @this, Action<T> action) { action(@this); return @this; }

        /// <summary>
        /// Applies <paramref name="with"/> to each element of <paramref name="enumerable"/> and returns <paramref name="enumerable"/>
        /// Enables a 'fluent' style syntax, and actions in a Linq chain.
        /// Warning! Note that applying mutating actions to a collection breaks immutability
        /// </summary>
        /// <param name="enumerable">A collection of objects to act on</param>
        /// <param name="with">The action to apply</param>
        /// <returns><paramref name="enumerable"/></returns>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T> with)
        {
            foreach (var item in enumerable) { with(item); }
            return enumerable;
        }
    }
}