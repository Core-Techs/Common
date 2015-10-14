using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreTechs.Common
{
    public enum UpsertResult
    {
        Updated,
        Inserted,
    }

    /// <summary>
    /// Interface describing a collection that stores items that can be quickly looked up by a key.
    /// It is up to the implementation of the collection whether all items must have a key or whether
    /// some items may be stored without quick lookup capability.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IIdentifiedCollection<TKey, TItem> : ICollection<TItem>
    {
        /// <summary>
        /// Attempt to update/replace an item (looked up by key). If the key of the item is not in the
        /// collection, it will be inserted. If the collection supports null keys, null keyed items
        /// will always be inserted.
        /// </summary>
        /// <param name="item">The item to upsert.</param>
        /// <returns>Whether upsert resulted in an update or insertion</returns>
        UpsertResult Upsert(TItem item);
        bool Remove(TKey key);
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TItem value);
        TItem this[TKey key] { get; }

        IReadOnlyDictionary<TKey, TItem> AsReadOnlyDictionary();
    }

    /// <summary>
    /// Class which represents a collection of items which can be looked up with a key. The key for
    /// the items is described by the <see cref="KeyFinder"/> delegate provided to the class.
    /// Typically, the key will be embedded in the items themselves in some way.
    /// </summary>
    /// <remarks>
    /// The collection's iteration order is based on the insertion order of the elements.
    /// 
    /// It is not required that the <see cref="KeyFinder"/> always return a non-null key. In the case
    /// that the key is nullable and <see cref="KeyFinder"/> results in null, the item being stored
    /// will not be reachable using the look-up-by-key methods.
    /// 
    /// This class is NOT thread-safe.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TItem">The type of items in the collection</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class IdentifiedCollection<TKey, TItem> : IIdentifiedCollection<TKey, TItem>
    {
        /// <summary>
        /// A function that takes an item and returns a key that should be usable to identify the item.
        /// </summary>
        /// <remarks>
        /// For correct functioning, it is required that the state changes of the item not
        /// affect the key returned by the delegate. Furthermore, the key identifying the item
        /// is expected to be unique in the collection. Adding a different item that provides the
        /// same key as an item already in the <see cref="IdentifiedCollection{TKey,TItem}"/>
        /// (with exception of null keys) is considered an error and will throw an exception.
        /// In the case that the key finder returns null for an item, the item can still be stored,
        /// but it will not have a fast (keyed) way to look it up again.
        /// </remarks>
        /// <param name="item">The item used to produce the key.</param>
        /// <returns>
        /// A key that can be used to identify/look up the item. If the key type is nullable,
        /// this is allowed to return null.
        /// </returns>
        public delegate TKey KeyFinder(TItem item);

        protected readonly Dictionary<TKey, TItem> Dictionary;
        protected readonly List<TItem> Items;
        private readonly KeyFinder _keyFinder;

        /// <summary>
        /// Initializes a new instance of the class using the provided delegate as the
        /// key finder for this <see cref="IdentifiedCollection{TKey,TItem}"/> and using
        /// the default key equality
        /// keyComparer.
        /// </summary>
        /// <param name="keyFinder">A delegate following the contract of <see cref="KeyFinder"/>.</param>
        public IdentifiedCollection(KeyFinder keyFinder)
            : this(keyFinder, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class using the provided delegate as the
        /// key finder for this <see cref="IdentifiedCollection{TKey,TItem}"/> and using
        /// the specified key equality
        /// keyComparer.
        /// </summary>
        /// <param name="keyFinder">A delegate following the contract of <see cref="KeyFinder"/>.</param>
        /// <param name="keyComparer">An IEqualityComparer used to distinguish keys.</param>
        public IdentifiedCollection(KeyFinder keyFinder, IEqualityComparer<TKey> keyComparer)
        {
            if (keyFinder == null) throw new ArgumentNullException(nameof(keyFinder));
            _keyFinder = keyFinder;
            Items = new List<TItem>();
            Dictionary = new Dictionary<TKey, TItem>(keyComparer ?? EqualityComparer<TKey>.Default);
        }

        #region ICollection Implementation
        public int Count => Items.Count;
        public bool IsReadOnly => false;

        public void Add(TItem item)
        {
            AddNonNullKeyFor(item);
            Items.Add(item);
        }

        public bool Remove(TItem item)
        {
            var key = _keyFinder(item);
            if (key == null) return Items.Remove(item);

            return Dictionary.Remove(key) && Items.Remove(item);
        }

        public void Clear()
        {
            Dictionary.Clear();
            Items.Clear();
        }

        public bool Contains(TItem item)
        {
            var key = _keyFinder(item);
            if (key == null) return Items.Contains(item);

            TItem itemFromDictionary;
            return Dictionary.TryGetValue(key, out itemFromDictionary) && EqualityComparer<TItem>.Default.Equals(item, itemFromDictionary);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion ICollection Implementation

        #region Dictionary-like Functionality

        /// <summary>
        /// Attempt to update/replace an item (looked up by key). If the key of the item is not in the
        /// collection, it will be inserted. If the collection supports null keys, null keyed items
        /// will always inserted.
        /// </summary>
        /// <remarks>
        /// The effect on the order of the enumeration is unspecified for now. This may either result
        /// in the replacement of the position of the item or the removal of the existing item and
        /// appending of the new item.
        /// If there is a "good reason" to specify this in the future, the implementation may change.
        /// </remarks>
        /// <param name="item">The item to upsert.</param>
        /// <returns>Whether upsert resulted in an update or insertion</returns>
        public UpsertResult Upsert(TItem item)
        {
            var key = _keyFinder(item);
            if (key == null)
            {
                Add(item);
                return UpsertResult.Inserted;
            }

            TItem existingItem;
            if (!Dictionary.TryGetValue(key, out existingItem))
            {
                Add(item);
                return UpsertResult.Inserted;
            }

            if (ReferenceEquals(item, existingItem))
                return UpsertResult.Updated;

            var indexOfExistingItem = Items.IndexOf(existingItem);
            Dictionary[key] = item;
            Items[@indexOfExistingItem] = item;
            return UpsertResult.Updated;
        }

        public bool Remove(TKey key)
        {
            TItem value;
            return Dictionary.TryGetValue(key, out value) && Remove(value);
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TItem value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public TItem this[TKey key] => Dictionary[key];

        public IReadOnlyDictionary<TKey, TItem> AsReadOnlyDictionary()
        {
            return Dictionary;
        }
        #endregion Dictionary-like Functionality

        private void AddNonNullKeyFor(TItem item)
        {
            var key = _keyFinder(item);
            if (key != null) Dictionary.Add(key, item);
        }
    }
}
