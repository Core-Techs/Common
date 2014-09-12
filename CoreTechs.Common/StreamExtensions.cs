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
        /// Seeks to the next occurrence of the target string in the stream.
        /// </summary>
        /// <returns>The starting position of the string or null if not found.</returns>
        public static long? SeekTo(this Stream stream, string target, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Default;
            var bytes = encoding.GetBytes(target);
            return SeekTo(stream, bytes);
        }

        /// <summary>
        /// Seeks to the next occurrence of the target byte sequence in the stream.
        /// </summary>
        /// <returns>The starting position of the sequence or null if not found.</returns>
        public static long? SeekTo(this Stream stream, byte[] target)
        {
            var t = 0;

            foreach (var b in stream.EnumerateBytes())
            {
                if ((b == target[t] || b == target[t = 0]) && target.Length == t + 1)
                {
                    stream.Seek(-target.Length, SeekOrigin.Current);
                    return stream.Position;
                }

                t++;
            }

            return null;
        }
    }
}