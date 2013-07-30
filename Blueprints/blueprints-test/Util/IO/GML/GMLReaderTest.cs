using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.IO.GML
{
    [TestFixture(Category = "GMLReaderTest")]
    public class GmlReaderTest
    {
        const string Label = "label";

        [Test]
        public void ExampleGmlGetsCorrectNumberOfElements()
        {
            var graph = new TinkerGraph();
            
            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(3, graph.GetVertices().Count());
            Assert.AreEqual(3, graph.GetEdges().Count());
        }

        [Test]
        public void ExampleGmlGetsCorrectTopology()
        {
            var graph = new TinkerGraph();

            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            IVertex v1 = graph.GetVertex(1);
            IVertex v2 = graph.GetVertex(2);
            IVertex v3 = graph.GetVertex(3);

            IEnumerable<IEdge> out1 = v1.GetEdges(Direction.Out);
            IEdge e1 = out1.First();
            Assert.AreEqual(v2, e1.GetVertex(Direction.In));

            IEnumerable<IEdge> out2 = v2.GetEdges(Direction.Out);
            IEdge e2 = out2.First();
            Assert.AreEqual(v3, e2.GetVertex(Direction.In));

            IEnumerable<IEdge> out3 = v3.GetEdges(Direction.Out);
            IEdge e3 = out3.First();
            Assert.AreEqual(v1, e3.GetVertex(Direction.In));
        }

        [Test]
        public void ExampleGmlGetsCorrectProperties(){
            var graph = new TinkerGraph();

            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            IVertex v1 = graph.GetVertex(1);
            Assert.AreEqual("Node 1", v1.GetProperty(Label));

            IVertex v2 = graph.GetVertex(2);
            Assert.AreEqual("Node 2", v2.GetProperty(Label));

            IVertex v3 = graph.GetVertex(3);
            Assert.AreEqual("Node 3", v3.GetProperty(Label));

            IEnumerable<IEdge> out1 = v1.GetEdges(Direction.Out);
            IEdge e1 = out1.First();
            Assert.AreEqual("Edge from node 1 to node 2", e1.GetLabel());

            IEnumerable<IEdge> out2 = v2.GetEdges(Direction.Out);
            IEdge e2 = out2.First();
            Assert.AreEqual("Edge from node 2 to node 3", e2.GetLabel());

            IEnumerable<IEdge> out3 = v3.GetEdges(Direction.Out);
            IEdge e3 = out3.First();
            Assert.AreEqual("Edge from node 3 to node 1", e3.GetLabel());
        }

        [Test]
        public void MalformedThrowsIoException()
        {
            try
            {
                using (var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "malformed.gml"))
                {
                    GmlReader.InputGraph(new TinkerGraph(), stream);
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
        public void Example2GmlTestingMapParsing()
        {
            var graph = new TinkerGraph();

            using (var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example2.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            object property = graph.GetVertex(1).GetProperty(GmlTokens.Graphics);
            Assert.True(property is IDictionary);

            var map = (Dictionary<string, object>) property;
            Assert.AreEqual(5, map.Count);
            Assert.AreEqual(0.1, map.Get("x"));
            // NB comes back as int
            Assert.AreEqual(0, map.Get("y"));
            Assert.AreEqual(0.1, map.Get("w"));
            Assert.AreEqual(0.1, map.Get("h"));
            Assert.AreEqual("earth.gif", map.Get("bitmap"));
        }


    }
}
