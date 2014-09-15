using System;
using System.IO;

namespace CoreTechs.Common
{
    /// <summary>
    /// Records the current stream position on construction
    /// and seeks back to it on disposal.
    /// </summary>
    public class StreamBookmark : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _position;

        public long Position { get { return _position; } }

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