using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "IndexableGraphHelperTest")]
    public class IndexableGraphHelperTest : BaseTest
    {
        [Test]
        public void testAddUniqueVertex()
        {
            IndexableGraph graph = new TinkerGraph();
            Vertex marko = graph.addVertex(0);
            marko.setProperty("name", "marko");
            Index index = graph.createIndex("txIdx", typeof(Vertex));
            index.put("name", "marko", marko);
            Vertex vertex = IndexableGraphHelper.addUniqueVertex(graph, null, index, "name", "marko");
            Assert.AreEqual(vertex.getProperty("name"), "marko");
            Assert.AreEqual(vertex, graph.getVertex(0));
            Assert.AreEqual(count(graph.getVertices()), 1);
            Assert.AreEqual(count(graph.getEdges()), 0);

            vertex = IndexableGraphHelper.addUniqueVertex(graph, null, index, "name", "darrick");
            Assert.AreEqual(vertex.getProperty("name"), "darrick");
            Assert.AreEqual(count(graph.getVertices()), 2);
            Assert.AreEqual(vertex.getId(), "1");
        }
    }
}
