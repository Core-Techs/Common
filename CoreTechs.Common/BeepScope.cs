using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    /// <summary>
    /// Random beeps on a background task from the time of construction until disposal.
    /// Use this to add that that 1960's space ship computer feeling to your software.
    /// </summary>
    public class BeepScope : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _task;

        public BeepScope() : this(200, 3000, TimeSpan.FromMilliseconds(100)) { }
        public BeepScope(int minFreq, int maxFreq, TimeSpan beepDuration)
        {
            var token = _cts.Token;
            _task = Task.Run(() =>
            {

                var rng = new Random();
                while (!token.IsCancellationRequested)
                    Console.Beep(rng.Next(minFreq, maxFreq), (int)beepDuration.TotalMilliseconds);

            }, token);
        }

        public void Dispose()
        {
            using(_cts)
            using (_task)
            {
                _cts.Cancel();
                _task.Wait();
            }
        }
    }
}
