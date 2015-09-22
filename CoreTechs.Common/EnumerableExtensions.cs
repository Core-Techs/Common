using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SkipWhileNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.SkipWhile(x => !predicate(x));
        }

        public static IEnumerable<T> TakeWhileNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.TakeWhile(x => !predicate(x));
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Where(x => !predicate(x));
        }

        public static IEnumerable<IEnumerable<T>> Buffer<T>(this IEnumerable<T> source, int bufferSize)
        {
            return source.Consume(x => x.Take(bufferSize));
        }

        /// <summary>
        /// Calls the supplied func on the source sequence until it has been consumed.
        /// Your func will receive a sequence beginning where the previous invocation left off.
        /// </summary>
        public static IEnumerable<TResult> Consume<TSource, TResult>(this IEnumerable<TSource> seq, Func<IEnumerable<TSource>, TResult> func)
        {
            if (seq == null) throw new ArgumentNullException("seq");
            if (func == null) throw new ArgumentNullException("func");

            using (var it = seq.GetEnumerator())
                while (it.MoveNext())
                    yield return func(it.AsEnumerable(true, false));
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator, bool primed = false, bool disposeEnumerator = false)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");

            using (disposeEnumerator ? enumerator : null)
            {
                if (primed)
                    yield return enumerator.Current;

                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Shuffles an enumeration of items.
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng = null)
        {
            return source.ToArray().Shuffle(rng);
        }

        /// <summary>
        /// Shuffles a collection of items.
        /// </summary>
        /// <thanks to="Jon Skeet">http://stackoverflow.com/a/1287572/64334</thanks>
        public static IEnumerable<T> Shuffle<T>(this IList<T> list, Random rng = null)
        {
            rng = rng ?? RNG.Instance;
            var elements = new T[list.Count];
            list.CopyTo(elements, 0);
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                var swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        /// <summary>
        /// Yields an object indefinetely.
        /// </summary>
        public static IEnumerable<T> Repeat<T>(this T source)
        {
            while (true) yield return source;
        }

        /// <summary>
        /// Repeatedly enumerates the source, yielding it's items.
        /// </summary>
        public static IEnumerable<T> RepeatMany<T>(this IEnumerable<T> source)
        {
            return source.Repeat().SelectMany();
        }

        /// <summary>
        /// Flattens an enumerable of enumerables.
        /// </summary>
        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(x => x);
        }

        /// <summary>
        /// Repeatedly invokes the delegate and yields it's result.
        /// </summary>
        public static IEnumerable<T> InvokeRepeatedly<T>(this Func<T> func)
        {
            while (true) yield return func();
        }

        /// <thanks to="Jon Skeet">http://stackoverflow.com/a/648240/64334</thanks>
        public static T RandomElement<T>(this IEnumerable<T> source, Random rng = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            rng = rng ?? RNG.Instance;

            var list = source as IList<T>;
            if (list != null) return list.RandomElement(rng);

            var current = default(T);
            var count = 0;
            foreach (var element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                    current = element;
            }

            if (count == 0)
                throw new InvalidOperationException("Sequence was empty");

            return current;
        }

        // more efficient for collections with an indexer
        public static T RandomElement<T>(this IList<T> list, Random rng = null)
        {
            if (list == null) throw new ArgumentNullException("list");

            if (list.Count == 0)
                throw new InvalidOperationException("List was empty");

            rng = rng ?? RNG.Instance;
            var i = rng.Next(list.Count);
            var element = list[i];
            return element;
        }

        public static T GetNextOrDefault<T>(this IEnumerator<T> source, T @default = default(T))
        {
            if (source == null) throw new ArgumentNullException("source");
            return !source.MoveNext() ? @default : source.Current;
        }

        public static IEnumerable<List<T>> Split<T>(this IEnumerable<T> source, T delim)
        {
            return source.SplitWhere(x => x.Equals(delim));
        }

        public static IEnumerable<List<T>> SplitWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var list = new List<T>();
            var lastWasDelim = false;

            foreach (var item in source)
            {
                lastWasDelim = false;
                if (predicate(item))
                {
                    yield return list;
                    list = new List<T>();
                    lastWasDelim = true;
                }
                else list.Add(item);
            }

            if (list.Any() || lastWasDelim)
                yield return list;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        /// Gets the last n items of the enumerable.
        /// </summary>
        [Obsolete("Use Tail instead", false)]
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return Tail(source, count);
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> enumerable, long count)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            var it = enumerable.GetEnumerator();
            for (long i = 0; i < count && it.MoveNext(); i++)
                yield return it.Current;
        }

        public static void Enumerate(this IEnumerable enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            foreach (var item in enumerable)
                item.Noop();
        }

        public static IEnumerable<T[]> AccumulateUntil<T>(this IEnumerable<T> source,
        Func<T, bool> predicate)
        {
            return AccumulateWhile(source, predicate.Invert());
        }

        public static IEnumerable<T[]> AccumulateWhile<T>(this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            var accumulated = new List<T>();

            foreach (var item in source)
            {
                if (!predicate(item) && accumulated.Any())
                {
                    yield return accumulated.ToArray();
                    accumulated.Clear();
                }

                accumulated.Add(item);
            }

            if (accumulated.Any()) yield return accumulated.ToArray();
        }

        /// <summary>
        /// Wraps another <see cref="IEnumerable{T}"/> so that a background task can
        /// pre-fetch items from the underlying sequence into a buffer that the consumer
        /// can then enumerate.
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
        public static PreFetchingEnumerable<T> PreFetch<T>(this IEnumerable<T> source, int? capacity = 2)
        {
            return new PreFetchingEnumerable<T>(source, capacity);
        }

        public static BufferedEnumerator<T> GetBufferedEnumerator<T>(this IEnumerable<T> enumerable, int? capacity = null)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            return new BufferedEnumerator<T>(enumerable.GetEnumerator(), capacity, true);
        }

        /// <summary>
        /// Produces a sequence that excludes the last elements of the source sequence.
        /// </summary>
        /// <param name="seq">The source sequence</param>
        /// <param name="tailLength">The length of the tail to be excluded.</param>
        public static IEnumerable<T> ExceptTail<T>(this IEnumerable<T> seq, int? tailLength = null)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            if (tailLength == null)
            {
                // tail length is count - 1

                foreach (var item in seq.Take(1))
                    yield return item;

                yield break;
            }

            if (tailLength < 0)
                throw new ArgumentOutOfRangeException("tailLength");

            var arr = seq as IList<T>;
            if (arr != null)
            {
                // count is known. no need to buffer.

                for (var i = 0; i < arr.Count - tailLength; i++)
                    yield return arr[i];

                yield break;
            }

            // count is unknown. store items in the linked list.

            var list = new LinkedList<T>();

            foreach (var item in seq)
            {
                list.AddLast(item);

                if (list.Count > tailLength)
                {
                    yield return list.First.Value;
                    list.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="seq">The source sequence</param>
        /// <param name="tailLength">The length of the tail.</param>
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> seq, int? tailLength = null)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");

            if (tailLength == null)
            {
                foreach (var item in seq.Skip(1))
                    yield return item;

                yield break;
            }

            var arr = seq as IList<T>;
            if (arr != null)
            {
                // count is known. no need to buffer.

                for (var i = arr.Count - tailLength.Value; i < arr.Count; i++)
                    yield return arr[i];

                yield break;
            }

            // count is unknown. store items into the linked list.

            var list = new LinkedList<T>();

            foreach (var item in seq)
            {
                list.AddLast(item);

                if (list.Count > tailLength)
                    list.RemoveFirst();
            }

            foreach (var item in list)
                yield return item;
        }

        /// <summary>
        /// Gets the distinct elements of sequence based on what equalsImpl decides are equal elements.
        /// </summary>
        /// <remarks>Provide a hashImpl for more speed.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence that is being enumerated</param>
        /// <param name="equalsImpl">
        ///     An implementation of function that tests equality of the type emitted by the enumerable
        /// </param>
        /// <param name="hashImpl">
        ///     Optional. An implementation that returns a hash that can be used to improve performance
        /// </param>
        /// <exception cref="System.ArgumentNullException">When either seq or equalsImpl are null</exception>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> seq,
                                                 Func<T, T, bool> equalsImpl,
                                                 Func<T, int> hashImpl = null)
        {
            if (seq == null)
                throw new ArgumentNullException("seq");
            if (equalsImpl == null)
                throw new ArgumentNullException("equalsImpl");

            var comparer = new DelegateEqualityComparer<T>(equalsImpl, hashImpl);
            return seq.Distinct(comparer);
        }
    }
}
