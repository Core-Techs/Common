using System.Threading.Tasks;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class MsgLoopTests
    {
        private const int Iters = 1000000;
        private int _i;

        [Test]
        public void Test()
        {
            using (var loop = new MessageLoop<MsgLoopTests>(this))
                Parallel.For(0, Iters, i => loop.Do(x => ++x._i));

            Assert.AreEqual(Iters,_i);
        }

        [Test]
        public void Test2()
        {
            _i = 0;
            Parallel.For(0, Iters, i => ++_i);
            Assert.AreNotEqual(Iters, _i);


        }
    }
}