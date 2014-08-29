using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace CoreTechs.Common
{
    /// <summary>
    /// Use this instead of System.Random.
    /// </summary>
    public static class RNG
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> LocalRandom =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        private static readonly ThreadLocal<RNGCryptoServiceProvider> LocalCrypto =
            new ThreadLocal<RNGCryptoServiceProvider>(() => new RNGCryptoServiceProvider());

        public static Random Instance
        {
            get { return LocalRandom.Value; }
        }

        public static RNGCryptoServiceProvider Crypto
        {
            get { return LocalCrypto.Value; }
        }

        public static byte[] GetBytes(this RandomNumberGenerator rng,  int count)
        {
            var bytes = new byte[count];
            rng.GetBytes(bytes);
            return bytes;
        }

        public static byte[] GetNonZeroBytes(this RandomNumberGenerator rng, int count)
        {
            var bytes = new byte[count];
            rng.GetNonZeroBytes(bytes);
            return bytes;
        }

        public static int Next()
        {
            return Instance.Next();
        }

        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            Instance.NextBytes(buffer);
        }

        public static byte[] NextBytes(long count)
        {
            return Instance.NextBytes(count);
        }

        public static double NextDouble()
        {
            return Instance.NextDouble();
        }

        public static bool NextBool()
        {
            return Instance.NextBool();
        }

        public static bool NextBool(double probability)
        {
            return Instance.NextBool(probability);
        }

        public static TimeSpan NextTimeSpan()
        {
            return Instance.NextTimeSpan();
        }

        public static TimeSpan NextTimeSpan(TimeSpan maxValue)
        {
            return Instance.NextTimeSpan(maxValue);
        }

        public static TimeSpan NextTimeSpan(TimeSpan minValue, TimeSpan maxValue)
        {
            return Instance.NextTimeSpan(minValue, maxValue);
        }

        public static DateTime NextDateTime(DateTime maxValue)
        {
            return Instance.NextDateTime(maxValue);
        }

        public static DateTime NextDateTime(DateTime minValue, DateTime maxValue)
        {
            return Instance.NextDateTime(minValue, maxValue);
        }

        public static DateTime NextDateTime()
        {
            return Instance.NextDateTime();
        }

        public static DateTimeOffset NextDateTimeOffset(DateTimeOffset maxValue)
        {
            return Instance.NextDateTimeOffset(maxValue);
        }

        public static DateTimeOffset NextDateTimeOffset(DateTimeOffset minValue, DateTimeOffset maxValue)
        {
            return Instance.NextDateTimeOffset(minValue, maxValue);
        }

        public static DateTimeOffset NextDateTimeOffset()
        {
            return Instance.NextDateTimeOffset();
        }

        public static long NextInt64(long maxValue)
        {
            return Instance.NextInt64(maxValue);
        }

        public static long NextInt64(long minValue, long maxValue)
        {
            return Instance.NextInt64(minValue, maxValue);
        }

        public static long NextInt64()
        {
            return Instance.NextInt64();
        }

        public static object Next(Type type)
        {
            return Instance.Next(type);
        }

        public static T Next<T>()
        {
            return Instance.Next<T>();
        }
    }
}