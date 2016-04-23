using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
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
        private readonly BlockingCollection<TaskCompletionSource<object>> _msgs;
        private readonly Thread _loopThread;
        private readonly TState _state;
        private readonly Func<TState, MessageContext, Task<object>> _interceptor;
        private readonly bool _disposeState;

        public MessageLoop(Func<TState> stateFactory, bool disposeState = true, int? capacity = null, Func<TState, MessageContext, Task<object>> interceptor = null)
        {
            if (stateFactory == null) throw new ArgumentNullException(nameof(stateFactory));

            _interceptor = interceptor;

            _msgs = capacity.HasValue
                ? new BlockingCollection<TaskCompletionSource<object>>(capacity.Value)
                : new BlockingCollection<TaskCompletionSource<object>>();

            using (var loopReady = new ManualResetEventSlim())
            {
                TState state = default(TState);
                ExceptionDispatchInfo ctorEx = null;

                _loopThread = new Thread(async () =>
                {
                    try
                    {
                         state = stateFactory();
                    }
                    catch (Exception ex)
                    {
                        ctorEx = ExceptionDispatchInfo.Capture(ex);                        
                    }
                    finally
                    {
                        loopReady.Set();
                    }

                    foreach (var msg in _msgs.GetConsumingEnumerable())
                        await ProcessMessageAsync(msg);
                });

                _loopThread.Start();
                loopReady.Wait();
                ctorEx?.Throw();
                _state = state;
            }

            _disposeState = disposeState;
        }
        

        private async Task ProcessMessageAsync(TaskCompletionSource<object> msg)
        {
            try
            {
                var msgCtx = (MessageContext)msg.Task.AsyncState;
                var task = _interceptor == null
                    ? msgCtx.Func(_state)
                    : _interceptor(_state, msgCtx);

                var result = await task;
                msg.SetResult(result);
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

        public void Dispose()
        {
            var state = _disposeState ? _state as IDisposable : null;

            using(state)            
            using (_msgs)
            {
                _msgs.CompleteAdding();
                _loopThread.Join();
            }
        }

        public async Task<TResult> GetAsync<TResult>(Func<TState, Task<TResult>> resultFunc)
        {
            if (resultFunc == null) throw new ArgumentNullException(nameof(resultFunc));            

            Func<TState, Task<object>> func = async state => await resultFunc(state);
            TaskCompletionSource<object> msg;

            if (Thread.CurrentThread == _loopThread)
            {
                // already in message loop. inline execution.
                msg = new TaskCompletionSource<object>(new MessageContext(func, true));
                ProcessMessageAsync(msg);
            }
            else
            {
                msg = new TaskCompletionSource<object>(new MessageContext(func, false));
                _msgs.Add(msg);
            }

            var result = await msg.Task;
            return (TResult)result;
        }

        public Task<TResult> GetAsync<TResult>(Func<TState, TResult> resultFunc) =>
            GetAsync(state => Task.FromResult(resultFunc(state)));

        public async Task DoAsync(Func<TState, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            await GetAsync(async state =>
            {
                await action(state);
                return 0;
            });
        }
        public Task DoAsync(Action<TState> action) =>
            GetAsync(state =>
            {
                action(state);
                return Task.FromResult(0);
            });

        public class MessageContext
        {
            internal MessageContext(Func<TState, Task<object>> func, bool nestedMessage)
            {
                Func = func;
                NestedMessage = nestedMessage;
            }


            public Func<TState, Task<object>> Func { get; }

            /// <summary>
            /// True when the message is being sent from the thread that is processing messages.
            /// </summary>
            public bool NestedMessage { get; }

        }
    }
}
