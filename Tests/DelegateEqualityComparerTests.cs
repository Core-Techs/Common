using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class DelegateEqualityComparerTests
    {
        [Test]
        public void CanMakeEqualityTest()
        {
            var first = new ExampleClass {Number = 1, Word = "Hat"};
            var second = new ExampleClass {Number = 1, Word = "Sock"};
            var comparer = new DelegateEqualityComparer<ExampleClass>((c1, c2) => c1.Number == c2.Number);

            var defaultEquals = first == second;
            var result = comparer.Equals(first, second);

            Assume.That(defaultEquals, Is.False);
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanAlsoRecognizeInequality()
        {
            var first = new ExampleClass { Number = 1, Word = "Hat" };
            var second = new ExampleClass { Number = 2, Word = "Hat" };
            var comparer = new DelegateEqualityComparer<ExampleClass>((c1, c2) => c1.Number == c2.Number);

            var defaultEquals = first == second;
            var result = comparer.Equals(first, second);

            Assume.That(defaultEquals, Is.False);
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanComputeHashOfClass()
        {
            var first = new ExampleClass();
            var comparer = new DelegateEqualityComparer<ExampleClass>((c1, c2) => false, c => 1);

            var result = comparer.GetHashCode(first);

            Assert.That(result, Is.EqualTo(1));
        }

        private class ExampleClass
        {
            public int Number { get; set; }
            public string Word { get; set; }
        }
    }
}
