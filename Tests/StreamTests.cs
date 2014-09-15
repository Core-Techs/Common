using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class StreamTests
    {
        [Test]
        public void CanSeekToBeginningOfByteSequence()
        {
            using (var stream = new Byte[] { 1, 0, 1, 0, 2 }.ToMemoryStream())
            {
                var found = stream.SeekStartOf(new byte[] { 1, 0, 2 });
                Assert.True(found);
                Assert.AreEqual(2, stream.Position);

            }
        }

        [Test]
        public void CanSeekTo_EndOf_ByteSequence()
        {
            using (var stream = new Byte[] { 1, 0, 1, 0, 2 }.ToMemoryStream())
            {
                var found = stream.SeekEndOf(new byte[] { 1, 0, 1 });
                Assert.True(found);
                Assert.AreEqual(3, stream.Position);
            }
        }

        [Test]
        public void CanSeek_ToEndOf_ByteSequence_WhereSequenceIs_AtEndOfStream()
        {
            using (var stream = new Byte[] { 1, 0, 1, 0, 2 }.ToMemoryStream())
            {
                var found = stream.SeekEndOf(new byte[] { 1, 0, 2 });
                Assert.True(found);
                Assert.AreEqual(5, stream.Position);
            }
        }

        [Test]
        public void FalseWhenNotFound()
        {
            using (var stream = new Byte[] { 1, 0, 1, 0, 1 }.ToMemoryStream())
            {
                var found = stream.SeekStartOf(new byte[] { 1, 0, 2 });
                Assert.False(found);
            }
        }

        [Test]
        public void CanSeekToBeginningOfString()
        {                                  //                      1         2         3
            //             012345678901234567890123456789012345678
            using (var stream = Encoding.ASCII.GetBytes("My name is Ronnie Overby. What's yours?").ToMemoryStream())
            {
                var found = stream.SeekStartOf("Ronnie Overby", Encoding.ASCII);
                Assert.True(found);
                Assert.AreEqual(11, stream.Position);
            }
        }

        [Test]
        public void CanSeekToEndOfString()
        {                                  //                      1         2         3
            //             012345678901234567890123456789012345678
            using (var stream = Encoding.ASCII.GetBytes("My name is Ronnie Overby. What's yours?").ToMemoryStream())
            {
                var found = stream.SeekEndOf("Ronnie Overby", Encoding.ASCII);
                Assert.True(found);
                Assert.AreEqual(24, stream.Position);
            }
        }

        [Test]
        public void CanRead_Thru_FindingTarget()
        {
            using (var stream = new byte[] { 1, 0, 1, 0, 2, 0, 3, 0, 2, 0, 1, 0, 1 }.ToMemoryStream())
            {
                var target = new byte[] { 3, 0, 2 };
                var expected = new byte[] { 1, 0, 1, 0, 2, 0 };
                CollectionAssert.AreEqual(stream.EnumerateBytesUntil(target), expected);
            }
        }

        [Test]
        public void CanRead_Thru_FindingTarget_ButItsNotThere()
        {
            using (var stream = new byte[] { 1, 0, 1, 0, 2, 0, 3, 0, 2, 0, 1, 0, 1 }.ToMemoryStream())
            {
                var target = new byte[] { 3, 0, 2, 4 };
                var expected = stream.ToArray();
                var enumerated = stream.EnumerateBytesUntil(target).ToArray();
                CollectionAssert.AreEqual(enumerated, expected);
            }
        }

        [Test]
        public void CanRead_Thru_FindingTargetString()
        {
            const string source = "Ronnie Overby is my name.";
            var stream = Encoding.Default.GetBytes(source).ToMemoryStream();
            var bytes = stream.EnumerateBytesUntil(" is").ToArray();
            var s = Encoding.Default.GetString(bytes);
            Assert.AreEqual("Ronnie Overby", s);
        }

        [Test]
        public void CanFindFirstString()
        {
            const string source = "Ronnie Overby is my name.";
            var stream = Encoding.Default.GetBytes(source).ToMemoryStream();
            var found = stream.SeekEndOfAny(new[] { "Ronnie Overby is my name. AND NOW A WORD", "Ronnie Smith", "Oven", " is" });
            Assert.AreEqual(" is", found);
            Assert.AreEqual(16,stream.Position);
        }

      

       

    }


}

