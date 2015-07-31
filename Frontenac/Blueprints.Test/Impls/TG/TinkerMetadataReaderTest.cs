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
            _tinkerGrapĥ = TinkerGraphFactory.CreateTinkerGraph();
            using (var stream = GetResource<TinkerMetadataReaderTest>("example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_tinkerGrapĥ, stream);
            }
        }

        private TinkerGrapĥ _tinkerGrapĥ;

        [Test]
        public void ExampleMetadataGetsCorrectCurrentId()
        {
            Assert.AreEqual(_tinkerGrapĥ.CurrentId, 0);
        }

        [Test]
        public void ExampleMetadataGetsCorrectEdgeKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGrapĥ.EdgeKeyIndex.Index.Count());
            Assert.AreEqual(1, _tinkerGrapĥ.GetEdges("weight", 0.5).Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectIndices()
        {
            Assert.AreEqual(2, _tinkerGrapĥ.Indices.Count());

            var idxAge = _tinkerGrapĥ.GetIndex("age", typeof (IVertex));
            var vertices = idxAge.Get("age", 27);
            Assert.AreEqual(1, vertices.Count());

            var idxWeight = _tinkerGrapĥ.GetIndex("weight", typeof (IEdge));
            var edges = idxWeight.Get("weight", 0.5);
            Assert.AreEqual(1, edges.Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectVertexKeyIndices()
        {
            Assert.AreEqual(1, _tinkerGrapĥ.VertexKeyIndex.Index.Count());
            Assert.AreEqual(1, _tinkerGrapĥ.GetVertices("age", 27).Count());
        }
    }
}