using System;
using System.Collections.Generic;

namespace CoreTechs.Common
{
    public class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _hash;

        public DelegateEqualityComparer(Func<T, T, bool> @equals, Func<T, int> hash = null)
        {
            if (@equals == null) throw new ArgumentNullException("equals");
            _equals = @equals;
            _hash = hash ?? (_ => 0);
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hash(obj);
        }
    }
}