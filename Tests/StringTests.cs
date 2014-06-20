using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    class StringTests
    {
        [Test]
        public void SafeSubstringWorksLikeSubstring()
        {
            const string s = "In A World...";
            const int i = 5;
            const int l = 5;

            var safeSub = s.SafeSubstring(i, l);
            var sub = s.Substring(i, l);
            Assert.AreEqual(sub,safeSub);
        }

        [Test]
        public void SafeSubstringIsFineWithNegatives()
        {
            const string s = "In A World...";
            const int i = -5;
            const int l = -5;

            var safeSub = s.SafeSubstring(i, l);
            Assert.AreEqual("",safeSub);
        }

        [Test]
        public void SafeSubstringIsFineWithOutOfBoundLength()
        {
            const string s = "In A World...";
            const int i = 5;
            const int l = int.MaxValue;

            var safeSub = s.SafeSubstring(i, l);
            Assert.AreEqual("World...", safeSub);
        }

        [Test]
        public void SafeSubstringIsFineWithOutOfBoundStartAndLength()
        {
            const string s = "In A World...";
            const int i = int.MaxValue - 1;
            const int l = int.MaxValue;

            var safeSub = s.SafeSubstring(i, l);
            Assert.AreEqual("", safeSub);
        }
    }
}
