using System.Collections.Generic;

namespace CoreTechs.Common.Text
{
    public interface ICsvWritable
    {
        IEnumerable<object> GetCsvHeadings();
        IEnumerable<object> GetCsvFields();
    }
}