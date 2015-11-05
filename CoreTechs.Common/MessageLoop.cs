using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    /// <summary>
    /// A task-based message loop that allows for interacting with generic state.
    /// This is useful when you need to interact with an object from multiple threads,
    /// but that object is not thread safe or it requires thread affinity.
    /// 
    /// Synchronization is achieved by sequencing interactions through the
    /// message loop, rather than using locks.
    /// </summary>
    public class MessageLoop<TState> : IDisposable
    {
        private readonly Task _loopTask;
        private readonly BlockingCollection<TaskCompletionSource<object>> _msgs;
        private readonly Thread _loopThread;
        private readonly TState _state;
        private readonly IDisposable _disposableState;
        private readonly Func<TState, Func<TState, Task<object>>, Task<object>> _interceptor;


        public MessageLoop(Func<TState> stateFactory, bool disposeState = true, int? capacity = null, Func<TState, Func<TState, Task<object>>, Task<object>> interceptor = null)
        {
            if (stateFactory == null) throw new ArgumentNullException(nameof(stateFactory));

            _interceptor = interceptor;

            _msgs = capacity.HasValue
                ? new BlockingCollection<TaskCompletionSource<object>>(capacity.Value)
                : new BlockingCollection<TaskCompletionSource<object>>();

            var readyExitCtor = new TaskCompletionSource<Tuple<TState, Thread>>();

            _loopTask = Task.Run(() =>
            {
                try
                {
                    var state = stateFactory();
                    readyExitCtor.SetResult(Tuple.Create(state, Thread.CurrentThread));
                }
                catch (Exception ex)
                {
                    readyExitCtor.SetException(ex);
                    return;
                }

                foreach (var msg in _msgs.GetConsumingEnumerable())
                {
                    try
                    {
                        var func = (Func<TState, Task<object>>)msg.Task.AsyncState;

                        var task = _interceptor == null
                            ? func(_state)
                            : _interceptor(_state, func);

                        msg.SetResult(task.Result);
                    }
                    catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
                    {
                        msg.SetException(ex.InnerException);
                    }
                    catch (Exception ex)
                    {
                        msg.SetException(ex);
                    }
                }

            });

            try
            {
                var result = readyExitCtor.Task.Result;
                _state = result.Item1;
                _loopThread = result.Item2;
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerException;
            }

            if (disposeState)
                _disposableState = _state as IDisposable;
        }

        public void Dispose()
        {
            using (_loopTask)
            using (_msgs)
            using (_disposableState)
            {
                _msgs.CompleteAdding();
                _loopTask.Wait();
            }
        }

        public async Task<TResult> GetAsync<TResult>(Func<TState, Task<TResult>> resultFunc)
        {
            if (resultFunc == null) throw new ArgumentNullException(nameof(resultFunc));

            if (Thread.CurrentThread == _loopThread)
            {
                // already in message loop. inline execution.
                return await resultFunc(_state).ConfigureAwait(false);
            }

            Func<TState, Task<object>> func = async state => await resultFunc(state).ConfigureAwait(false);
            var msg = new TaskCompletionSource<object>(func);
            _msgs.Add(msg);
            var result = await msg.Task.ConfigureAwait(false);
            return (TResult)result;
        }

        public TResult Get<TResult>(Func<TState, TResult> resultFunc)
        {
            try
            {
                return GetAsync(state => Task.FromResult(resultFunc(state))).Result;
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerException;
            }
        }


        public async Task DoAsync(Func<TState, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            await GetAsync(async state =>
            {
                await action(state);
                return 0;
            });
        }

        public void Do(Action<TState> action)
        {
            try
            {
                GetAsync(state =>
                {
                    action(state);
                    return Task.FromResult(0);
                }).Wait();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerException;
            }
        }
    }
}
