using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace CoreTechs.Common.Reflection
{
    public static class Memoizer
    {
        static readonly ConcurrentDictionary<object, object> Cache = new ConcurrentDictionary<object, object>();

        public static T Get<T>(object keyData, Func<T> factory, [CallerMemberName] string @namespace = null)
        {
            var cacheKey = new {@namespace, keyData};
            return (T) Cache.GetOrAdd(cacheKey, _ => factory());
        }
    }
}