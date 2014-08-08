using System;
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