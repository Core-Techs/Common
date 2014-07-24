using System;
using System.Threading;

namespace CoreTechs.Common
{
    public class Sequence
    {
        long _i;

        public Sequence(long init = 0)
        {
            _i = init;
        }

        public long Next()
        {
            return Interlocked.Increment(ref _i);
        }

        public static readonly Sequence Instance =
            new Sequence(DateTime.Now.Ticks);
    }
}