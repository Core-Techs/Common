using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class AttemptTests
    {

        private int DivByZero()
        {
            var zero = 0;
            return 1 / zero;
        }

        [Test]
        public void CanCatchWhenPredicateSatisfied()
        {
            Attempt.Get(DivByZero).CatchIf(a => true);
        }

        [Test]
        public void NoCatchWhenPredicateNotSatisfied()
        {

            Assert.Throws<DivideByZeroException>(() =>
                Attempt.Get(DivByZero).CatchIf(a => false));
        }

        [Test]
        public void CanCatchWhenExceptionIs()
        {
            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).CatchIfExceptionIs<SqlTypeException>();
        }

        [Test]
        public void NoCatchWhenExceptionIsNotExactly()
        {
            Assert.Throws<SqlAlreadyFilledException>(() =>
                Attempt.Do(() =>
                {
                    throw new SqlAlreadyFilledException();
                }).CatchIfExceptionIsExactly<SqlTypeException>());
        }

        [Test]
        public void CanCatchWhenExceptionIsExactly()
        {
            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).CatchIfExceptionIs<SqlAlreadyFilledException>();
        }

        [Test]
        public void NoCatchWhenExceptionIsNot()
        {
            Assert.Throws<SqlAlreadyFilledException>(() =>
                Attempt.Do(() =>
                {
                    throw new SqlAlreadyFilledException();
                }).CatchIfExceptionIs<ArgumentNullException>());
        }

        [Test]
        public void CanThrowWhenPredicateSatisfied()
        {

            Assert.Throws<DivideByZeroException>(() =>
                Attempt.Get(DivByZero).ThrowIf(a => true));
        }

        [Test]
        public void NoThrowWhenPredicateNotSatisfied()
        {
            Attempt.Get(DivByZero).ThrowIf(a => false);
        }

        [Test]
        public void CanThrowWhenExceptionIs()
        {
            Assert.Throws<SqlAlreadyFilledException>(() =>

            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).ThrowIfExceptionIs<SqlTypeException>());
        }

        [Test]
        public void NoThrowWhenExceptionIsNotExactly()
        {
            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).ThrowIfExceptionIsExactly<SqlTypeException>();
        }

        [Test]
        public void CanThrowWhenExceptionIsExactly()
        {
            Assert.Throws<SqlAlreadyFilledException>(() =>
            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).ThrowIfExceptionIs<SqlAlreadyFilledException>());
        }

        [Test]
        public void NoThrowWhenExceptionIsNot()
        {
            Attempt.Do(() =>
            {
                throw new SqlAlreadyFilledException();
            }).ThrowIfExceptionIs<ArgumentNullException>();
        }

        [Test]
        public void RepeatedAttemptThrowWhereThrowsWhenPredSatisfied()
        {
            Assert.Throws<DivideByZeroException>(() =>
                Attempt.Repeatedly.Get(DivByZero).ThrowWhere(x => true).ToArray());
        }

        [Test]
        public void RepeatedAttemptThrowWhereCatchesWhenPredNotSatisfied()
        {
            Attempt.Repeatedly.Get(DivByZero).ThrowWhere(x => false).Take(2).Execute();
        }

        [Test]
        public void RepeatedAttemptCatchWhere_CatchesWhenPredSatisfied()
        {
            Attempt.Repeatedly.Get(DivByZero).CatchWhere(x => true).Take(2).Execute();
        }

        [Test]
        public void RepeatedAttemptCatchWhereThrowsWhenPredNotSatisfied()
        {
            Assert.Throws<DivideByZeroException>(() =>
                Attempt.Repeatedly.Get(DivByZero).CatchWhere(x => false).ToArray());
        }

        [Test]
        async public Task CanDoAsync()
        {
            var value = 0;
            var attempt = await Attempt.DoAsync(() => Task.Run(() => value = 123));
            Assert.True(attempt.Succeeded);
            Assert.AreEqual(123, value);
        }

        [Test]
        async public Task CanGetAsync()
        {
            var attempt = await Attempt.GetAsync(() => Task.FromResult(123));
            Assert.True(attempt.Succeeded);
            Assert.AreEqual(123, attempt.Value);
        }

        [Test]
        async public Task CanGetAsync2()
        {
            var attempt = await 123.AttemptGetAsync(Task.FromResult);
            Assert.True(attempt.Succeeded);
            Assert.AreEqual(123, attempt.Value);
        }

        [Test]
        async public Task CanRepeatedlyDoAsync()
        {
            var value = 0;

            await Attempt.Repeatedly.DoAsync(() =>
            {
                return  Task.Run(() =>
                {
                    if (RNG.NextBool(.99))
                        throw new ApplicationException("FAILED!");

                    value = 123;
                });
            }).ExecuteAsync();

            Assert.AreEqual(123,value);
        }

        [Test]
        async public Task CanRepeatedlyGetAsync()
        {
            var failures = 0;
            var attempts = await Attempt.Repeatedly.GetAsync(() =>
            {
              return Task.Run(() =>
                {
                    if (RNG.NextBool(.99))
                    {
                        failures++;
                        throw new ApplicationException("FAILED!");
                    }

                    return 123;
                });
            }).ExecuteAsync();

            Console.WriteLine(failures);
            Assert.AreEqual(123,attempts.GetValueOrDefault());
        }
     
    }
}