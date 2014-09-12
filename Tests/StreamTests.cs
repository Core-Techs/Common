using System;
using System.Text;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class StreamTests
    {
        [Test]
        public void CanFindBytes()
        {
            using (var stream = new Byte[]{1,0,1,0,2}.ToMemoryStream())
            {
                var position = stream.SeekTo(new byte[] {1, 0, 2});
                Assert.AreEqual(2, position);

            }
        }

        [Test]
        public void NullWhenNotFound()
        {
            using (var stream = new Byte[]{1,0,1,0,1}.ToMemoryStream())
            {
                var position = stream.SeekTo(new byte[] {1, 0, 2});
                Assert.IsNull(position);
            }
        }

        [Test]
        public void CanFindAString()
        {                                  //                      1         2         3
                                          //             012345678901234567890123456789012345678
            using (var stream = Encoding.ASCII.GetBytes("My name is Ronnie Overby. What's yours?").ToMemoryStream())
            {
                var position = stream.SeekTo("Ronnie Overby", Encoding.ASCII);
                Assert.AreEqual(11, position);
            }
        }
    }
}