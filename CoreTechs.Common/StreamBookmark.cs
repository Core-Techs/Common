using System;
using System.IO;

namespace CoreTechs.Common
{
    public class StreamBookmark : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _position;

        public StreamBookmark(Stream stream)
        {
            _stream = stream;
            _position = stream.Position;
        }

        public void Dispose()
        {
            _stream.Seek(_position, SeekOrigin.Begin);
        }
    }
}