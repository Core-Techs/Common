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
        public void CantDisposeGenericDisposableMoreThanOnce()
        {
            var d = this.AsDisposable(_ => { });
            Assert.False(d.Disposed);
            d.Dispose();
            Assert.True(d.Disposed);
            Assert.Throws<ObjectDisposedException>(() => d.Dispose());
        }
    }
}