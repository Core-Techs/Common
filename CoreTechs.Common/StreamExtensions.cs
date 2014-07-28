using System;
using System.Collections.Generic;
using System.IO;

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
    }
}