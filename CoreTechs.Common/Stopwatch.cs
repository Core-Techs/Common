using System;

namespace CoreTechs.Common
{
    /// <summary>
    /// Much like System.Diagnostics.Stopwatch, but you can set the started and stopped times directly.
    /// </summary>
    public class Stopwatch
    {
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Stopped { get; set; }

        public static Stopwatch StartNew()
        {
            var sw = new Stopwatch();
            sw.Start();
            return sw;
        }

        public bool IsRunning
        {
            get
            {
                return Started.HasValue && !Stopped.HasValue;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                if (Started == null)
                    return TimeSpan.Zero;

                if (Stopped == null)
                    return DateTimeOffset.Now - Started.Value;

                return Stopped.Value - Started.Value;
            }
        }

        public void Start()
        {
            if (IsRunning) return;
            Started = DateTimeOffset.Now;
            Stopped = null;
        }

        public void Stop()
        {
            if (!IsRunning) return;
            Stopped = DateTimeOffset.Now;
        }

        public void Reset()
        {
            Started = Stopped = null;
        }

        public void Restart()
        {
            Reset();
            Start();
        }
    }
}