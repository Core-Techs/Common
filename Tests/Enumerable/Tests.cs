using System;
using System.Collections.Generic;
using System.Linq;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests.Enumerable
{
    public class Tests
    {
        [Test]
        public void CanSplitEnumerable()
        {
            var e = new int?[] { -1, -1, 1, null, 2, -1, 1, -1 };
            CollectionAssert.AreEqual(new[]
            {
                new int?[0],
                new int?[0],
                new int?[]{1,null,2},
                new int?[]{1},
                new int?[0],
                
            }, e.Split(-1));
        }

        [Test]
        public void PreFetchedEnumerableExceptionsBubbleUp()
        {
            const int n = 50;
            const string exMsg = "ARGH!";

            var source = System.Linq.Enumerable.Range(0, n * 2)
                .Select(x =>
                {
                    if (x > n)
                        throw new Exception(exMsg);

                    return x;
                });

            var prefetched = source.PreFetch();

            try
            {
                foreach (var i in prefetched)
                    Assert.True(i <= n);

                Assert.Fail("enumeration of prefetched should have failed");
            }
            catch (AggregateException agx)
            {
                agx.Handle(x => x.Message == exMsg);
            }
        }

        [Test]
        public void CanPreFetchEnumerable()
        {
            var source = CreateSequence().Take(10).ToArray();

            var log = new List<bool>();

            var prefetched = source
                .Select(x =>
                {
                    lock (log)
                        log.Add(true);
                    return x;
                })
                .PreFetch(2)
                .Select(x =>
                {
                    lock (log)
                        log.Add(false);
                    return x;
                })
                .ToArray();

            CollectionAssert.AreEqual(source, prefetched);

            // ensure log entries are heterogenous (production and consumption operations were interleaved)

            bool? prev = null;
            var split = log.SplitWhere(x =>
            {
                var hadPrev = prev.HasValue;
                var changed = x != prev;
                prev = x;
                return hadPrev && changed;
            }).ToArray();

            Assert.True(split.Length > 2);

        }

        IEnumerable<long> CreateSequence()
        {
            while (true)
            {
                yield return Environment.TickCount;
            }
        }
    }
}
