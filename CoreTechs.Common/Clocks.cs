using System;
using System.Threading;

namespace CoreTechs.Common
{
    /// <summary>
    /// Simple abstraction over the system clock.
    /// </summary>
    public interface IClock
    {
        DateTimeOffset Now { get; }
    }

    public class TestClock : IClock
    {
        private DateTimeOffset _now;

        /// <summary>
        /// Provides the Now property's value. The previous value is passed to the function.
        /// </summary>
        public Func<DateTimeOffset, DateTimeOffset> NextNowFunction { get; set; }

        public TestClock(DateTimeOffset initialInstant, Func<DateTimeOffset,DateTimeOffset> nextNowFunction)
        {
            if (nextNowFunction == null) throw new ArgumentNullException("nextNowFunction");
            
            _now = initialInstant;
            NextNowFunction = prev =>
            {
                NextNowFunction = nextNowFunction;
                return initialInstant;
            };
        }

        public TestClock(DateTimeOffset initialInstant, TimeSpan increment)
            : this(initialInstant, prev => prev + increment)
        {
        }

        public DateTimeOffset Now
        {
            get { return _now = NextNowFunction(_now); }
        }
    }

    public sealed class SystemClock : IClock
    {
        public DateTimeOffset Now { get { return DateTimeOffset.Now; } }

        private SystemClock() { }

        private static readonly SystemClock TheInstance = new SystemClock();
        public static SystemClock Instance
        {
            get
            {
                return TheInstance;
            }
        }
    }
}
