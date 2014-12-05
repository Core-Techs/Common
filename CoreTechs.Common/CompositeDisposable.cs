using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    /// <summary>
    /// A container for other IDisposable objects that can be disposed all at once.
    /// Disposal occurs in the order that items are enumerated.
    /// Disposal exceptions do not prevent disposal of other elements.
    /// </summary>
    public class CompositeDisposable : IDisposable
    {
        private readonly object _syncLock = new object();
        private bool _disposed;
        private IDisposable[] _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null) 
                throw new ArgumentNullException("disposables");

            _disposables = disposables.ToArray();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_syncLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            try
            {
                _disposables.DisposeAllTheThings();
            }
            finally
            {
                _disposables = null;
            }
        }
    }
}