using System.Collections.Generic;
using System.Linq;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests.IdentifiedCollection
{
    [TestFixture]
    public class UsingIdentifiedCollectionForEntitiesWithNullableId
    {
        private IIdentifiedCollection<int?, Entity> _identifiedCollection;

        [SetUp]
        public void GivenLoadedIdentifiedCollection()
        {
            _identifiedCollection = new IdentifiedCollection<int?, Entity>(e => e.Id == 0 ? (int?)null : e.Id)
            {
                new Entity(0, "Zero1"),
                new Entity(0, "Zero2"),
                new Entity(0, "Zero3"),
                new Entity(1, "one"),
                new Entity(2, "two"),
            };
        }

        [TestCase(1, Result = "one")]
        [TestCase(2, Result = "two")]
        public string CanLookUpEntitiesByKey(int key)
        {
            return _identifiedCollection[key].Data;
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(0)]
        public void MissingKeysThrowExceptions(int key)
        {
            Assume.That(!_identifiedCollection.ContainsKey(key));
            Entity item;

            TestDelegate gettingItemByMissingKey = () => item = _identifiedCollection[key];

            Assert.Throws<KeyNotFoundException>(gettingItemByMissingKey);
        }

        [Test]
        public void WhenIteratingKeylessItemsExist()
        {
            var expectedData = new[] { "Zero1", "Zero2", "Zero3", "one", "two" };

            Assert.That(_identifiedCollection.LongCount(), Is.EqualTo(5));
            Assert.That(_identifiedCollection.Select(it => it.Data), Is.EquivalentTo(expectedData));
        }

        [Test]
        public void WhenViewingAsReadOnlyDictionaryKeylessItemsDontExist()
        {
            var asDict = _identifiedCollection.AsReadOnlyDictionary();
            var expectedData = new[] { "one", "two" };

            Assert.That(asDict.LongCount(), Is.EqualTo(2));
            Assert.That(asDict.Keys.LongCount(), Is.EqualTo(2));
            Assert.That(asDict.Values.Select(en => en.Data), Is.EquivalentTo(expectedData));
        }

        [Test]
        public void UpsertingNullsAlwaysInserts()
        {
            Assume.That(_identifiedCollection, Has.Count.EqualTo(5));

            var zeroEntity = new Entity(0, "newzero");

            var results = new[]
            {
                _identifiedCollection.Upsert(zeroEntity),
                _identifiedCollection.Upsert(zeroEntity),
                _identifiedCollection.Upsert(zeroEntity),
            };

            Assert.That(results, Is.All.EqualTo(UpsertResult.Inserted));
            Assert.That(_identifiedCollection, Has.Count.EqualTo(8));
            Assert.That(_identifiedCollection.Last().Data, Is.EqualTo("newzero"));
        }

        #region Classes used for tests
        private class Entity
        {
            public int Id { get; private set; }
            public string Data { get; private set; }

            public Entity(int id, string data)
            {
                Id = id;
                Data = data;
            }
        }
        #endregion
    }
}
