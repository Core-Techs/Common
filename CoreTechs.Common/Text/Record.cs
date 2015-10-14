using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common.Text
{
    /// <summary>
    /// A dictionary that also allows numeric index access to data items.
    /// </summary>
    public class Record : IReadOnlyDictionary<string, string>, IReadOnlyList<string>
    {
        private readonly string[] _data;
        private readonly Dictionary<string, string> _dict;

        public Record(IEnumerable<string> names, string[] data, IEqualityComparer<string> stringComparer)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (data == null) throw new ArgumentNullException(nameof(data));

            _dict = new Dictionary<string, string>(stringComparer);
            _data = data;
            var i = 0;
            foreach (var name in names)
            {
                _dict[name] = data.Length > i
                    ? data[i++]
                    : null;
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return _data.Cast<string>().GetEnumerator();
        }

         IEnumerator<KeyValuePair<string, string>>  IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
         {
             return _dict.GetEnumerator();
         }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        public int Count => _data.Length;

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public string this[string key] => _dict[key];

        public string this[int index] => _data[index];

        public IEnumerable<string> Keys => _dict.Keys;

        public IEnumerable<string> Values => _dict.Values;
    }
}