using CoreTechs.Common.Text;
using NUnit.Framework;
using System.IO;

namespace Tests.Text
{
    public class CsvWriterTests
    {
        [Test]
        public void CanWriteNullValue()
        {
            var writer = new StringWriter();
            var csv = new CsvWriter(writer);
            csv.AddFields(null, null);

            Assert.AreEqual(",", writer.ToString());

        }

    }
}
