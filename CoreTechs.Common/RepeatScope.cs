using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    public abstract class RepeatScope : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _task;

        protected RepeatScope()
        {
            var token = _cts.Token;
            _task = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                    Execute();

            }, token);
        }

        protected abstract void Execute();
        protected virtual IEnumerable<IDisposable> GetDisposables()
        {
            yield break;
        }

        public void Dispose()
        {
            using (_cts)
            using (_task)
            {
                _cts.CancelAndWait(_task);

                var attempts = GetDisposables().Select(d => Attempt.Do(d.Dispose)).ToArray();
                if (attempts.Any(a => !a.Succeeded))
                    throw new AggregateException(attempts.Select(a => a.Exception));
            }
        }

    }

  
}