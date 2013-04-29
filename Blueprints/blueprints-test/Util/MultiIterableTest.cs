using NUnit.Framework;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "MultiIterableTest")]
    public class MultiIterableTest
    {
        [Test]
        public void TestBasicFunctionality()
        {
            var itty = new MultiIterable<int>(new List<IEnumerable<int>>{new[]{1, 2, 3}, new[]{4, 5}, new[]{6, 7, 8}});
            int counter = 0;
            foreach (int i in itty)
            {
                counter++;
                Assert.AreEqual(counter, i);
            }
            Assert.AreEqual(counter, 8);
        }
    }
}
