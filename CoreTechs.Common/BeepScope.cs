using System;

namespace CoreTechs.Common
{
    /// <summary>
    /// Random beeps on a background task from the time of construction until disposal.
    /// Use this to add that that 1960's space ship computer feeling to your software.
    /// </summary>
   public class BeepScope : RepeatScope
    {
        private readonly TimeSpan _beepDuration;
        private readonly int _minFreq;
        private readonly int _maxFreq;

        public BeepScope(int minFreq = 200, int maxFreq = 3000, TimeSpan? beepDuration = null)
        {
            _minFreq = minFreq;
            _maxFreq = maxFreq;
            _beepDuration = beepDuration ?? TimeSpan.FromMilliseconds(100);
        }

        protected override void Execute()
        {
            var frequency = RNG.Next(_minFreq, _maxFreq);
            var totalMilliseconds = (int) _beepDuration.TotalMilliseconds;
            Console.Beep(frequency, totalMilliseconds);
        }
    }
}
