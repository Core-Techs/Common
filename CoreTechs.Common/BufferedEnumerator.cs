using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreTechs.Common
{
    /// <summary>
    /// Wraps enumerators so that you can traverse in reverse :)
    /// </summary>
    public class BufferedEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> _enumerator;
        readonly LinkedList<T> _buffer = new LinkedList<T>();
        private LinkedListNode<T> _current;
        readonly bool _disposeSource;
        private bool _disposed;
        private readonly int? _cap;

        public BufferedEnumerator(IEnumerator<T> enumerator, int? capacity = null, bool disposeSourceEnumerator = true)
        {
            if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));

            if (capacity.HasValue && capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _enumerator = enumerator;
            _disposeSource = disposeSourceEnumerator;
            _cap = capacity;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _buffer.Clear();
                _current = null;
            }
            finally
            {
                if (_disposeSource)
                    _enumerator.Dispose();

                _enumerator = null;
                _disposed = true;
            }
        }

        public bool MoveNext()
        {
            if (_current == null || _current == _buffer.Last)
            {
                var moved = _enumerator.MoveNext();
                if (moved)
                    _current = _buffer.AddLast(_enumerator.Current);

                EnsureCapacity();

                return moved;
            }

            _current = _current.Next;
            return true;
        }

        public bool MovePrevious()
        {
            if (_current == null || _current == _buffer.First)
                return false;

            _current = _current.Previous;
            return true;
        }

        public void Reset()
        {
            _enumerator.Reset();
            _buffer.Clear();
            _current = null;
        }

        public T Current => _current == null ? default(T) : _current.Value;

        public void ClearBuffer()
        {
            var currentIsLast = _current == _buffer.Last;

            _buffer.Clear();
            _current = null;

            if (currentIsLast)
                _current = _buffer.AddFirst(_enumerator.Current);
        }

        public void ClearBufferBeforeCurrent()
        {
            ClearBufferBeforeNode(_current);
        }

        private void ClearBufferBeforeNode(LinkedListNode<T> node)
        {
            if (node == null)
                return;

            while (node != _buffer.First)
                _buffer.RemoveFirst();
        }

        private void EnsureCapacity()
        {
            while (_buffer.Count > _cap + 1) // +1 because current doesn't count
                _buffer.RemoveFirst();
        }

        object IEnumerator.Current => Current;
    }
}