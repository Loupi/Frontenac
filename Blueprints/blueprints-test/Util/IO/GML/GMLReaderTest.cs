using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.IO.GML
{
    [TestFixture(Category = "GMLReaderTest")]
    public class GMLReaderTest
    {
        const string LABEL = "label";

        [Test]
        public void ExampleGMLGetsCorrectNumberOfElements()
        {
            TinkerGraph graph = new TinkerGraph();
            
            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(3, graph.GetVertices().Count());
            Assert.AreEqual(3, graph.GetEdges().Count());
        }

        [Test]
        public void ExampleGMLGetsCorrectTopology()
        {
            TinkerGraph graph = new TinkerGraph();

            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.InputGraph(graph, stream);
            }

            Vertex v1 = graph.GetVertex(1);
            Vertex v2 = graph.GetVertex(2);
            Vertex v3 = graph.GetVertex(3);

            IEnumerable<Edge> out1 = v1.GetEdges(Direction.OUT);
            Edge e1 = out1.First();
            Assert.AreEqual(v2, e1.GetVertex(Direction.IN));

            IEnumerable<Edge> out2 = v2.GetEdges(Direction.OUT);
            Edge e2 = out2.First();
            Assert.AreEqual(v3, e2.GetVertex(Direction.IN));

            IEnumerable<Edge> out3 = v3.GetEdges(Direction.OUT);
            Edge e3 = out3.First();
            Assert.AreEqual(v1, e3.GetVertex(Direction.IN));
        }

        [Test]
        public void ExampleGMLGetsCorrectProperties(){
            TinkerGraph graph = new TinkerGraph();

            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.InputGraph(graph, stream);
            }

            Vertex v1 = graph.GetVertex(1);
            Assert.AreEqual("Node 1", v1.GetProperty(LABEL));

            Vertex v2 = graph.GetVertex(2);
            Assert.AreEqual("Node 2", v2.GetProperty(LABEL));

            Vertex v3 = graph.GetVertex(3);
            Assert.AreEqual("Node 3", v3.GetProperty(LABEL));

            IEnumerable<Edge> out1 = v1.GetEdges(Direction.OUT);
            Edge e1 = out1.First();
            Assert.AreEqual("Edge from node 1 to node 2", e1.GetLabel());

            IEnumerable<Edge> out2 = v2.GetEdges(Direction.OUT);
            Edge e2 = out2.First();
            Assert.AreEqual("Edge from node 2 to node 3", e2.GetLabel());

            IEnumerable<Edge> out3 = v3.GetEdges(Direction.OUT);
            Edge e3 = out3.First();
            Assert.AreEqual("Edge from node 3 to node 1", e3.GetLabel());
        }

        [Test]
        public void MalformedThrowsIOException()
        {
            try
            {
                using (var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "malformed.gml"))
                {
                    GMLReader.InputGraph(new TinkerGraph(), stream);
                }
                Assert.Fail();
            }
            catch (IOException)
            {
                Assert.Pass();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Example2GMLTestingMapParsing()
        {
            TinkerGraph graph = new TinkerGraph();

            using (var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example2.gml"))
            {
                GMLReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            object property = graph.GetVertex(1).GetProperty(GMLTokens.GRAPHICS);
            Assert.True(property is IDictionary);

            Dictionary<string, object> map = (Dictionary<string, object>) property;
            Assert.AreEqual(5, map.Count);
            Assert.AreEqual(0.1f, map.Get("x"));
            // NB comes back as int
            Assert.AreEqual(0, map.Get("y"));
            Assert.AreEqual(0.1f, map.Get("w"));
            Assert.AreEqual(0.1f, map.Get("h"));
            Assert.AreEqual("earth.gif", map.Get("bitmap"));
        }


    }
}
