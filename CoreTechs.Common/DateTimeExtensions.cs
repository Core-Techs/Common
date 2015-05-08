using System;
using System.Threading;

namespace CoreTechs.Common
{
    public enum DateTimePrecision
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    public static class DateTimeExtensions
    {
        public static DateTime Truncate(this DateTime dt, DateTimePrecision precision = DateTimePrecision.Millisecond)
        {
            switch (precision)
            {
                case DateTimePrecision.Millisecond:
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond,
                        dt.Kind);
                case DateTimePrecision.Second:
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
                case DateTimePrecision.Minute:
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
                case DateTimePrecision.Hour:
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind);
                case DateTimePrecision.Day:
                    return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
                case DateTimePrecision.Month:
                    return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Kind);
                case DateTimePrecision.Year:
                    return new DateTime(dt.Year, 1, 1, 0, 0, 0, 0, dt.Kind);
                default:
                    throw new ArgumentOutOfRangeException("precision");
            }
        }

        public static DateTimeOffset Truncate(this DateTimeOffset dt, DateTimePrecision precision = DateTimePrecision.Millisecond)
        {
            switch (precision)
            {
                case DateTimePrecision.Millisecond:
                    return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond,
                        dt.Offset);
                case DateTimePrecision.Second:
                    return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Offset);
                case DateTimePrecision.Minute:
                    return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Offset);
                case DateTimePrecision.Hour:
                    return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Offset);
                case DateTimePrecision.Day:
                    return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Offset);
                case DateTimePrecision.Month:
                    return new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Offset);
                case DateTimePrecision.Year:
                    return new DateTimeOffset(dt.Year, 1, 1, 0, 0, 0, 0, dt.Offset);
                default:
                    throw new ArgumentOutOfRangeException("precision");
            }
        }

        public static double TotalYears(this TimeSpan timeSpan)
        {
            return timeSpan.TotalDays / Constants.DaysPerYear;
        }

        public static double TotalMonths(this TimeSpan timeSpan)
        {
            return timeSpan.TotalYears() * 12;
        }

        public static void Sleep(this TimeSpan timeSpan)
        {
            Thread.Sleep(timeSpan);
        }

        public static TimeSpan Multiply(this TimeSpan timeSpan, int n)
        {
            return TimeSpan.FromTicks(timeSpan.Ticks*n);
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, double n)
        {
            return TimeSpan.FromTicks((long) (timeSpan.Ticks/n));
        }

        public static DateTime SpecifyKind(this DateTime dt, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(dt, kind);
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dt)
        {
            return new DateTimeOffset(dt);
        }

        public static DateTime SetYear(this DateTime dt, int year)
        {
            return new DateTime(year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        public static DateTime SetMonth(this DateTime dt, int month)
        {
            return new DateTime(dt.Year, month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        public static DateTime SetDay(this DateTime dt, int day)
        {
            return new DateTime(dt.Year, dt.Month, day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        public static DateTime SetHour(this DateTime dt, int hour)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, hour, dt.Minute, dt.Second, dt.Millisecond);
        }

        public static DateTime SetMinute(this DateTime dt, int minute)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minute, dt.Second, dt.Millisecond);
        }

        public static DateTime SetSecond(this DateTime dt, int second)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second, dt.Millisecond);
        }

        public static DateTime SetMillisecond(this DateTime dt, int millisecond)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, millisecond);
        }

        public static DateTimeOffset SetYear(this DateTimeOffset dt, int year)
        {
            return new DateTimeOffset(year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond,dt.Offset);
        }

        public static DateTimeOffset SetMonth(this DateTimeOffset dt, int month)
        {
            return new DateTimeOffset(dt.Year, month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Offset);
        }

        public static DateTimeOffset SetDay(this DateTimeOffset dt, int day)
        {
            return new DateTimeOffset(dt.Year, dt.Month, day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Offset);
        }

        public static DateTimeOffset SetHour(this DateTimeOffset dt, int hour)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, hour, dt.Minute, dt.Second, dt.Millisecond, dt.Offset);
        }

        public static DateTimeOffset SetMinute(this DateTimeOffset dt, int minute)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, minute, dt.Second, dt.Millisecond, dt.Offset);
        }

        public static DateTimeOffset SetSecond(this DateTimeOffset dt, int second)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second, dt.Millisecond, dt.Offset);
        }

        public static DateTimeOffset SetMillisecond(this DateTimeOffset dt, int millisecond)
        {
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, millisecond, dt.Offset);
        }

        public static TimeSpan Abs(this TimeSpan timeSpan)
        {
            return timeSpan < TimeSpan.Zero ? timeSpan.Negate() : timeSpan;
        }
    }
}