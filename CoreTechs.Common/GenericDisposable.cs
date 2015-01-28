using System;

namespace CoreTechs.Common
{
    public class GenericDisposable<T> : IDisposable
    {
        private readonly object _mutex = new object();
        private readonly Action<T> _onDispose;
        private T _obj;
        public bool Disposed { get; private set; }

        public GenericDisposable(T obj, Action<T> onDispose)
        {
            _obj = obj;
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

                try
                {
                    if (_onDispose != null)
                        _onDispose(_obj);

                    Disposed = true;
                }
                finally
                {
                    _obj = default(T);
                } 
            }
        }
    }
}