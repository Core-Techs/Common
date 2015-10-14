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

        public long Position { get; }

        public StreamBookmark(Stream stream)
        {
            _stream = stream;
            Position = stream.Position;
        }

        public void Dispose()
        {
            _stream.Seek(Position, SeekOrigin.Begin);
        }
    }
}