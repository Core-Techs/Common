using System;
using System.Collections.Generic;
using System.Linq;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests.IdentifiedCollection
{
    [TestFixture]
    public class UsingIdentifiedCollectionForEntitiesWithIntegerId
    {
        private IIdentifiedCollection<int, Entity> _identifiedCollection;

        [SetUp]
        public void GivenLoadedIdentifiedCollection()
        {
            _identifiedCollection = new IdentifiedCollection<int, Entity>(e => e.Id)
            {
                new Entity(1, "one"),
                new Entity(2, "two"),
                new Entity(3, "three"),
            };
        }

        [TestCase(1, "one")]
        [TestCase(2, "two")]
        [TestCase(3, "three")]
        public void CanLookUpEntitiesByKey(int key, string result)
        {
            Assert.AreEqual(result, _identifiedCollection[key].Data);
        }

        [TestCase(0, false)]
        [TestCase(1,  true)]
        [TestCase(2,  true)]
        [TestCase(3,  true)]
        [TestCase(4,  false)]
        [TestCase(5,  false)]
        public void CanSeeWhetherKeyIsContained(int key, bool result)
        {
            Assert.AreEqual(result, _identifiedCollection.ContainsKey(key));
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(0)]
        public void MissingKeysThrowExceptions(int key)
        {
            Assume.That(!_identifiedCollection.ContainsKey(key));
            Entity item;

            TestDelegate gettingItemByMissingKey = () => item = _identifiedCollection[key];

            Assert.Throws<KeyNotFoundException>(gettingItemByMissingKey);
        }

        [Test]
        public void CanTryToGetItemsWithKey()
        {
            Entity missingItem;
            Entity existingItem;

            var foundMissingItem = _identifiedCollection.TryGetValue(4, out missingItem);
            var foundExistingItem = _identifiedCollection.TryGetValue(3, out existingItem);

            Assert.That(foundMissingItem, Is.False);
            Assert.That(foundExistingItem, Is.True);
            Assert.That(existingItem, Is.Not.Null);
            Assert.That(existingItem.Data, Is.EqualTo("three"));
        }

        [Test]
        public void IteratingOverItemsResultsInSameOrderAsInsertion()
        {
            var incrementingIdCol = new IdentifiedCollection<int, Entity>(it => it.Id);
            foreach (var i in System.Linq.Enumerable.Range(1, 10))
                incrementingIdCol.Add(new Entity(i, string.Empty));

            var prevId = 0;
            foreach (var item in incrementingIdCol)
            {
                Assert.That(item.Id, Is.EqualTo(prevId + 1));
                prevId = item.Id;
            }
        }

        [Test]
        public void CanClearItemsFromCollection()
        {
            Assume.That(_identifiedCollection, Is.Not.Empty);
            Assume.That(_identifiedCollection.ContainsKey(1), Is.True);

            _identifiedCollection.Clear();

            Assert.That(_identifiedCollection, Is.Empty);
            Assert.That(_identifiedCollection.ContainsKey(1), Is.False);
        }

        [Test]
        public void CanRemoveBasedOnKey()
        {
            Assume.That(_identifiedCollection, Has.Count.GreaterThan(0));
            var entityToRemove = _identifiedCollection.First();

            var entityWasRemoved = _identifiedCollection.Remove(entityToRemove.Id);

            Assert.That(entityWasRemoved);
            Assert.That(_identifiedCollection.ContainsKey(entityToRemove.Id), Is.False, "The key should have been removed from the collection");
            Assert.That(_identifiedCollection.Contains(entityToRemove), Is.False, "The entity should have been removed from the collection");
        }

        [Test]
        public void CantInsertItemsThatWouldDuplicateKeys()
        {
            Assume.That(_identifiedCollection.ContainsKey(1));
            var duplicateEntity = new Entity(1, "Impossible To Insert");

            TestDelegate insertionOfDuplicateEntity = () => _identifiedCollection.Add(duplicateEntity);

            Assert.Throws<ArgumentException>(insertionOfDuplicateEntity);
        }

        [Test]
        public void CanUpsertNewItems()
        {
            Assume.That(!_identifiedCollection.ContainsKey(4));
            var newEntity = new Entity(4, "four");

            var result = _identifiedCollection.Upsert(newEntity);

            Assert.That(result, Is.EqualTo(UpsertResult.Inserted));
            Assert.That(_identifiedCollection, Has.Count.EqualTo(4));
        }

        [Test]
        public void CanUpsertReplacingItems()
        {
            Assume.That(_identifiedCollection.ContainsKey(2));
            var changedEntity = new Entity(2, "changed");
            var existingEntity = _identifiedCollection[2];

            var result = _identifiedCollection.Upsert(changedEntity);

            Assert.That(result, Is.EqualTo(UpsertResult.Updated));
            Assert.That(_identifiedCollection, Has.Count.EqualTo(3));
            Assert.That(_identifiedCollection[2].Data, Is.EqualTo("changed"));
            Assert.That(existingEntity.Data, Is.EqualTo("two"));
        }

        [Test]
        public void CanUpsertExistingItems()
        {
            Assume.That(_identifiedCollection, Is.Not.Empty);
            var existingEntity = _identifiedCollection.First();

            var result = _identifiedCollection.Upsert(existingEntity);

            Assert.That(result, Is.EqualTo(UpsertResult.Updated));
            Assert.That(_identifiedCollection, Has.Count.EqualTo(3));
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
        #endregion Classes used for tests
    }
}
