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
        public void CanBufferEnumerable()
        {
            var buffers = 1.To(35).Buffer(10).Select(b => b.ToArray()).ToArray();
            
            CollectionAssert.AreEqual(1.To(10), buffers[0]);
            CollectionAssert.AreEqual(11.To(20), buffers[1]);
            CollectionAssert.AreEqual(21.To(30), buffers[2]);
            CollectionAssert.AreEqual(31.To(35), buffers[3]);
        }

        [Test]
        public void CanSplitEnumerable()
        {
            var e = new int?[] { -1, -1, 1, null, 2, -1, 1, -1 };
            var result = e.Split(-1).ToArray();
            CollectionAssert.AreEqual(new[]
            {
                new int?[0],
                new int?[0],
                new int?[]{1,null,2},
                new int?[]{1},
                new int?[0],
                
            }, result);
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

        private ExampleClass[] _exampleList;

        [SetUp]
        public void SetUpExampleListForDistinctTests()
        {
            _exampleList = new[]
            {
                new ExampleClass(1, "Hat"),
                new ExampleClass(1, "Shoe"),
            };
        }

        [Test]
        public void CanGetDistinctElementsOnNumber()
        {
            Func<ExampleClass, ExampleClass, bool> equalityOnNumberImpl = (c1, c2) => c1.Number == c2.Number;

            var distinctOnNumber = _exampleList.Distinct(equalityOnNumberImpl).ToList();

            Assert.That(distinctOnNumber, Has.Count.EqualTo(1));
        }

        [Test]
        public void CanGetDistinctElementsOnWord()
        {
            Func<ExampleClass, ExampleClass, bool> equalityOnWordImpl = (c1, c2) => string.Equals(c1.Word, c2.Word);

            var distinctOnWord = _exampleList.Distinct(equalityOnWordImpl).ToList();

            Assert.That(distinctOnWord, Has.Count.EqualTo(2));
        }

        IEnumerable<long> CreateSequence()
        {
            while (true)
            {
                yield return Environment.TickCount;
            }
        }

        private class ExampleClass
        {
            public ExampleClass(int number, string word)
            {
                Number = number;
                Word = word;
            }

            public int Number { get; set; }
            public string Word { get; set; }
        }
    }
}
