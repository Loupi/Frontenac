using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    [TestFixture(Category = "ParameterTest")]
    public class ParameterTest
    {
        [Test]
        public void testEquality()
        {
            Parameter<string, long> a = new Parameter<string, long>("blah", 7L);
            Parameter<string, long> b = new Parameter<string, long>("blah", 7L);

            Assert.AreEqual(a, a);
            Assert.AreEqual(b, b);
            Assert.AreEqual(a, b);

            Parameter<string, long> c = new Parameter<string, long>("blah", 6L);

            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);

            Parameter<string, long> d = new Parameter<string, long>("boop", 7L);

            Assert.AreNotEqual(a, d);
            Assert.AreNotEqual(b, d);
            Assert.AreNotEqual(c, d);
        }
    }
}
