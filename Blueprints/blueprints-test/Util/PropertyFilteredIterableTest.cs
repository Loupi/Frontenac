using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "PropertyFilteredIterableTest")]
    public class PropertyFilteredIterableTest : BaseTest
    {
        [Test]
        public void TestBasicFunctionality()
        {
            var graph = new TinkerGraph();
            IVertex a = graph.AddVertex("a");
            a.SetProperty("age", 29);
            IVertex b = graph.AddVertex("b");
            b.SetProperty("age", 29);
            IVertex c = graph.AddVertex("c");
            c.SetProperty("age", 30);
            IVertex d = graph.AddVertex("d");
            d.SetProperty("age", 31);

            // throw a vertex without the expected key in the mix
            IVertex e = graph.AddVertex("e");
            var list = new List<IVertex>{a, b, c, d, e};

            var iterable = new PropertyFilteredIterable<IVertex>("age", 29, list);
            Assert.AreEqual(Count(iterable), 2);
            Assert.AreEqual(Count(iterable), 2);
            foreach (IVertex vertex in iterable)
                Assert.True(vertex.Equals(a) || vertex.Equals(b));
            
            iterable = new PropertyFilteredIterable<IVertex>("age", 30, list);
            Assert.AreEqual(Count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<IVertex>("age", 30, graph.GetVertices());
            Assert.AreEqual(Count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<IVertex>("age", 37, list);
            Assert.AreEqual(Count(iterable), 0);
        }
    }
}
