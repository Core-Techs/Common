using System;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class DisposableTests
    {
        [Test]
        public void DisposeAnything()
        {
            var obj = new
            {
                Name = "Ronnie",
                BirthDate = new DateTime(1984, 5, 10)
            };

            int? age = null;
            
            var disposable = obj.AsDisposable(x => age = (int) (DateTime.Now - x.BirthDate).TotalYears());
            
            using (disposable)
                Assert.Null(age);

            Assert.NotNull(age);
        }

        [Test]
        public void CanDisposeGenericDisposableMoreThanOnceWithNoEffect()
        {
            var num = 0;
            var d = this.AsDisposable(_ => { ++num; });
            Assert.False(d.Disposed);
            d.Dispose();
            Assert.AreEqual(num, 1);
            Assert.True(d.Disposed);
            d.Dispose();
            Assert.AreEqual(num, 1);
        }
    }
}