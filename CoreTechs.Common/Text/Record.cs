using System;
using System.Collections.Generic;

namespace CoreTechs.Common.Text
{
    /// <summary>
    /// A dictionary that also allows numeric index access to data items.
    /// </summary>
    public class Record : Dictionary<string, string>
    {
        readonly string[] _data;
        public Record(IEnumerable<string> names, string[] data, IEqualityComparer<string> stringComparer)
            : base(stringComparer)
        {
            if (names == null) throw new ArgumentNullException("names");
            if (data == null) throw new ArgumentNullException("data");

            _data = data;
            var i = 0;
            foreach (var name in names)
            {
                this[name] = data.Length > i
                    ? data[i++]
                    : null;
            }
        }

        public string this[int index]
        {
            get
            {
                return _data[index];
            }
        }
    }
}