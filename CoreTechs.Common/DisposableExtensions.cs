using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public static class DisposableExtensions
    {
        /// <summary>
        /// Disposes each IDisposable in enumerated order.
        /// Disposal exceptions will not prevent disposal of other elements.
        /// The entire enumerable will be traversed before the first disposal.
        /// </summary>
        public static void Dispose(this IEnumerable<IDisposable> disposables)
        {
            if (disposables == null)
                throw new ArgumentNullException("disposables");

            var dispose = disposables
                .Where(x => x != null)
                .Aggregate<IDisposable, Action>(
                    () => { },
                    (a, d) => () =>
                    {
                        using (d) a();
                    });

            dispose();
        }
    }
}