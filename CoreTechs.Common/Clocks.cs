using System;

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
        private readonly object _syncLock = new object();

        public TestClock(DateTimeOffset initialInstant, Func<DateTimeOffset, DateTimeOffset> nextNowFunction)
        {
            if (nextNowFunction == null) throw new ArgumentNullException(nameof(nextNowFunction));

            _prevInstant = initialInstant;
            _nextNowFunction = prev =>
            {
                _nextNowFunction = nextNowFunction;
                return initialInstant;
            };
        }

        public TestClock(DateTimeOffset initialInstant, TimeSpan increment)
            : this(initialInstant, prev => prev + increment)
        {
        }

        public DateTimeOffset Now
        {
            get
            {
                lock (_syncLock)
                {
                    var now = _prevInstant = _nextNowFunction(_prevInstant);
                    return now;
                }
            }
        }
    }


    /// <summary>
    /// Clock that returns DateTimeOffset.Now.
    /// </summary>
    public class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;

        public static SystemClock Instance { get; } = new SystemClock();
    }
}
