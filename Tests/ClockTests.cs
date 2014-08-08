using System;
using System.Linq;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    internal class ClockTests
    {
        [Test]
        public void CanGetNowFromSystemClock()
        {
            Assert.AreEqual(DateTimeOffset.Now, SystemClock.Instance.Now);
        }

        [Test]
        public void TestClockIsThreadSafe()
        {
            const int n = 10000;
            var clock = new TestClock(DateTimeOffset.Now, 1.Milliseconds());
            var nows = System.Linq.Enumerable.Range(0, n)   
                .AsParallel()
                .Select(x => clock.Now)
                .Distinct()
                .Count();

            Assert.AreEqual(n, nows);
        }

        [Test]
        public void TestClockWithIncrementTimeSpan()
        {
            var init = new DateTimeOffset(new DateTime(1984, 5, 10));
            var clock = new TestClock(init, 1.Weeks());

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(clock.Now, init + i.Weeks()); 
        }

        [Test]
        public void CanMakeTimeStandStill()
        {
            var init = new DateTimeOffset(new DateTime(1984, 5, 10));
            var clock = new TestClock(init, x => x);

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(clock.Now, init);
        }
    }
}