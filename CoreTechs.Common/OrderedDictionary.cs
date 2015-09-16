using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoreTechs.Common
{
    public class OrderedDictionary<TKey, TValue> : IdentifiedCollection<TKey, KeyValuePair<TKey, TValue>>, IDictionary<TKey,TValue>
    {
        public OrderedDictionary(IEqualityComparer<TKey> keyComparer = null)
            : base(x => x.Key, keyComparer)
        {
        }

        public void Add(TKey key, TValue value)
        {
            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<TKey, TValue> pair;
            if (base.TryGetValue(key, out pair))
            {
                value = pair.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public new TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                    return value;
                
                throw new KeyNotFoundException(string.Format("The key \"{0}\" was not found.", key));
            }
            set { Upsert(new KeyValuePair<TKey, TValue>(key, value)); }
        }

        public ICollection<TKey> Keys
        {
            get { return Dictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return new ReadOnlyCollection<TValue>(Items.Select(x => x.Value).ToArray()); }
        }
    }
}