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
        public void exampleGmlGetsCorrectNumberOfElements()
        {
            TinkerGraph graph = new TinkerGraph();
            
            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.inputGraph(graph, stream);
            }

            Assert.AreEqual(3, graph.getVertices().Count());
            Assert.AreEqual(3, graph.getEdges().Count());
        }

        [Test]
        public void exampleGmlGetsCorrectTopology()
        {
            TinkerGraph graph = new TinkerGraph();

            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.inputGraph(graph, stream);
            }

            Vertex v1 = graph.getVertex(1);
            Vertex v2 = graph.getVertex(2);
            Vertex v3 = graph.getVertex(3);

            IEnumerable<Edge> out1 = v1.getEdges(Direction.OUT);
            Edge e1 = out1.First();
            Assert.AreEqual(v2, e1.getVertex(Direction.IN));

            IEnumerable<Edge> out2 = v2.getEdges(Direction.OUT);
            Edge e2 = out2.First();
            Assert.AreEqual(v3, e2.getVertex(Direction.IN));

            IEnumerable<Edge> out3 = v3.getEdges(Direction.OUT);
            Edge e3 = out3.First();
            Assert.AreEqual(v1, e3.getVertex(Direction.IN));
        }

        [Test]
        public void exampleGmlGetsCorrectProperties(){
            TinkerGraph graph = new TinkerGraph();

            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.inputGraph(graph, stream);
            }

            Vertex v1 = graph.getVertex(1);
            Assert.AreEqual("Node 1", v1.getProperty(LABEL));

            Vertex v2 = graph.getVertex(2);
            Assert.AreEqual("Node 2", v2.getProperty(LABEL));

            Vertex v3 = graph.getVertex(3);
            Assert.AreEqual("Node 3", v3.getProperty(LABEL));

            IEnumerable<Edge> out1 = v1.getEdges(Direction.OUT);
            Edge e1 = out1.First();
            Assert.AreEqual("Edge from node 1 to node 2", e1.getLabel());

            IEnumerable<Edge> out2 = v2.getEdges(Direction.OUT);
            Edge e2 = out2.First();
            Assert.AreEqual("Edge from node 2 to node 3", e2.getLabel());

            IEnumerable<Edge> out3 = v3.getEdges(Direction.OUT);
            Edge e3 = out3.First();
            Assert.AreEqual("Edge from node 3 to node 1", e3.getLabel());
        }

        [Test]
        public void malformedThrowsIoException()
        {
            try
            {
                using (var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "malformed.gml"))
                {
                    GMLReader.inputGraph(new TinkerGraph(), stream);
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
        public void example2GmlTestingMapParsing()
        {
            TinkerGraph graph = new TinkerGraph();

            using (var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example2.gml"))
            {
                GMLReader.inputGraph(graph, stream);
            }

            Assert.AreEqual(2, graph.getVertices().Count());
            Assert.AreEqual(1, graph.getEdges().Count());

            object property = graph.getVertex(1).getProperty(GMLTokens.GRAPHICS);
            Assert.True(property is IDictionary);

            Dictionary<string, object> map = (Dictionary<string, object>) property;
            Assert.AreEqual(5, map.Count);
            Assert.AreEqual(0.1f, map.get("x"));
            // NB comes back as int
            Assert.AreEqual(0, map.get("y"));
            Assert.AreEqual(0.1f, map.get("w"));
            Assert.AreEqual(0.1f, map.get("h"));
            Assert.AreEqual("earth.gif", map.get("bitmap"));
        }


    }
}
