using System;
using System.Linq;
using System.Reflection;
using CoreTechs.Common.Reflection;
using NUnit.Framework;

namespace Tests.Reflection
{
    public class ReflectionTests
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public decimal? Money { get; set; }
        public TimeSpan? Duration { get; set; }
        private object PrivateProperty { get; set; }
        internal object InternalProperty { get; set; }
        protected object ProtectedProperty { get; set; }
        static public object StaticPublicProperty { get; set; }
        static private object StaticPrivateProperty { get; set; }

        [Test]
        public void TestGetProperties()
        {
            var props = this.GetAllProperties();
            var type = GetType();

            CollectionAssert.AreEqual(new[]
            {
                type.GetProperty("Name"),
                type.GetProperty("Age"),
                type.GetProperty("Money"),
                type.GetProperty("Duration"),
                type.GetProperty("PrivateProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                type.GetProperty("InternalProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                type.GetProperty("ProtectedProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                type.GetProperty("StaticPublicProperty", BindingFlags.Public | BindingFlags.Static),
                type.GetProperty("StaticPrivateProperty", BindingFlags.NonPublic | BindingFlags.Static),
            }, props);
        }

        [Test]
        public void TestGetProperty()
        {
            for (decimal i = 0; i < 100000; i++)
            {
                Duration = TimeSpan.FromTicks((long)i);
                Name = i.ToString();
                Money = i;
                Age = (int)i;

                var name = this.GetPropertyValue<string>("Name");
                var money = this.GetPropertyValue<decimal>("Money");
                var age = this.GetPropertyValue<int>("Age");
                var dur = this.GetPropertyValue<TimeSpan>("Duration");

                Assert.AreEqual(Name, name);
                Assert.AreEqual(Money, money);
                Assert.AreEqual(Age, age);
                Assert.AreEqual(Duration, dur);
            }
        }

        [Test]
        public void TestSetProperty()
        {
            for (decimal i = 0; i < 100000; i++)
            {
                var name = i.ToString();
                var age = (int)i;
                decimal? money = i;
                var dur = TimeSpan.FromTicks((long)i);

                this.SetPropertyValue("Duration", dur);
                this.SetPropertyValue("Name", i.ToString());
                this.SetPropertyValue("Age", age);
                this.SetPropertyValue("Money", money);

                Assert.AreEqual(name, Name);
                Assert.AreEqual(dur, Duration);
                Assert.AreEqual(age, Age);
                Assert.AreEqual(money, Money);
            }
        }

    }
}