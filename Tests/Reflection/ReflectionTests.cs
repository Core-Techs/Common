using System;
using CoreTechs.Common.Reflection;
using NUnit.Framework;

namespace Tests.Reflection
{
    public class ReflectionTests
    {
        public string Name { get; set; }

        [Test]
        public void TestGetProperty()
        {

            for (int i = 0; i < 1000000; i++)
            {
                Name = i.ToString();
                var name = this.GetPropertyValue<string>("Name");
                Assert.AreEqual(Name, name);
            }

        }

        [Test]
        public void TestSetProperty()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var name = i.ToString();
                this.SetPropertyValue("Name", i.ToString());
                Assert.AreEqual(name, Name);
            }
        }

    }
}