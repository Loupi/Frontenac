using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "IndexableGraphHelperTest")]
    public class IndexableGraphHelperTest : BaseTest
    {
        [Test]
        public void TestAddUniqueVertex()
        {
            var graph = new TinkerGraph();
            var marko = graph.AddVertex(0);
            marko.SetProperty("name", "marko");
            var index = graph.CreateIndex("txIdx", typeof (IVertex));
            index.Put("name", "marko", marko);
            var vertex = IndexableGraphHelper.AddUniqueVertex(graph, null, index, "name", "marko");
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex, graph.GetVertex(0));
            Assert.AreEqual(Count(graph.GetVertices()), 1);
            Assert.AreEqual(Count(graph.GetEdges()), 0);

            vertex = IndexableGraphHelper.AddUniqueVertex(graph, null, index, "name", "darrick");
            Assert.AreEqual(vertex.GetProperty("name"), "darrick");
            Assert.AreEqual(Count(graph.GetVertices()), 2);
            Assert.AreEqual(vertex.Id, "1");
        }
    }
}