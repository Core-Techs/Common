using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace CoreTechs.Common
{
    /// <summary>
    /// Allows easy "memoization" (http://en.wikipedia.org/wiki/Memoization) of return data for function. 
    /// Use the Get function to get at memoized data.
    /// </summary>
    public class Memoizer
    {
        /// <remarks>
        /// Direct access provided to the memoization cache, so that values can be manually added/removed.
        /// </remarks>>
        public readonly ConcurrentDictionary<object, object> Cache = new ConcurrentDictionary<object, object>();

        /// <summary>
        /// Returns data produced by the factory. Future calls with matching keyData and namespace
        /// will return the "memoized" value.
        /// </summary>
        /// <typeparam name="T">The type of data to be returned.</typeparam>
        /// <param name="factory">The function that produces the data.</param>
        /// <param name="keyData">Data that is used as a cache key. This needs to have a good implementation of GetHashCode. Best to use anonymous objects with primitive properties.</param>
        /// <param name="namespace">
        /// A namespace for the cache key. 
        /// This is automatically set to the caller's member name when no value is passed in.</param>
        public T Get<T>(object keyData, Func<T> factory,  [CallerMemberName] string @namespace = null)
        {
            var cacheKey = new {@namespace, keyData};
            return (T) Cache.GetOrAdd(cacheKey, _ => factory());
        }

        private static readonly Lazy<Memoizer> LazyInstance = new Lazy<Memoizer>(() => new Memoizer());
        public static Memoizer Instance
        {
            get { return LazyInstance.Value; }
        }

        /// <summary>
        /// Used by this library.
        /// </summary>
        internal static readonly Lazy<Memoizer> InternalInstance = new Lazy<Memoizer>(() => new Memoizer());
    }
}