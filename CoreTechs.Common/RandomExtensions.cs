using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CoreTechs.Common
{
    /// <remarks>
    /// Some of the methods are naive implementations and may have subtle
    /// issues like modulo bias and nonuniform distribution.
    /// That's okay for the most part, but be aware that it's possible.
    /// Feel free to improve them.
    /// </remarks>
    public static class RandomExtensions
    {

        public static bool NextBool(this Random r)
        {
            return r.Next(2) == 0;
        }

        public static bool NextBool(this Random r, double probability)
        {
            return r.NextDouble() < probability;
        }

        public static TimeSpan NextTimeSpan(this Random r)
        {
            return new TimeSpan(r.NextInt64());
        }

        public static TimeSpan NextTimeSpan(this Random r, TimeSpan maxValue)
        {
            return r.NextTimeSpan(TimeSpan.MinValue, maxValue);
        }

        public static TimeSpan NextTimeSpan(this Random r, TimeSpan minValue, TimeSpan maxValue)
        {
            return new TimeSpan(r.NextInt64(minValue.Ticks, maxValue.Ticks));
        }

        public static DateTime NextDateTime(this Random r, DateTime maxValue)
        {
            return r.NextDateTime(DateTime.MinValue, maxValue);
        }

        public static DateTime NextDateTime(this Random r, DateTime minValue, DateTime maxValue)
        {
            return new DateTime(r.NextInt64(minValue.Ticks, maxValue.Ticks), maxValue.Kind);
        }

        public static DateTime NextDateTime(this Random r)
        {
            var min = DateTime.MinValue.Ticks;
            var max = DateTime.MaxValue.Ticks + 1;
            return new DateTime(r.NextInt64(min, max));
        }

        public static DateTimeOffset NextDateTimeOffset(this Random rng)
        {
            return rng.NextDateTimeOffset(DateTimeOffset.MaxValue);
        }

        public static DateTimeOffset NextDateTimeOffset(this Random rng, DateTimeOffset max)
        {
            return rng.NextDateTimeOffset(DateTimeOffset.MinValue, max);
        }

        public static DateTimeOffset NextDateTimeOffset(this Random rng, DateTimeOffset min, DateTimeOffset max)
        {
            return new DateTimeOffset(rng.NextInt64(min.Ticks, max.Ticks), max.Offset);
        }

        public static long NextInt64(this Random r, long maxValue)
        {
            return r.NextInt64(0, maxValue);
        }

        public static long NextInt64(this Random r)
        {
            return r.NextInt64(0, Int64.MaxValue);
        }

        /// <summary>
        /// Returns a random long from min (inclusive) to max (exclusive)
        /// </summary>
        /// <param name="random">The given random instance</param>
        /// <param name="minValue">The inclusive minimum bound</param>
        /// <param name="maxValue">The exclusive maximum bound.  Must be greater than min</param>
        /// <thanks>http://stackoverflow.com/a/13095144/64334</thanks>
        public static long NextInt64(this Random random, long minValue, long maxValue)
        {
            if (maxValue <= minValue)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            var uRange = (ulong)(maxValue - minValue);

            //Prevent a modolo bias; see http://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                var buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > UInt64.MaxValue - ((UInt64.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + minValue;
        }

        public static T NextElement<T>(this Random rng, IEnumerable<T> enumerable)
        {
            return enumerable.RandomElement(rng);
        }

        public static T NextElement<T>(this Random rng, IList<T> list)
        {
            return list.RandomElement(rng);
        }

        /// <summary>
        /// Returns an Int32 with a random value across the entire range of
        /// possible values.
        /// </summary>
        /// <thanks to="Jon Skeet">http://stackoverflow.com/a/609529/64334</thanks>
        public static int NextInt32(this Random rng)
        {
            unchecked
            {
                var firstBits = rng.Next(0, 1 << 4) << 28;
                var lastBits = rng.Next(0, 1 << 28);
                return firstBits | lastBits;
            }
        }

        /// <thanks to="Jon Skeet">http://stackoverflow.com/a/609529/64334</thanks>
        public static decimal NextDecimal(this Random rng, bool? isNegative = null, byte? scale = null)
        {
            if (scale < 0 || scale > 28)
                throw new ArgumentOutOfRangeException("scale", "0 >= scale <= 28");

            scale = scale ?? (byte) rng.Next(29);
            var sign = isNegative ?? rng.NextBool();
            return new decimal(rng.NextInt32(),
                               rng.NextInt32(),
                               rng.NextInt32(),
                               sign,
                               scale.Value);
        }

        public static byte[] NextBytes(this Random rng,long count)
        {
            var buffer = new byte[count];
            rng.NextBytes(buffer);
            return buffer;
        }


        public static object Next(this Random rng, Type type)
        {
            if (type == typeof(Int32))
                return RNG.Next();

            if (type == typeof(UInt32))
                return BitConverter.ToUInt32(RNG.NextBytes(sizeof(UInt32)), 0);

            if (type == typeof(Int64))
                return BitConverter.ToInt64(RNG.NextBytes(sizeof(Int64)), 0);

            if (type == typeof(UInt64))
                return BitConverter.ToUInt64(RNG.NextBytes(sizeof(UInt64)), 0);

            if (type == typeof(Int16))
                return BitConverter.ToInt16(RNG.NextBytes(sizeof(Int16)), 0);

            if (type == typeof(UInt16))
                return BitConverter.ToUInt16(RNG.NextBytes(sizeof(UInt16)), 0);

            if (type == typeof (Boolean))
                return rng.NextBool();

            if (typeof (IEnumerable<Byte>).IsAssignableFrom(type))
                return rng.NextBytes(4);

            if (type == typeof (Byte))
                return rng.NextBytes(1).Single();

            if (type == typeof (SByte))
                unchecked
                {
                    return (SByte)rng.NextBytes(1).Single();
                }

            if (type == typeof (Double))
                return rng.NextDouble();

            if (type == typeof(Single))
                return BitConverter.ToSingle(RNG.NextBytes(sizeof(Single)), 0);

            if (type == typeof (Decimal))
                return rng.NextDecimal();

            if (type == typeof (String))
                return rng.Next().ToString(CultureInfo.InvariantCulture);

            if (type == typeof (DateTime))
                return rng.NextDateTime();

            if (type == typeof (DateTimeOffset))
                return rng.NextDateTimeOffset();

            if (type == typeof (TimeSpan))
                return rng.NextTimeSpan();

            if (type == typeof (Char))
                return 0.Repeat().Select(_ => BitConverter.ToChar(rng.NextBytes(sizeof (Char)), 0))
                    .WhereNot(char.IsControl)
                    .WhereNot(char.IsSurrogate)
                    .First();

            if (type == typeof (Guid))
                return Guid.NewGuid();

            var types = new[]
            {
                typeof (Int32), typeof (UInt32), typeof (Int16), typeof (UInt16), typeof (Int64), typeof (UInt64),
                typeof (byte), typeof (sbyte), typeof (byte[]), typeof (Double), typeof (Single), typeof (Decimal),
                typeof (bool), typeof (Guid), typeof (String), typeof (Char), typeof (DateTime),
                typeof (DateTimeOffset), typeof (TimeSpan)
            };

            if (type == typeof (Type))
                return types.RandomElement(rng);

            if (type == typeof (Object))
                return rng.Next(types.RandomElement(rng));

            throw new NotSupportedException("type not supported: " + type.FullName);
        }

        public static T Next<T>(this Random rng)
        {
            return (T) rng.Next(typeof (T));
        }
    }

    
}