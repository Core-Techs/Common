using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreTechs.Common
{
    public static class StreamExtensions
    {
        public static IEnumerable<byte> EnumerateBytes(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            int b;
            while ((b = stream.ReadByte()) != -1)
                yield return (byte) b;
        }

        public static MemoryStream ToMemoryStream(this IEnumerable<byte> bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");

            return new MemoryStream(bytes.ToArray());
        }
    }
}