using System;
using System.Collections.Generic;
using System.Threading;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class LRUCacheTests
    {
        [Test]
        public void BasicTest()
        {
            var cache = new LRUCache<int, int>(2);
            var invocationCount = 0;
            
            Func<int, int> factory = k =>
            {
                invocationCount++;

                // extremely CPU intensive operation
                return k*2;
            };

            cache.Get(1, factory); // a: factory()
            cache.Get(2, factory); // b: factory()
            cache.Get(1, factory); // c: cached (a)
            cache.Get(2, factory); // d: cached (b)
            cache.Get(3, factory); // e: factory(); discard [1,2] (c)
            cache.Get(1, factory); // f: factory(); discard [2,4] (d)

            // we should have had 2 cached results
            // and 4 factory invocations
            Assert.AreEqual(4, invocationCount); // (a,b,e,f)

            // should only have 2 items in cache
            Assert.AreEqual(2, cache.Count); // (e,f)

            // we should have the keys/values enumerated in order of most -> least recently used
            CollectionAssert.AreEqual(new[]
            {
                new KeyValuePair<int, int>(1, 2), // (f)
                new KeyValuePair<int, int>(3, 6), // (e)
            }, cache);

        }

        [Test]
        public void ForceLockRecursionException()
        {
            var cache = new LRUCache<int, int>(1);

            cache.Get(0, k => k);

            // write during a read
            foreach (var item in cache)
                Assert.Throws<LockRecursionException>(() => cache.Get(1, k => k));
        }
    }
}