using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public class CompositeDisposable : IDisposable
    {
        private bool _disposed;
        private readonly IDisposable[] _disposables;
        private readonly object _syncLock = new object();

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null) 
                throw new ArgumentNullException("disposables");

            _disposables = disposables.ToArray();
        }

        public void Dispose()
        {
            if (!_disposed)
                return;

            lock (_syncLock)
            {
                if (!_disposed)
                    return;

                _disposed = true;
            }

            _disposables.Dispose();
        }
    }
}