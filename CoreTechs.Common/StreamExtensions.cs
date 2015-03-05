using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoreTechs.Common
{
    public static class StreamExtensions
    {
        public static IEnumerable<byte> EnumerateBytes(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            int b;
            while ((b = stream.ReadByte()) != -1)
                yield return (byte)b;
        }

        public static MemoryStream ToMemoryStream(this IEnumerable<byte> bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");

            return new MemoryStream(bytes.ToArray());
        }

        /// <summary>
        /// Seeks to the position of the next occurrence of the target string in the stream.
        /// </summary>
        /// <returns>True if found. False if not found.</returns>
        public static bool SeekStartOf(this Stream stream, string target, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(target);
            return SeekStartOf(stream, bytes);
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence of the target string in the stream.
        /// </summary>
        /// <returns>True if found. False if not found.</returns>
        public static bool SeekEndOf(this Stream stream, string target, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(target);
            return SeekEndOf(stream, bytes);
        }

        /// <summary>
        /// Seeks to the position of the next occurrence of the target byte sequence in the stream.
        /// </summary>
        /// <returns>True if found. False if not found.</returns>
        public static bool SeekStartOf(this Stream stream, byte[] target)
        {
            if (!SeekEndOf(stream, target))
                return false;

            stream.Seek(-target.Length, SeekOrigin.Current);
            return true;
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence of the target byte sequence in the stream.
        /// </summary>
        /// <returns>True if found. False if not found.</returns>
        public static bool SeekEndOf(this Stream stream, byte[] target)
        {
            var found = SeekEndOfAny(stream, new[] { target });
            return found != null;
        }


        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target byte sequences found in the stream.
        /// </summary>
        /// <returns>The target byte sequence that was first found or null.</returns>
        public static string SeekStartOfAny(this Stream stream, params string[] targets)
        {
            return SeekStartOfAny(stream, null, targets);
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target byte sequences found in the stream.
        /// </summary>
        /// <returns>The target byte sequence that was first found or null.</returns>
        public static string SeekStartOfAny(this Stream stream, Encoding encoding, params string[] targets)
        {
            var encodedTargets = targets.Select(s => s.Encode(encoding)).ToArray();
            var found = SeekStartOfAny(stream, encodedTargets);

            if (found == null)
                return null;

            stream.Seek(-found.Length, SeekOrigin.Current);
            return found.Decode();
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target byte sequences found in the stream.
        /// </summary>
        /// <returns>The target byte sequence that was first found or null.</returns>
        public static byte[] SeekStartOfAny(this Stream stream, params byte[][] targets)
        {
            var found = SeekEndOfAny(stream, targets);

            if (found == null)
                return null;

            stream.Seek(-found.Length, SeekOrigin.Current);
            return found;
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target byte sequences found in the stream.
        /// </summary>
        /// <returns>The target byte sequence that was first found or null.</returns>
        public static byte[] SeekEndOfAny(this Stream stream, params byte[][] targets)
        {
            // contains the next expected target byte index
            // for the index-correlated target :)
            var expectations = new int[targets.Length];

            foreach (var currentByte in stream.EnumerateBytes())
            {
                for (var targetIndex = 0; targetIndex < targets.Length; targetIndex++)
                {
                    var target = targets[targetIndex];

                    // test current byte against target expectation
                    // if failed and not the first byte of target, 
                    // start over, trying the first byte of the target
                    if (currentByte != target[expectations[targetIndex]])
                        if (expectations[targetIndex] == 0 || currentByte != target[expectations[targetIndex] = 0])
                            continue;

                    // test target has been completely matched
                    if (target.Length == expectations[targetIndex] + 1)
                        return target;

                    // not completely matched,
                    // but the current byte meets the targets current expectation
                    // advance the expected byte index
                    expectations[targetIndex]++;
                }
            }

            return null;
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target strings found in the stream.
        /// </summary>
        /// <returns>The target string that was first found or null.</returns>
        public static string SeekEndOfAny(this Stream stream, params string[] targets)
        {
            return SeekEndOfAny(stream, null, targets);
        }

        /// <summary>
        /// Seeks to the position after the end of the next occurrence 
        /// of any of the target strings found in the stream.
        /// </summary>
        /// <returns>The target string that was first found or null.</returns>
        public static string SeekEndOfAny(this Stream stream, Encoding encoding, params string[] targets)
        {
            var encodedTargets = targets.Select(s => s.Encode(encoding)).ToArray();
            var found = SeekEndOfAny(stream, encodedTargets);
            return found == null ? null : found.Decode(encoding);
        }

        /// <summary>
        /// Counts the number of bytes between the current position and the target byte sequence.
        /// The starting position is returned to once the target is found or the end of the stream is reached.
        /// </summary>
        public static long CountBytesUntil(this Stream stream, byte[] target)
        {
            byte[] targetFound;
            return CountBytesUntilAny(stream, new[] { target }, out targetFound);
        }

        /// <summary>
        /// Counts the number of bytes between the current position and the target string.
        /// The starting position is returned to once the target is found or the end of the stream is reached.
        /// </summary>
        public static long CountBytesUntil(this Stream stream, string target, Encoding encoding = null)
        {
            string targetFound;
            return CountBytesUntilAny(stream, new[] { target }, out targetFound, encoding);
        }

        /// <summary>
        /// Counts the number of bytes between the current position and the first occurrence of any of the target byte sequences.
        /// The starting position is returned to once a target is found or the end of the stream is reached.
        /// </summary>
        public static long CountBytesUntilAny(this Stream stream, byte[][] targets, out byte[] targetFound)
        {
            using (var start = stream.Bookmark())
            {
                targetFound = stream.SeekEndOfAny(targets);
                var end = stream.Position;
                var count = end - start.Position;

                if (targetFound != null)
                    count -= targetFound.Length;

                return count;
            }
        }


        /// <summary>
        /// Counts the number of bytes between the current position and the first occurrence of any of the target strings.
        /// The starting position is returned to once a target is found or the end of the stream is reached.
        /// </summary>
        /// <param name="targetFound">The string that was found first or null if no target was found.</param>
        public static long CountBytesUntilAny(this Stream stream, string[] targets, out string targetFound,
            Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = targets.Select(x => encoding.GetBytes(x)).ToArray();
            byte[] bytesFound;
            var count = CountBytesUntilAny(stream, bytes, out bytesFound);
            targetFound = bytesFound == null ? null : bytesFound.Decode(encoding);
            return count;
        }

        /// <summary>
        /// Enumerates the bytes until the target begins or the end of the stream.
        /// </summary>
        /// <param name="target">The target to search for.</param>
        /// <param name="positionAfterTarget">True to seek to the end of the target after enumeration; false to keep position at beginning of target.</param>
        /// <param name="encoding">The encoding used to decode the target string.</param>
        /// <returns>An enumerable of bytes.</returns>
        public static IEnumerable<byte> EnumerateBytesUntil(this Stream stream, string target,
            bool positionAfterTarget = false, Encoding encoding = null)
        {
            return EnumerateBytesUntilAny(stream, new[] { target }, positionAfterTarget, encoding);
        }


        /// <summary>
        /// Enumerates the bytes until the target begins or the end of the stream.
        /// </summary>
        /// <param name="target">The target to search for.</param>
        /// <param name="positionAfterTarget">True to seek to the end of the target after enumeration; false to keep position at beginning of target.</param>
        /// <returns>An enumerable of bytes.</returns>
        public static IEnumerable<byte> EnumerateBytesUntil(this Stream stream, byte[] target,
            bool positionAfterTarget = false)
        {
            return EnumerateBytesUntilAny(stream, new[] { target }, positionAfterTarget);
        }



        /// <summary>
        /// Enumerates the bytes until any of the target byte sequences begin or the end of the stream.
        /// </summary>
        /// <param name="targets">The targets to search for.</param>
        /// <param name="positionAfterTarget">True to seek to the end of the found target after enumeration; false to keep position at beginning of target.</param>
        /// <returns>An enumerable of bytes.</returns>
        public static IEnumerable<byte> EnumerateBytesUntilAny(this Stream stream, byte[][] targets,
            bool positionAfterTarget = false)
        {
            byte[] targetFound;
            var count = stream.CountBytesUntilAny(targets, out targetFound);
            foreach (var b in stream.EnumerateBytes().Take(count))
                yield return b;

            if (positionAfterTarget)
                stream.Seek(targetFound.Length, SeekOrigin.Current);
        }


        /// <summary>
        /// Enumerates the bytes until any of the target strings begin or the end of the stream.
        /// </summary>
        /// <param name="targets">The targets to search for.</param>
        /// <param name="positionAfterTarget">True to seek to the end of the found target after enumeration; false to keep position at beginning of target.</param>
        /// <param name="encoding">The encoding used to decode the target strings.</param>
        /// <returns>An enumerable of bytes.</returns>
        public static IEnumerable<byte> EnumerateBytesUntilAny(this Stream stream, string[] targets,
            bool positionAfterTarget = false, Encoding encoding = null)
        {
            string targetFound;
            var count = stream.CountBytesUntilAny(targets, out targetFound, encoding);
            foreach (var b in stream.EnumerateBytes().Take(count))
                yield return b;

            if (positionAfterTarget)
                stream.Seek(targetFound.Length, SeekOrigin.Current);
        }

        /// <summary>
        /// Creates a <see cref="StreamBookmark"/>.
        /// </summary>
        public static StreamBookmark Bookmark(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return new StreamBookmark(stream);
        }
    }
}
