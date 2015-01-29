using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class ByteSizeTests
    {
        [Test]
        public void CanAdd()
        {
            var a = ByteSize.FromGigabytes(1);
            var b = ByteSize.FromGigabytes(2);

            var c = a + b;

            Assert.AreEqual(ByteSize.FromGigabytes(3), c);
        }

        [Test]
        public void CanSubtract()
        {
            var a = ByteSize.FromGigabytes(3);
            var b = ByteSize.FromGigabytes(1.5);

            var c = a - b;

            Assert.AreEqual(ByteSize.FromGigabytes(1.5), c);
        }
    }
}