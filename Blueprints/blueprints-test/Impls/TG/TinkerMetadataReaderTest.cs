using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataReaderTest")]
    public class TinkerMetadataReaderTest
    {
        TinkerGraph graph;

        [SetUp]
        public void beforeTest()
        {
            this.graph = TinkerGraphFactory.createTinkerGraph();
        }

        [Test]
        public void exampleMetadataGetsCorrectCurrentId()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.load(this.graph, stream);
            }

            Assert.AreEqual(this.graph.currentId, 0);
        }

        [Test]
        public void exampleMetadataGetsCorrectIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.load(this.graph, stream);
            }
            
            Assert.AreEqual(2, this.graph.indices.Count());

            Index idxAge = this.graph.getIndex("age", typeof(Vertex));
            CloseableIterable<Element> vertices = idxAge.get("age", 27);
            Assert.AreEqual(1, vertices.Count());
            vertices.Dispose();

            Index idxWeight = this.graph.getIndex("weight", typeof(Edge));
            CloseableIterable<Element> edges = idxWeight.get("weight", 0.5f);
            Assert.AreEqual(1, edges.Count());
            edges.Dispose();
        }

        [Test]
        public void exampleMetadataGetsCorrectVertexKeyIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.load(this.graph, stream);
            }
            
            Assert.AreEqual(1, this.graph.vertexKeyIndex.index.Count());
            Assert.AreEqual(1, graph.getVertices("age", 27).Count());
        }

        [Test]
        public void exampleMetadataGetsCorrectEdgeKeyIndices()
        {
            using (var stream = typeof(TinkerMetadataReaderTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataReaderTest), "example-tinkergraph-metadata.dat"))
            {
                TinkerMetadataReader.load(this.graph, stream);
            }
            
            Assert.AreEqual(1, this.graph.edgeKeyIndex.index.Count());
            Assert.AreEqual(1, graph.getEdges("weight", 0.5f).Count());
        }
    }
}
