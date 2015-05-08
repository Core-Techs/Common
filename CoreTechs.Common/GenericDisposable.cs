using System;

namespace CoreTechs.Common
{
    public class GenericDisposable<T> : IDisposable
    {
        private readonly object _mutex = new object();
        private readonly Action<T> _onDispose;
        public T Value { get; private set; }
        public bool Disposed { get; private set; }

        public GenericDisposable(T value, Action<T> onDispose)
        {
            Value = value;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            lock (_mutex)
            {
                if (Disposed)
                    return;

                if (_onDispose != null)
                    _onDispose(Value);

                Disposed = true;
            }
        }
    }
}