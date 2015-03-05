using System;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    class StringTests
    {
        [Test]
        public void CanParseHexBytes()
        {
            var expected = Convert.FromBase64String("mz8Uam82f7dY9OSJDT/CJg==");

            const string hex = "9B-3F-14-6A-6F-36-7F-B7-58-F4-E4-89-0D-3F-C2-26";

            var bytes = hex.AsHexBytes();
            CollectionAssert.AreEqual(expected, bytes);
            
            // try with colons
            bytes = hex.Replace("-",":").AsHexBytes();
            CollectionAssert.AreEqual(expected, bytes);
            
            // try without dashes
            bytes = hex.Replace("-","").AsHexBytes();
            CollectionAssert.AreEqual(expected, bytes);
            
            // try with whitespace
            bytes = hex.Replace("-"," ").AsHexBytes();
            CollectionAssert.AreEqual(expected, bytes);
        }

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
