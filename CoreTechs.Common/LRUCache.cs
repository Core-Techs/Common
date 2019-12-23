using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CoreTechs.Common
{
    /// <summary>
    /// A generic, thread-safe, probably-naive cache that discards the least recently used
    /// items when the cache has reached capacity.
    /// </summary>
    /// <remarks>
    /// Deadlock potential has been intentionally exposed by GetEnumerator().
    /// The caller shouldn't attempt a write operation while enumerating the cache.
    /// Don't be a dummy.
    /// 
    /// The implementation uses a dictionary and a linked-list.
    /// The linked list is used to position the key/value pairs in order of
    /// most recently used to least recently used. The linked list allows for
    /// fast transplantation of key/value pairs from any point in the list to
    /// the head of the list.
    /// The dictionary allows fast key-based access to the linked list nodes.
    /// 
    /// A R/W lock is used to syncronize access of the 2 collections. Most operations
    /// that require syncronization will only acquire a write-lock after a read-lock
    /// has been proven insufficient.
    /// 
    /// No attempt at atomicity was made when interacting with the underlying collections.
    /// If an exception is thrown before writing to the last collection, it should be assumed
    /// that the cache instance is in an inconsistent state. Duh.
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LRUCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private  int _capacity;

        public int Capacity
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return _capacity;
                }
                finally
                {
                    Lock.ExitReadLock();
                }

            }
            set
            {
                if (Capacity == value)
                    return;

                Lock.EnterWriteLock();
                try
                {
                     _capacity = value;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

            }
        }

        protected readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> Dict
            = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

        protected readonly LinkedList<KeyValuePair<TKey, TValue>> List
            = new LinkedList<KeyValuePair<TKey, TValue>>();

        protected readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public LRUCache(int capacity)
        {
            _capacity = capacity;
        }

        /// <summary>
        /// The number of items in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return Dict.Count;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// All the keys in the cache,
        /// ordered from most recently used
        /// to least recently used.
        /// </summary>
        public TKey[] Keys
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return List.Select(x => x.Key).ToArray();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// All the values in the cache,
        /// ordered from most recently used
        /// to least recently used.
        /// </summary>
        public TValue[] Values
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    return List.Select(x => x.Value).ToArray();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// The most recently used value in the cache.
        /// </summary>
        public TValue Newest
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (List.Count == 0)
                        throw new InvalidOperationException("The cache is empty.");

                    return List.First.Value.Value;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// The least recently used value in the cache.
        /// </summary>
        public TValue Oldest
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (List.Count == 0)
                        throw new InvalidOperationException("The cache is empty.");

                    return List.Last.Value.Value;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Fired when the oldest item is discarded.
        /// </summary>
        public event EventHandler<KeyValuePair<TKey, TValue>> OldestItemRemoved;

        protected virtual void OnOldestItemRemoved(KeyValuePair<TKey, TValue> e)
        {
            var handler = OldestItemRemoved;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Gets an value by key from the cache. If the key is not found
        /// the factory is invoked to produce the value and it is stored
        /// in the cache before being returned. If the cache is full 
        /// the oldest item in the cache will be removed.
        /// </summary>
        /// <param name="key">The key to find the item by.</param>
        /// <param name="factory">The function that's invoked to create the value
        /// when the key is not found. The function takes the key as an argument.</param>
        /// <returns>The value.</returns>
        public TValue Get(TKey key, Func<TKey, TValue> factory)
        {
            if (ReferenceEquals(key, null)) throw new ArgumentNullException(nameof(key));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            LinkedListNode<KeyValuePair<TKey, TValue>> item = null, removed = null;

            Lock.EnterReadLock();
            try
            {
                if (Dict.ContainsKey(key))
                {
                    item = Dict[key];

                    if (List.First == item)
                        // best case scenario
                        // no write lock required
                        return item.Value.Value;
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            Lock.EnterWriteLock();
            try
            {
                // was item created during our read attempt
                // or was it created by another thread?
                if (item == null && Dict.ContainsKey(key))
                    item = Dict[key];

                if (item != null)
                {
                    // mark item as recently used
                    List.Remove(item);
                    List.AddFirst(item);
                    return item.Value.Value;
                }

                // key not found
                // create the value
                var value = factory(key);

                // store in collections
                item = List.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
                Dict.Add(key, item);

                // check cap
                if (List.Count > _capacity)
                {
                    // get rid of oldest item
                    removed = List.Last;
                    Dict.Remove(removed.Value.Key);
                    List.Remove(removed);
                }

                return value;
            }
            catch
            {
                // don't fire the ItemRemoved event
                // after exiting the write lock
                removed = null;
                throw;
            }
            finally
            {
                Lock.ExitWriteLock();

                if (removed != null)
                    OnOldestItemRemoved(removed.Value);
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            if (Count == 0)
                return;

            Lock.EnterWriteLock();
            try
            {
                Dict.Clear();
                List.Clear();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the item with the given key from the cache.
        /// </summary>
        /// <returns>
        /// Whether the item was removed or not. 
        /// If false is returned it means that the item was never in the cache 
        /// or that another thread removed the item.
        /// </returns>
        public bool Remove(TKey key)
        {
            Lock.EnterUpgradeableReadLock();
            try
            {
                if (!Dict.ContainsKey(key))
                    return false;

                Lock.EnterWriteLock();
                try
                {
                    var item = Dict[key];
                    Dict.Remove(key);
                    List.Remove(item);

                    return true;
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
            finally
            {
                Lock.ExitUpgradeableReadLock();
            }
        }

        /// <remarks>
        /// DO NOT ATTEMPT WRITE OPERATIONS ON THE CACHE WHILE ENUMERATING.
        /// IT WILL DEADLOCK!
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Lock.EnterReadLock();
            try
            {
                foreach (var pair in List)
                    yield return pair; // danger: deadlock potential
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
