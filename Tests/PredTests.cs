using System.Linq;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    internal class PredicateTests
    {
        [Test]
        public void CanUsePredicates()
        {
            var source = new[] { 1, 2, 3, 4, 5 };

            var isEven = new Pred<int>(x => x%2 == 0);
            var isOdd = isEven.Invert();
            var isOddOr2 = isOdd.Or(x => x == 2);

            var count = source.Where(isOddOr2).Count();

            Assert.AreEqual(4, count);
        }
    }
}