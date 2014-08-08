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

    /// <summary>
    /// Clock that allows controlling the next value returned by the Now property.
    /// </summary>
    public class TestClock : IClock
    {
        private DateTimeOffset _prevInstant;
        private Func<DateTimeOffset, DateTimeOffset> _nextNowFunction;

        /// <summary>
        /// Provides the Now property's value. The previous value is passed to the function.
        /// </summary>
        public Func<DateTimeOffset, DateTimeOffset> NextNowFunction
        {
            get { return _nextNowFunction; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _nextNowFunction = value;
            }
        }

        public TestClock(DateTimeOffset initialInstant, Func<DateTimeOffset,DateTimeOffset> nextNowFunction)
        {
            if (nextNowFunction == null) throw new ArgumentNullException("nextNowFunction");
            
            _prevInstant = initialInstant;
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
            get { return _prevInstant = NextNowFunction(_prevInstant); }
        }
    }


    /// <summary>
    /// Clock that returns DateTimeOffset.Now.
    /// </summary>
    public  class SystemClock : IClock
    {
        public DateTimeOffset Now { get { return DateTimeOffset.Now; } }

        private static readonly SystemClock Clock = new SystemClock();

        public static SystemClock Instance
        {
            get { return Clock; }
        }
    }
}
