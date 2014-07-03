using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Invokes cancellation on the token source and then waits on for tasks to complete,
        /// suppressing any <see cref="OperationCanceledException"/> that is thrown.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="tasks">The tasks to wait on.</param>
        /// <param name="millisecondsTimeout">The max duration to wait for tasks to complete.</param>
        /// <param name="waitCancellationToken">A cancellation token for cancelling the wait on tasks. Don't pass the cancellation token that is produced by your CancellationTokenSource.</param>
        public static void CancelAndWait(this CancellationTokenSource cancellationTokenSource, IEnumerable<Task> tasks, int millisecondsTimeout = -1, CancellationToken waitCancellationToken = default(CancellationToken))
        {
            cancellationTokenSource.Cancel();
            tasks.WaitAllHandlingCancellation(millisecondsTimeout, waitCancellationToken);
        }

        /// <summary>
        /// Invokes cancellation on the token source and then wait for the task to complete,
        /// suppressing any <see cref="OperationCanceledException"/> that is thrown.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="task">The task to wait on.</param>
        /// <param name="millisecondsTimeout">The max duration to wait for tasks to complete.</param>
        /// <param name="waitCancellationToken">A cancellation token for cancelling the wait on tasks. Don't pass the cancellation token that is produced by your CancellationTokenSource.</param>
        public static void CancelAndWait(this CancellationTokenSource cancellationTokenSource, Task task, int millisecondsTimeout = -1, CancellationToken waitCancellationToken = default(CancellationToken))
        {
            cancellationTokenSource.Cancel();
            new[] {task}.WaitAllHandlingCancellation(millisecondsTimeout, waitCancellationToken);
        }

        public static void WaitHandlingCancellation(this Task task, int millisecondsTimeout = -1,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            WaitAllHandlingCancellation(new[] {task}, millisecondsTimeout, cancellationToken);
        }

        public static void WaitAllHandlingCancellation(this IEnumerable<Task> tasks, int millisecondsTimeout = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Task.WaitAll(tasks as Task[] ?? tasks.ToArray(), millisecondsTimeout, cancellationToken);
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle(x => x is OperationCanceledException);
            }
        }

    }
}