using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "PropertyFilteredIterableTest")]
    public class PropertyFilteredIterableTest : BaseTest
    {
        [Test]
        public void testBasicFunctionality()
        {
            TinkerGraph graph = new TinkerGraph();
            Vertex a = graph.addVertex("a");
            a.setProperty("age", 29);
            Vertex b = graph.addVertex("b");
            b.setProperty("age", 29);
            Vertex c = graph.addVertex("c");
            c.setProperty("age", 30);
            Vertex d = graph.addVertex("d");
            d.setProperty("age", 31);

            // throw a vertex without the expected key in the mix
            Vertex e = graph.addVertex("e");
            List<Vertex> list = new List<Vertex>{a, b, c, d, e};

            PropertyFilteredIterable<Vertex> iterable = new PropertyFilteredIterable<Vertex>("age", 29, list);
            Assert.AreEqual(count(iterable), 2);
            Assert.AreEqual(count(iterable), 2);
            foreach (Vertex vertex in iterable)
                Assert.True(vertex.Equals(a) || vertex.Equals(b));
            
            iterable = new PropertyFilteredIterable<Vertex>("age", 30, list);
            Assert.AreEqual(count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<Vertex>("age", 30, graph.getVertices());
            Assert.AreEqual(count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<Vertex>("age", 37, list);
            Assert.AreEqual(count(iterable), 0);
        }
    }
}
