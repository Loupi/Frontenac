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
            _tinkerGraph = TinkerGraphFactory.CreateTinkerGraph();
            using (var stream = GetResource<TinkerMetadataReaderTest>("example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_tinkerGraph, stream);
            }
        }

        private TinkerGraph _tinkerGraph;

        [Test]
        public void ExampleMetadataGetsCorrectCurrentId()
        {
            Assert.AreEqual(_tinkerGraph.CurrentId, 0);
        }

        [Test]
        public void ExampleMetadataGetsCorrectEdgeKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGraph.EdgeKeyIndex.Index.Count);
            Assert.AreEqual(1, _tinkerGraph.GetEdges("weight", 0.5).Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectIndices()
        {
            Assert.AreEqual(2, _tinkerGraph.Indices.Count);

            var idxAge = _tinkerGraph.GetIndex("age", typeof (IVertex));
            var vertices = idxAge.Get("age", 27);
            Assert.AreEqual(1, vertices.Count());

            var idxWeight = _tinkerGraph.GetIndex("weight", typeof (IEdge));
            var edges = idxWeight.Get("weight", 0.5);
            Assert.AreEqual(1, edges.Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectVertexKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGraph.VertexKeyIndex.Index.Count);
            Assert.AreEqual(1, _tinkerGraph.GetVertices("age", 27).Count());
        }
    }
}