using System;
using System.Collections.Generic;

namespace CoreTechs.Common
{
    public static class DisposableExtensions
    {
        public static void Dispose(this IEnumerable<IDisposable> disposables)
        {
            if (disposables == null) 
                throw new ArgumentNullException("disposables");

            Action dispose = () => { };

            foreach (var disposable in disposables)
            {
                var disposeCopy = dispose;
                var disposableCopy = disposable;

                dispose = () =>
                {
                    using (disposableCopy) { disposeCopy(); }
                };
            }

            dispose();
        }
    }
}