﻿using System;
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

        [Test]
        [TestCase("5/10/2015", 1984, "5/10/1984")]
        [TestCase("5/10/2015", 2015, "5/10/2015")]
        [TestCase("5/10/2015", 2020, "5/10/2020")]
        public void CanSetYear(DateTime dt, int year, DateTime expected)
        {
            Assert.AreEqual(expected, dt.SetYear(year));
        }

        [Test]
        [TestCase("5/10/2015", 11, "11/10/2015", false)]
        [TestCase("5/10/2015", 1, "1/10/2015", false)]
        [TestCase("5/10/2015", 5, "5/10/2015", false)]
        [TestCase("5/31/2015", 12, "12/31/2015",false)]

        [TestCase("5/31/2015", 0, "1/1/1111", true)]
        [TestCase("5/31/2015", 0, "1/1/1111", true)]
        public void CanSetMonth(DateTime dt, int month, DateTime expected, bool throws = false)
        {
            void Test()
            {

                Assert.AreEqual(expected, dt.SetMonth(month));
            }

            if (throws)
            {
                Assert.Throws<ArgumentOutOfRangeException>(Test);
            }
            else
            {
                Test();
            }
        }


    }
}