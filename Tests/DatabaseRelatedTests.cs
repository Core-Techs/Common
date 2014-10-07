using System.Data;
using System.Linq;
using CoreTechs.Common.Database;
using NUnit.Framework;

namespace Tests
{
    public class DatabaseRelatedTests
    {
        [Test]
        public void CanSelectObjInstancesFromRows()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof (int));
            dt.Columns.Add("Name", typeof (string));

            var row = dt.NewRow();
            row["Id"] = 1;
            row["Name"] = "Ronnie";

            dt.Rows.Add(row);

            var x = dt.AsEnumerable<MyClass>().Single();

            Assert.NotNull(x);
            Assert.AreEqual(1,x.Id);
            Assert.AreEqual("Ronnie", x.Name);

        }

        class MyClass
        {
            public int Id { get; set; } 
            public string Name { get; set; } 
        }
    }
}