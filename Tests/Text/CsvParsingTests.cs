using System.Collections.Generic;
using CoreTechs.Common;
using CoreTechs.Common.Text;
using NUnit.Framework;

namespace Tests.Text
{
    public class CsvParsingTests
    {
        [Test]
        public void Simple()
        {
            using (var rdr = TestFiles.Simple.ToStringReader())
            {
                var it = rdr.ReadCsv().GetEnumerator();

                it.MoveNext();
                var record = it.Current;
                CollectionAssert.AreEqual(new[] { "A", "B", "C" }, record);

                it.MoveNext();
                record = it.Current;
                CollectionAssert.AreEqual(new[] { "D", "E", "F" }, record);

                Assert.False(it.MoveNext());
            }
        }

        [Test]
        public void SimpleTextQualified()
        {
            using (var rdr = TestFiles.SimpleTextQualified.ToStringReader())
            {
                var it = rdr.ReadCsv().GetEnumerator();

                it.MoveNext();
                var record = it.Current;
                CollectionAssert.AreEqual(new[] { "A", "B", "C" }, record);

                it.MoveNext();
                record = it.Current;
                CollectionAssert.AreEqual(new[] { "D", "E", "F" }, record);

                Assert.False(it.MoveNext());
            }
        }

        [Test]
        public void AdvancedTextQualified()
        {
            using (var rdr = TestFiles.AdvancedTextQualified.ToStringReader())
            {
                var it = rdr.ReadCsv().GetEnumerator();

                it.MoveNext();
                var record = it.Current;
                CollectionAssert.AreEqual(new[] { "A,B,C", "D,E,F" }, record);

                it.MoveNext();
                record = it.Current;
                CollectionAssert.AreEqual(new[] { "G,H,I", "J,K,L" }, record);

                it.MoveNext();
                record = it.Current;
                CollectionAssert.AreEqual(new[] { "Ronnie \"Dwanye\" Overby", "\"Tiner\" Overby", "\"THIS\",\r\n\"THAT\",\r\n\"THE OTHER!\"" }, record);

                Assert.False(it.MoveNext());
            }
        }

        [Test]
        public void FieldDataIsTrimmed()
        {
            using (var rdr = TestFiles.FieldDataIsTrimmed.ToStringReader())
            {
                var it = rdr.ReadCsv().GetEnumerator();

                var record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "Ronnie", "Overby" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "  Tina  ", "  Overby  " }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "  Anna     Lukus  " }, record);

                Assert.False(it.MoveNext());
            }
        }

        [Test]
        public void EmptyFields()
        {

            using (var rdr = TestFiles.EmptyFields.ToStringReader())
            {
                var it = rdr.ReadCsv().GetEnumerator();

                var record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "A", "" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "B" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "C", "" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "D", "E", "" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "F", "", "G", "" }, record);

                record = it.GetNextOrDefault();
                CollectionAssert.AreEqual(new[] { "", "", "", "" }, record);

                Assert.False(it.MoveNext());
            }
        }

        [Test]
        public void WithHeader()
        {

            using (var rdr = TestFiles.WithHeader.ToStringReader())
            {
                var it = rdr.ReadCsvWithHeader().GetEnumerator();

                var record = it.GetNextOrDefault();
                Assert.AreEqual("Ronnie", record["Name"]);
                Assert.AreEqual("30", record["Age"]);
                Assert.AreEqual("Male", record["Gender"]);
                Assert.AreEqual("Core Techs", record["EMPLOYER"]);
                
                record = it.GetNextOrDefault();
                Assert.AreEqual("Tina", record["Name"]);
                Assert.AreEqual("30", record["Age"]);
                Assert.AreEqual("Female", record["Gender"]);
                Assert.AreEqual("HPU", record["employer"]);
                
                record = it.GetNextOrDefault();
                Assert.AreEqual("Lukus", record["Name"]);
                Assert.AreEqual("8", record["Age"]);
                Assert.AreEqual("Male", record["Gender"]);
                Assert.IsEmpty(record["Employer"]);
                
                record = it.GetNextOrDefault();
                Assert.AreEqual("Anna", record[0]);
                Assert.AreEqual("3", record[1]);
                Assert.AreEqual("Female", record[2]);
                Assert.IsNull(record["Employer"]);
                Assert.Throws<KeyNotFoundException>(() => record["ASDF"].Noop());

                Assert.False(it.MoveNext());
            }
        }
    }
}
