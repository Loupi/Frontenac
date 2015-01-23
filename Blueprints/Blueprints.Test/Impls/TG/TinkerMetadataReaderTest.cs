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
            _tinkerGraĥ = TinkerGraphFactory.CreateTinkerGraph();
            using (var stream = GetResource<TinkerMetadataReaderTest>("example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_tinkerGraĥ, stream);
            }
        }

        private TinkerGraĥ _tinkerGraĥ;

        [Test]
        public void ExampleMetadataGetsCorrectCurrentId()
        {
            Assert.AreEqual(_tinkerGraĥ.CurrentId, 0);
        }

        [Test]
        public void ExampleMetadataGetsCorrectEdgeKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGraĥ.EdgeKeyIndex.Index.Count());
            Assert.AreEqual(1, _tinkerGraĥ.GetEdges("weight", 0.5).Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectIndices()
        {
            Assert.AreEqual(2, _tinkerGraĥ.Indices.Count());

            var idxAge = _tinkerGraĥ.GetIndex("age", typeof (IVertex));
            var vertices = idxAge.Get("age", 27);
            Assert.AreEqual(1, vertices.Count());

            var idxWeight = _tinkerGraĥ.GetIndex("weight", typeof (IEdge));
            var edges = idxWeight.Get("weight", 0.5);
            Assert.AreEqual(1, edges.Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectVertexKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGraĥ.VertexKeyIndex.Index.Count());
            Assert.AreEqual(1, _tinkerGraĥ.GetVertices("age", 27).Count());
        }
    }
}