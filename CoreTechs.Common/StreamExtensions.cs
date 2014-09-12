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
        public static string SeekStartOfAny(this Stream stream,  params string[] targets)
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
            var t = new int[targets.Length];

            foreach (var b in stream.EnumerateBytes())
            {
                for (var ti = 0; ti < targets.Length; ti++)
                {
                    var target = targets[ti];

                    if (b != target[t[ti]] && b != target[t[ti] = 0])
                        continue;

                    if (target.Length == t[ti] + 1)
                        return targets[ti];

                    t[ti]++;
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

        public static long CountBytesUntil(this Stream stream, byte[] target)
        {
            var start = stream.Position;
            var found = stream.SeekEndOf(target);
            var end = stream.Position;
            var count = end - start;

            if (found)
                count -= target.Length;

            stream.Seek(start, SeekOrigin.Begin);
            return count;
        }

        public static long CountBytesUntil(this Stream stream, string target, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(target);
            return CountBytesUntil(stream, bytes);
        }

        public static IEnumerable<byte> EnumerateBytesUntil(this Stream stream, string target, bool positionAfterTarget = false, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(target);
            return EnumerateBytesUntil(stream, bytes, positionAfterTarget);
        }

        public static IEnumerable<byte> EnumerateBytesUntil(this Stream stream, byte[] target, bool positionAfterTarget = false)
        {
            var count = stream.CountBytesUntil(target);
            foreach (var b in stream.EnumerateBytes().Take(count))
                yield return b;


            if (positionAfterTarget)
                stream.Seek(target.Length, SeekOrigin.Current);
        }

        public static StreamBookmark Bookmark(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return new StreamBookmark(stream);
        }
    }
}