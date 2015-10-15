using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreTechs.Common
{
    /// <summary>
    /// A stream implementation for enumerable sequences.
    /// </summary>
    public class EnumerableStream : Stream
    {
        private readonly IEnumerator<IEnumerable<byte>> _it;
        private readonly MemoryStream _ms = new MemoryStream();
        private long _pos;

        public EnumerableStream(IEnumerable<IEnumerable<byte>> sequence)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            _it = sequence.GetEnumerator();
        }

        public static EnumerableStream ForEnumerable<T>(IEnumerable<T> sequence, Func<T, IEnumerable<byte>> serializer)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            return new EnumerableStream(sequence.Select(serializer));
        }

        protected override void Dispose(bool disposing)
        {
            using (_it)
            using (_ms)
                base.Dispose(disposing);
        }

        public override void Flush() => _ms.Flush();

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seeking is not supported.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Length cannot be set.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _ms.Read(buffer, offset, count);

            while (read < count && _it.MoveNext())
            {
                _ms.SetLength(0);

                var bytes = _it.Current as byte[] ?? _it.Current.ToArray();
                _ms.Write(bytes, 0, bytes.Length);
                _ms.Seek(-bytes.Length, SeekOrigin.Current);
                read += _ms.Read(buffer, read, count - read);
            }

            _pos += read;
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get { throw new NotSupportedException("Length is unknown."); }
        }

        public override long Position
        {
            get { return _pos; }
            set { throw new NotSupportedException("Position is read-only."); }
        }
    }
}
