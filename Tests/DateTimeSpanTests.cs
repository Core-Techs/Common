using System;
using System.Data;
using CoreTechs.Common;
using CoreTechs.Common.Database;
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

        [Test]
        public void aisdjfi()
        {
            var dt = new DataTable();
            var good=dt.AsEnumerable().Select(x => x.Create<DateTimeSpanTests>());
        }


    }
}