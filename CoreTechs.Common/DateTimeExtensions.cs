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
    }
}