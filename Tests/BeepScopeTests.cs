using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class BeepScopeTests
    {
        [Test]
        public void CanDisposeQuickly()
        {
            for (var i = 0; i < 100; i++)
                using (new BeepScope()) { }
        }
        
    }
}