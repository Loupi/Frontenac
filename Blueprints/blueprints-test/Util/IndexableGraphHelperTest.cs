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
        public void TestAddUniqueVertex()
        {
            IndexableGraph graph = new TinkerGraph();
            Vertex marko = graph.AddVertex(0);
            marko.SetProperty("name", "marko");
            Index index = graph.CreateIndex("txIdx", typeof(Vertex));
            index.Put("name", "marko", marko);
            Vertex vertex = IndexableGraphHelper.AddUniqueVertex(graph, null, index, "name", "marko");
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex, graph.GetVertex(0));
            Assert.AreEqual(Count(graph.GetVertices()), 1);
            Assert.AreEqual(Count(graph.GetEdges()), 0);

            vertex = IndexableGraphHelper.AddUniqueVertex(graph, null, index, "name", "darrick");
            Assert.AreEqual(vertex.GetProperty("name"), "darrick");
            Assert.AreEqual(Count(graph.GetVertices()), 2);
            Assert.AreEqual(vertex.GetId(), "1");
        }
    }
}
