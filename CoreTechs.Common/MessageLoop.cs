using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    /// <summary>
    /// A task based message loop that allows for interacting with generic state.
    /// This is useful when you need to interact with an object from multiple threads,
    /// but that object is not thread safe or it requires thread affinity.
    /// </summary>
    public class MessageLoop<T> : IDisposable
    {
        private readonly Task _task;
        private readonly BlockingCollection<Message> _msgs = new BlockingCollection<Message>();

        public MessageLoop(T state) : this(() => state, false) { }
        public MessageLoop(Func<T> stateFactory) : this(stateFactory, true) { }
        public MessageLoop(Func<T> stateFactory, bool disposeState)
        {
            var ready = new ManualResetEventSlim();
            _task = Task.Run(() =>
            {
                var state = stateFactory();
                using (disposeState ? state as IDisposable : null)
                {
                    ready.Set();

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
                    }}
            });

            ready.Wait();
        }

        public async Task<TResult> GetAsync<TResult>(Func<T, TResult> factory)
        {
            var result = await SendMessage(factory);

            if (result.Exception != null)
                result.Exception.Throw();

            return (TResult)result.Value;
        }

        public TResult Get<TResult>(Func<T, TResult> factory)
        {
            var result = SendMessage(factory).Result;

            if (result.Exception != null)
                result.Exception.Throw();

            return (TResult)result.Value;
        }

        public async Task DoAsync(Action<T> action)
        {
            await GetAsync(src =>
            {
                action(src);
                return 0;
            });
        }

        public void Do(Action<T> action)
        {
            Get(src =>
            {
                action(src);
                return 0;
            });
        }

        private Task<MessageResult> SendMessage<TResult>(Func<T, TResult> factory)
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
