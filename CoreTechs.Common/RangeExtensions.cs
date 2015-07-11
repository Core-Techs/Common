using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public static class RangeExtensions
    {

        public static IEnumerable<byte> To(this byte i, byte to, byte step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToByte);
        }

        public static IEnumerable<sbyte> To(this sbyte i, sbyte to, sbyte step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToSByte);
        }

        public static IEnumerable<ushort> To(this ushort i, ushort to, ushort step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToUInt16);
        }

        public static IEnumerable<short> To(this short i, short to, short step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToInt16);
        }

        public static IEnumerable<uint> To(this uint i, uint to, uint step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToUInt32);
        }

        public static IEnumerable<int> To(this int i, int to, int step = 1)
        {
            return To((long)i, to, step).Select(Convert.ToInt32);
        }

        public static IEnumerable<long> To(this long i, long to, long step = 1)
        {
            return To(i, to, (n, s) => (long)(n + s), step).Select(Convert.ToInt64);
        }

        public static IEnumerable<ulong> To(this ulong i, ulong to, ulong step = 1)
        {
            return To(i, to, (n, s) => (ulong)(n + s), step).Select(Convert.ToUInt64);
        }

        public static IEnumerable<float> To(this float i, float to, float step = 1)
        {
            return To((double)i, to, step).Select(Convert.ToSingle);
        }

        public static IEnumerable<double> To(this double i, double to, double step = 1)
        {
            return To(i, to, (n, s) => (double)((decimal)n + s), (decimal)step).Select(Convert.ToDouble);
        }

        public static IEnumerable<decimal> To(this decimal i, decimal to, decimal step = 1)
        {
            return To(i, to, (n, s) => n + s, step);
        }

        public static IEnumerable<DateTime> To(this DateTime i, DateTime to, TimeSpan step)
        {
            return i.To(_ => to, step);
        }

        public static IEnumerable<DateTimeOffset> To(this DateTimeOffset i, DateTimeOffset to, TimeSpan step)
        {
            return i.To(_ => to, step);
        }

        public static IEnumerable<TimeSpan> To(this TimeSpan i, TimeSpan to, TimeSpan step)
        {
            return i.To(_ => to, step);
        }

        public static IEnumerable<DateTime> To(this DateTime i, Func<DateTime,DateTime> toFunc, TimeSpan step)
        {
            return i.To(toFunc, (d, s) => d.Add(TimeSpan.FromTicks((long) s)), step.Ticks);
        }

        public static IEnumerable<DateTimeOffset> To(this DateTimeOffset i, Func<DateTimeOffset,DateTimeOffset> toFunc, TimeSpan step)
        {
            return i.To(toFunc, (d, s) => d.Add(TimeSpan.FromTicks((long) s)), step.Ticks);
        }

        public static IEnumerable<TimeSpan> To(this TimeSpan i, Func<TimeSpan, TimeSpan> toFunc, TimeSpan step)
        {
            return i.To(toFunc, (t, s) => t + TimeSpan.FromTicks((long)s), step.Ticks);
        }

        public static IEnumerable<char> To(this char i, char to, int step = 1)
        {
            return i.To(to, (c, s) => (char) (c + s), step);
        }

        public static IEnumerable<T> To<T>(this T i, Func<T, T> toFunc, Func<T, decimal, T> stepper, decimal step = 1)
            where T : IComparable
        {
            var to = toFunc(i);
            return To(i, to, stepper, step);
        }

        public static IEnumerable<T> To<T>(this T i, T to, Func<T, decimal, T> stepper, decimal step = 1) where T : IComparable
        {
            var c1 = Compare(i, to);
            var c2 = c1;

            step = c1 == -1 ? Math.Abs(step) :
                   c1 == 1 ? -Math.Abs(step) :
                   step;

            while (c1 == c2 || c2 == 0)
            {
                yield return i;
                i = stepper(i, step);
                c2 = Compare(i, to);
            }
        }

        /// <summary>
        /// This function only returns -1, 0, or 1.
        /// Some implementations of IComparable (<see cref="char"/>)
        /// return the actual difference between the 2 objects.
        /// That behavior broke the range producing algorithm.
        /// </summary>
        private static int Compare<T>(T a, T b) where T : IComparable
        {
            var result = a.CompareTo(b);
            if (result < 0) return -1;
            if (result > 0) return 1;
            return 0;
        }
    }
}
