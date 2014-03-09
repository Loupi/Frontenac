using System.Linq;
using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataReaderTest")]
    public class TinkerMetadataReaderTest : BaseTest
    {
        [SetUp]
        public void BeforeTest()
        {
            _graph = TinkerGraphFactory.CreateTinkerGraph();
            using (var stream = GetResource<TinkerMetadataReaderTest>("example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_graph, stream);
            }
        }

        private TinkerGraph _graph;

        [Test]
        public void ExampleMetadataGetsCorrectCurrentId()
        {
            Assert.AreEqual(_graph.CurrentId, 0);
        }

        [Test]
        public void ExampleMetadataGetsCorrectEdgeKeyIndices()
        {
            Assert.AreEqual(1, _graph.EdgeKeyIndex.Index.Count());
            Assert.AreEqual(1, _graph.GetEdges("weight", 0.5).Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectIndices()
        {
            Assert.AreEqual(2, _graph.Indices.Count());

            var idxAge = _graph.GetIndex("age", typeof (IVertex));
            var vertices = idxAge.Get("age", 27);
            Assert.AreEqual(1, vertices.Count());

            var idxWeight = _graph.GetIndex("weight", typeof (IEdge));
            var edges = idxWeight.Get("weight", 0.5);
            Assert.AreEqual(1, edges.Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectVertexKeyIndices()
        {
            Assert.AreEqual(1, _graph.VertexKeyIndex.Index.Count());
            Assert.AreEqual(1, _graph.GetVertices("age", 27).Count());
        }
    }
}