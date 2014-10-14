using System.Linq;
using CoreTechs.Common.Reflection;
using NUnit.Framework;

namespace Tests
{
    public class ReflectionTests : ReflectionTestsBase, IReflectionTests
    {
        [Test]
        public void CanGetPropertiesAsDeclared()
        {
            var props = GetType().GetPropertiesAsDeclared();

            Assert.AreEqual(3,props.Length);
            CollectionAssert.AllItemsAreNotNull(props);
            Assert.True(props.All(x => x.CanRead && x.CanWrite),"All properties are not readable and writable");
        }

        public string SomeString { get; set; }
        int IReflectionTests.SomeInt { get; set; }
    }

    public abstract class ReflectionTestsBase
    {
        public decimal SomeDecimal { get; set; }
    }

    public interface IReflectionTests
    {
        int SomeInt { get; set; }
    }
}