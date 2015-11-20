using System;
using System.Threading;
using System.Threading.Tasks;
using CoreTechs.Common;
using NUnit.Framework;
using static System.Console;

namespace Tests
{
    public class MsgLoopTests
    {

        [Test]
        public void Stress()
        {
            const int iters = 1000;

            var state = new Wrapper<int>();
            using (var loop = new MessageLoop<Wrapper<int>>(() => state))
                Parallel.For(0, iters, i => loop.Do(x => ++x.Value));

            Assert.AreEqual(iters, state.Value);

        }

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

        [Test]
        public async void TestGetAsync()
        {
            var expected = "abc123";
            using (var loop = new MessageLoop<string>(() => expected))
                Assert.AreEqual(expected, await loop.GetAsync(async state => state));
        }

     

        [Test]
        public void TestGet()
        {
            var expected = "abc123";
            using (var loop = new MessageLoop<string>(() => expected))
            {
                Assert.AreEqual(expected, loop.Get(state => state));
            }
        }

        [Test]
        public void TestDo()
        {
            var expected = "abc123";
            string actual = null;
            using (var loop = new MessageLoop<string>(() => expected))
                loop.Do(state => actual = state);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async void TestDoAsync()
        {
            var expected = "abc123";
            string actual = null;
            using (var loop = new MessageLoop<string>(() => expected))
                await loop.DoAsync(async state => actual = state);

            Assert.AreEqual(expected, actual);
        }

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public void TestGetException()
        {
            using (var loop = new MessageLoop<string>(() => ""))
                loop.Get(state => DivZero());
        }

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public async void TestGetAsyncException()
        {
            using (var loop = new MessageLoop<string>(() => ""))
                await loop.GetAsync(async state => DivZero());
        }

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public async void TestDoAsyncException()
        {
            using (var loop = new MessageLoop<string>(() => ""))
                await loop.DoAsync(async state => DivZero());
        }

        [Test, ExpectedException(typeof(DivideByZeroException))]
        public void TestDoException()
        {
            using (var loop = new MessageLoop<string>(() => ""))
                loop.Do(state => DivZero());
        }

        private static int DivZero()
        {
            var z = 0;
            return 1/z;
        }

        [Test]
        public void TestStateFactoryErrorsExposed()
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
        public void CanNestMessages()
        {
            const int expected = 123;
            using (var loop = new MessageLoop<int>(() => expected))
            {
                var x = loop.Get(n => loop.Get(n2 => loop.Get(n3 => n3)));
                Assert.AreEqual(expected, x);
            }
        }

        [Test]
        public void CanInterceptMessages()
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
                loop.Do(w => i++);
                try
                {
                    loop.Do(w => DivZero());
                }
                catch 
                {
                }
            }

            Assert.AreEqual(5,i);
        }

        [Test]
        public void CanInterceptNestedMessages()
        {
            var normalInterceptions = 0;
            var nestedInterceptions = 0;

            const int expected = 123;
            using (var loop = new MessageLoop<int>(() => expected,interceptor: async (i, ctx) =>
            {
                if (ctx.NestedMessage) nestedInterceptions++;
                else normalInterceptions++;

                return await ctx.Func(i);
            }))
            {
                var x = loop.Get(n => loop.Get(n2 => loop.Get(n3 => n3)));
                Assert.AreEqual(expected, x);
                Assert.AreEqual(1, normalInterceptions);
                Assert.AreEqual(2, nestedInterceptions);

            }
        }
    }
}