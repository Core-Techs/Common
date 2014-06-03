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
    }
}
