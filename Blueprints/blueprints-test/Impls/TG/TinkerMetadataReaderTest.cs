using NUnit.Framework;
using System.Linq;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataReaderTest")]
    public class TinkerMetadataReaderTest
    {
        TinkerGraph _graph;

        [SetUp]
        public void BeforeTest()
        {
            _graph = TinkerGraphFactory.CreateTinkerGraph();
        }

        [Test]
        public void ExampleMetadataGetsCorrectCurrentId()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_graph, stream);
            }

            Assert.AreEqual(_graph.CurrentId, 0);
        }

        [Test]
        public void ExampleMetadataGetsCorrectIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_graph, stream);
            }
            
            Assert.AreEqual(2, _graph.Indices.Count());

            IIndex idxAge = _graph.GetIndex("age", typeof(IVertex));
            ICloseableIterable<IElement> vertices = idxAge.Get("age", 27);
            Assert.AreEqual(1, vertices.Count());
            vertices.Dispose();

            IIndex idxWeight = _graph.GetIndex("weight", typeof(IEdge));
            ICloseableIterable<IElement> edges = idxWeight.Get("weight", 0.5f);
            Assert.AreEqual(1, edges.Count());
            edges.Dispose();
        }

        [Test]
        public void ExampleMetadataGetsCorrectVertexKeyIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_graph, stream);
            }
            
            Assert.AreEqual(1, _graph.VertexKeyIndex.Index.Count());
            Assert.AreEqual(1, _graph.GetVertices("age", 27).Count());
        }

        [Test]
        public void ExampleMetadataGetsCorrectEdgeKeyIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.Load(_graph, stream);
            }
            
            Assert.AreEqual(1, _graph.EdgeKeyIndex.Index.Count());
            Assert.AreEqual(1, _graph.GetEdges("weight", 0.5f).Count());
        }
    }
}
