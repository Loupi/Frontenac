using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GML
{
    [TestFixture(Category = "GMLReaderTest")]
    public class GmlReaderTest : BaseTest
    {
        private const string Label = "label";

        [Test]
        public void Example2GmlTestingMapParsing()
        {
            var graph = new TinkerGraph();

            using (var stream = GetResource<GmlReaderTest>("example2.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            var property = graph.GetVertex(1).GetProperty(GmlTokens.Graphics);
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

        [Test]
        public void ExampleGmlGetsCorrectNumberOfElements()
        {
            var graph = new TinkerGraph();
            using (var stream = GetResource<GmlReaderTest>("example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            Assert.AreEqual(3, graph.GetVertices().Count());
            Assert.AreEqual(3, graph.GetEdges().Count());
        }

        [Test]
        public void ExampleGmlGetsCorrectProperties()
        {
            var graph = new TinkerGraph();
            using (var stream = GetResource<GmlReaderTest>("example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            var v1 = graph.GetVertex(1);
            Assert.AreEqual("Node 1", v1.GetProperty(Label));

            var v2 = graph.GetVertex(2);
            Assert.AreEqual("Node 2", v2.GetProperty(Label));

            var v3 = graph.GetVertex(3);
            Assert.AreEqual("Node 3", v3.GetProperty(Label));

            var out1 = v1.GetEdges(Direction.Out);
            var e1 = out1.First();
            Assert.AreEqual("Edge from node 1 to node 2", e1.Label);

            var out2 = v2.GetEdges(Direction.Out);
            var e2 = out2.First();
            Assert.AreEqual("Edge from node 2 to node 3", e2.Label);

            var out3 = v3.GetEdges(Direction.Out);
            var e3 = out3.First();
            Assert.AreEqual("Edge from node 3 to node 1", e3.Label);
        }

        [Test]
        public void ExampleGmlGetsCorrectTopology()
        {
            var graph = new TinkerGraph();
            using (var stream = GetResource<GmlReaderTest>("example.gml"))
            {
                GmlReader.InputGraph(graph, stream);
            }

            var v1 = graph.GetVertex(1);
            var v2 = graph.GetVertex(2);
            var v3 = graph.GetVertex(3);

            var out1 = v1.GetEdges(Direction.Out);
            var e1 = out1.First();
            Assert.AreEqual(v2, e1.GetVertex(Direction.In));

            var out2 = v2.GetEdges(Direction.Out);
            var e2 = out2.First();
            Assert.AreEqual(v3, e2.GetVertex(Direction.In));

            var out3 = v3.GetEdges(Direction.Out);
            var e3 = out3.First();
            Assert.AreEqual(v1, e3.GetVertex(Direction.In));
        }

        [Test]
        public void MalformedThrowsIoException()
        {
            try
            {
                using (var stream = GetResource<GmlReaderTest>("malformed.gml"))
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
    }
}