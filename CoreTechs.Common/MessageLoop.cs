using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{

    /// <summary>
    /// A stateless, task-based message loop.
    /// Commonly useful within a class that you intend to be thread-safe,
    /// but don't want to use locks/mutexes.
    /// 
    /// Synchronization is achieved by sequencing interactions through the
    /// message loop, rather than using locks.
    /// </summary>
    public class MessageLoop : MessageLoop<object> { }

    /// <summary>
    /// A task-based message loop that allows for interacting with generic state.
    /// This is useful when you need to interact with an object from multiple threads,
    /// but that object is not thread safe or it requires thread affinity.
    /// 
    /// Synchronization is achieved by sequencing interactions through the
    /// message loop, rather than using locks.
    /// </summary>
    public class MessageLoop<T> : IDisposable
    {
        private readonly Task _task;
        private readonly BlockingCollection<Message> _msgs = new BlockingCollection<Message>();

        public MessageLoop(T state) : this(() => state, false) { }
        public MessageLoop() : this(() => default(T)) { }
        public MessageLoop(Func<T> stateFactory) : this(stateFactory, true) { }
        public MessageLoop(Func<T> stateFactory, bool disposeState)
        {
            stateFactory = stateFactory ?? (() => default(T));

            var readyExitCtor = new ManualResetEventSlim();
            _task = Task.Run(() =>
            {
                var state = stateFactory();
                using (disposeState ? state as IDisposable : null)
                {
                    readyExitCtor.Set();

                    foreach (var msg in _msgs.GetConsumingEnumerable())
                    {
                        var result = new MessageResult();
                        try
                        {
                            result.Value = msg.Factory(state);
                        }
                        catch (Exception ex)
                        {
                            result.Exception = ExceptionDispatchInfo.Capture(ex);
                        }

                        msg.CompletionSource.SetResult(result);
                    }
                }
            });

            readyExitCtor.Wait();
        }

        public async Task<TResult> GetAsync<TResult>(Func<T, TResult> factory)
        {
            var result = await SendMessageAsync(factory);

            if (result.Exception != null)
                result.Exception.Throw();

            return (TResult)result.Value;
        }

        public Task<TResult> GetAsync<TResult>(Func<TResult> factory)
        {
            return GetAsync(_ => factory());
        }

        public TResult Get<TResult>(Func<T, TResult> factory)
        {
            var result = SendMessageAsync(factory).Result;

            if (result.Exception != null)
                result.Exception.Throw();

            return (TResult)result.Value;
        }

        public TResult Get<TResult>(Func<TResult> factory)
        {
            return Get(_ => factory());
        }

        public async Task DoAsync(Action<T> action)
        {
            await GetAsync(src =>
            {
                action(src);
                return 0;
            });
        }

        public Task DoAsync(Action action)
        {
            return DoAsync(_ => action());
        }

        public void Do(Action<T> action)
        {
            Get(src =>
            {
                action(src);
                return 0;
            });
        }

        public void Do(Action action)
        {
            Do(_ => action());
        }

        private Task<MessageResult> SendMessageAsync<TResult>(Func<T, TResult> factory)
        {
            var msg = new Message(src => (object)factory(src));
            _msgs.Add(msg);
            return msg.CompletionSource.Task;
        }

        public void Dispose()
        {
            using (_task)
            using (_msgs)
            {
                _msgs.CompleteAdding();
                _task.Wait();
            }
        }

        class Message
        {
            public readonly TaskCompletionSource<MessageResult> CompletionSource
                = new TaskCompletionSource<MessageResult>();
            public readonly Func<T, object> Factory;

            public Message(Func<T, object> factory)
            {
                Factory = factory;
            }
        }

        class MessageResult
        {
            public object Value;
            public ExceptionDispatchInfo Exception;
        }
    }
}
