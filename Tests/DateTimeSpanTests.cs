using System;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    internal class DateTimeSpanTests
    {
        [Test]
        public void CanMultTimespan()
        {
            var span = TimeSpan.FromMinutes(1.5);
            var threeX = span.Multiply(3);
            Assert.AreEqual(TimeSpan.FromMinutes(4.5), threeX);
        }

        [Test]
        public void CanDivTimespan()
        {
            var span = TimeSpan.FromMinutes(1.5);
            var quot = span.Divide(3);
            Assert.AreEqual(TimeSpan.FromMinutes(.5), quot);
        }
    }
}