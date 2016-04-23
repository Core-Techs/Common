using CoreTechs.Common;
using Nito.AsyncEx;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class MsgLoopTests
    {

        [Test]
        public void CanGet() => AsyncContext.Run(async () =>
        {
            using (var loop = new MessageLoop<int>(() => 1))
            {
                var n = await loop.GetAsync(x => x);
                Assert.AreEqual(1, n);
            }
        });

        [Test]
        public void CanDoAsync() => AsyncContext.Run(async () =>
        {
            using (var loop = new MessageLoop<int>(() => 1))
            {
                int n = 0;
                await loop.DoAsync(x => n = x);
                Assert.AreEqual(1, n);
            }
        });

        [Test]
        public void CanGetAsync() => AsyncContext.Run(async () =>
        {
            using (var loop = new MessageLoop<int>(() => 1))
            {
                var n = await loop.GetAsync(x => x);
                Assert.AreEqual(1, n);
            }
        });

        [Test]
        public void Stress() => AsyncContext.Run(async () =>
        {
            const int workerCount = 10;
            const int iters = 100;

            using (var loop = new MessageLoop<Wrapper<int>>(() => new Wrapper<int>()))
            {
                var workers = System.Linq.Enumerable.Range(0, workerCount)
                .Select(i =>
                {
                    return Task.Run(() =>
                    {
                        for (int j = 0; j < iters; j++)
                        {
                            loop.DoAsync(w => w.Value++).Wait();
                        }
                    });
                }).ToArray();

                Task.WaitAll(workers);

                var n = await loop.GetAsync(async x =>
                {
                    return await Task.FromResult(x.Value);
                });
                Assert.AreEqual(workerCount*iters, n);
            }
        });

        [Test]
        public void StateIsDisposed()
        {
            var disposed = false;
            var state = 0.AsDisposable(_ => disposed = true);
            new MessageLoop<GenericDisposable<int>>(() => state).Dispose();
            Assert.True(disposed);
        }

        [Test]
        public void StateIsNotDisposed()
        {
            var disposed = false;
            var state = 0.AsDisposable(_ => disposed = true);
            new MessageLoop<GenericDisposable<int>>(() => state, false).Dispose();
            Assert.False(disposed);
        }

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public void TestGetAsyncException() => AsyncContext.Run(async () => 
        {
            using (var loop = new MessageLoop<string>(() => ""))
                await loop.GetAsync(state => Task.FromResult(DivZero()));
        });



        [Test, ExpectedException(typeof(DivideByZeroException))]
        public void TestDoAsyncException() => AsyncContext.Run(async () =>
        {
            using (var loop = new MessageLoop<string>(() => ""))
                await loop.DoAsync(state => Task.FromResult(DivZero()));
        });

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public void TestDoException() => AsyncContext.Run(async () =>
        {
            using (var loop = new MessageLoop<string>(() => ""))
                await loop.DoAsync(state => DivZero());
        });

        private static int DivZero()
        {
            var z = 0;
            return 1 / z;
        }

        [Test]
        public void StateFactoryErrorsExposed()
        {
            var exception = new Exception("AHH!");

            try
            {
                using (new MessageLoop<int>(() => { throw exception; }, false)) { }
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex, exception);
            }
        }

        [Test]
        public void CanNestMessages() => AsyncContext.Run(async () =>
        {
            const int expected = 123;
            using (var loop = new MessageLoop<int>(() => expected))
            {
                var x = await loop.GetAsync(async n => await loop.GetAsync(async n2 => await loop.GetAsync(n3 => n3)));
                Assert.AreEqual(expected, x);
            }
        });

        [Test]
        public void CanInterceptMessages() => AsyncContext.Run(async () =>
        {
            var i = 0;

            using (var loop = new MessageLoop<int>(() => 0, interceptor: async (s, ctx) =>
            {
                i++;
                try
                {
                    var result = await ctx.Func(s);
                    return result;
                }
                finally
                {
                    i++;
                }
            }))
            {
                await loop.DoAsync(w => Task.FromResult(i++));
                try
                {
                    await loop.DoAsync(w => DivZero());
                }
                catch
                {
                }
            }

            Assert.AreEqual(5, i);
        });

        [Test]
        public void CanInterceptNestedMessages() => AsyncContext.Run(async () =>
        {
            var normalInterceptions = 0;
            var nestedInterceptions = 0;

            const int expected = 123;
            using (var loop = new MessageLoop<int>(() => expected, interceptor: (i, ctx) =>
            {
                if (ctx.NestedMessage) nestedInterceptions++;
                else normalInterceptions++;

                return ctx.Func(i);
            }))
            {
                var x = await loop.GetAsync(async n => await loop.GetAsync(async n2 => await loop.GetAsync(n3 => n3)));
                Assert.AreEqual(expected, x);
                Assert.AreEqual(1, normalInterceptions);
                Assert.AreEqual(2, nestedInterceptions);

            }
        });
    }
}