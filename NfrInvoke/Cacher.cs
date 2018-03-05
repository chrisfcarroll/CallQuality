using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace NFRInvoke
{
    /// <summary>Memoize the results of method calls.</summary>
    public class Cacher : InvokeWrapper
    {
        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            return FromCacheOrCallback(KeyFor(wrappedFunctionCall, parameters), callback);
        }

        T FromCacheOrCallback<T>(string keystring, Func<T> callback)
        {
            var value = Cache.Get(keystring);
            if (value == null)
            {
                value = callback();
                Cache.Add(keystring, value??NullPlaceholder, DateTime.Now + CacheTime);
                keys.Add(keystring);
            }

            return value != null || typeof(T).IsValueType
                ? (T) value
                : (value.Equals(NullPlaceholder) ? (T)(object)null : (T) value);
        }
        /// <summary>Clear all cached entries known to this instance.</summary>
        public void Clear() { keys.ForEach(key => Cache.Remove(key)); keys.Clear();}

        /// <summary>Two instances of <see cref="Cacher"/> with the same <see cref="UniqueName"/> will share cache entries.</summary>
        public readonly string UniqueName;

        /// <summary>The TimeSpan for which entries should be cached.</summary>
        /// <remarks>If two <see cref="Cacher"/>s share the same name but different <see cref="CacheTime"/>, then the Cacher which puts a value into Cache will determine how long it is cached for.</remarks>
        public readonly TimeSpan CacheTime;

        string KeyFor(Delegate @delegate, params object[] parameters) {return ToString(@delegate, parameters) + ":" + UniqueName;}
        readonly List<string> keys = new List<string>();

        /// <param name="cacheTime">Used to set absolute expiry this many seconds in the future</param>
        /// <param name="uniqueName">Optional because your methods are usually uniquely identified by the MethodName and parameters of each call.</param>
        public Cacher(TimeSpan cacheTime, string uniqueName="")
        {
            UniqueName = uniqueName;
            CacheTime = cacheTime;
        }

        public static readonly MemoryCache Cache = MemoryCache.Default;
        static readonly object NullPlaceholder= new object();
    }
}