using NUnit.Framework;

namespace Frontenac.Blueprints
{
    [TestFixture(Category = "ParameterTest")]
    public class ParameterTest
    {
        [Test]
        public void TestEquality()
        {
            var a = new Parameter<string, long>("blah", 7L);
            var b = new Parameter<string, long>("blah", 7L);

            Assert.AreEqual(a, a);
            Assert.AreEqual(b, b);
            Assert.AreEqual(a, b);

            var c = new Parameter<string, long>("blah", 6L);

            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);

            var d = new Parameter<string, long>("boop", 7L);

            Assert.AreNotEqual(a, d);
            Assert.AreNotEqual(b, d);
            Assert.AreNotEqual(c, d);
        }
    }
}