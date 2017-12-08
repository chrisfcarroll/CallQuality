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
                if (value != null)
                {
                    Cache.Add(keystring, value, DateTime.Now.AddSeconds(CacheTimeSeconds));
                    keys.Add(keystring);
                }
            }
            return (T)value;
        }

        string KeyFor(Delegate @delegate, params object[] parameters) {return ToString(@delegate, parameters) + ":" + UniqueName;}


        public readonly string UniqueName;
        public readonly int CacheTimeSeconds;
        //TODO: Replace this with a pair of delegates
        public static readonly MemoryCache Cache = MemoryCache.Default;
        static readonly List<string> keys = new List<string>();

        public static void EmptyCaches() { keys.ForEach(key => Cache.Remove(key)); }

        /// <param name="cacheTimeSeconds">Used to set absolute expiry this many seconds in the future</param>
        /// <param name="uniqueName">Optional because your methods are usually uniquely identified by AssemblyQualifiedTypeName.MethodName.</param>
        public Cacher(int cacheTimeSeconds, string uniqueName="")
        {
            UniqueName = uniqueName;
            CacheTimeSeconds = cacheTimeSeconds;
            if (CacheTimeSeconds < 1) { CacheTimeSeconds = 1; }
        }
    }
}