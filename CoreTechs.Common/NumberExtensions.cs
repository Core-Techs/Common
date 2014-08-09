using System;

namespace CoreTechs.Common
{
    public static class NumberExtensions
    {
        public static TimeSpan Seconds(this double n)
        {
            return TimeSpan.FromSeconds(n);
        }

        public static TimeSpan Seconds(this int n)
        {
            return TimeSpan.FromSeconds(n);
        }

        public static TimeSpan Seconds(this long n)
        {
            return TimeSpan.FromSeconds(n);
        }

        public static TimeSpan Minutes(this double n)
        {
            return TimeSpan.FromMinutes(n);
        }

        public static TimeSpan Minutes(this int n)
        {
            return TimeSpan.FromMinutes(n);
        }

        public static TimeSpan Minutes(this long n)
        {
            return TimeSpan.FromMinutes(n);
        }

        public static TimeSpan Hours(this double n)
        {
            return TimeSpan.FromHours(n);
        }

        public static TimeSpan Hours(this int n)
        {
            return TimeSpan.FromHours(n);
        }

        public static TimeSpan Hours(this long n)
        {
            return TimeSpan.FromHours(n);
        }

        public static TimeSpan Weeks(this double n)
        {
            return TimeSpan.FromDays(n*7);
        }

        public static TimeSpan Weeks(this int n)
        {
            return TimeSpan.FromDays(n * 7);
        }

        public static TimeSpan Weeks(this long n)
        {
            return TimeSpan.FromDays(n * 7);
        }

        public static TimeSpan Days(this double n)
        {
            return TimeSpan.FromDays(n);
        }

        public static TimeSpan Days(this int n)
        {
            return TimeSpan.FromDays(n);
        }

        public static TimeSpan Days(this long n)
        {
            return TimeSpan.FromDays(n);
        }

        public static TimeSpan Milliseconds(this double n)
        {
            return TimeSpan.FromMilliseconds(n);
        }

        public static TimeSpan Milliseconds(this int n)
        {
            return TimeSpan.FromMilliseconds(n);
        }

        public static TimeSpan Milliseconds(this long n)
        {
            return TimeSpan.FromMilliseconds(n);
        }

        public static TimeSpan Ticks(this int n)
        {
            return TimeSpan.FromTicks(n);
        }

        public static TimeSpan Ticks(this long n)
        {
            return TimeSpan.FromTicks(n);
        }
    }
}
