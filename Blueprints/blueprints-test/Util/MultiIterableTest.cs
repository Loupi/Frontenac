using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "MultiIterableTest")]
    public class MultiIterableTest
    {
        [Test]
        public void testBasicFunctionality()
        {
            MultiIterable<int> itty = new MultiIterable<int>(new List<IEnumerable<int>>{new int[]{1, 2, 3}, new int[]{4, 5}, new int[]{6, 7, 8}});
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
