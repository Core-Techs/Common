using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    /// <summary>
    /// Wraps another <see cref="IEnumerable{T}"/> so that a background task can
    /// pre-fetch items from the underlying sequence into a buffer that the consumer
    /// can then enumerate.
    /// </summary>
    public class PreFetchingEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly int? _capacity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">
        /// The underlying enumerable sequence. 
        /// The source sequence is enumerated on demand (when the wrapping <see cref="PreFetchingEnumerable{T}"/> is enumerated).</param>
        /// <param name="capacity">
        /// The bounded capacity for the buffer.
        /// When set to null, the background task will enumerate the source without pausing to wait on the consumer to read from the buffer.
        /// The default value of 2 allows the producer thread to pre-fetch a single element before it is requested by the consumer.
        /// Increasing the value will allow more items to be pre-fetched from the source enumerable.
        /// Be careful to not exhaust memory resources.
        /// </param>
        public PreFetchingEnumerable(IEnumerable<T> source, int? capacity = 2)
        {
            if (source == null) 
                throw new ArgumentNullException("source");

            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity", "capacity must not be less than 1");

            _source = source;
            _capacity = capacity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            using (var buffer = CreateBuffer())
            using (var producer = Task.Run(() =>
            {
                try
                {
                    foreach (var item in _source)
                        buffer.Add(item);
                }
                finally
                {
                    buffer.CompleteAdding();
                }
            }))
            {
                foreach (var item in buffer.GetConsumingEnumerable())
                    yield return item;

                // wait to expose exceptions
                producer.Wait();
            }
        }

        private BlockingCollection<T> CreateBuffer()
        {
            if (_capacity.HasValue) 
                return new BlockingCollection<T>(_capacity.Value);

            return new BlockingCollection<T>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}