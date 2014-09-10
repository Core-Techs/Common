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
        private bool _disposed;
        private readonly List<IDisposable> _disposables;
        private readonly object _syncLock = new object();

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null) 
                throw new ArgumentNullException("disposables");

            _disposables = disposables.ToList();
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
                _disposables.Dispose();
            }
            finally
            {
                _disposables.Clear();
            }
            
        }
    }
}