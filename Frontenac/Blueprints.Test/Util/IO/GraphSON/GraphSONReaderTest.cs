using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONReaderTest")]
    public class GraphSonReaderTest
    {
        [Test]
        public void InputGraphCompactFullCycle()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var emptyGraph = new TinkerGraph();

            var edgeKeys = new HashSet<string>
                {
                    GraphSonTokens.Id,
                    GraphSonTokens.InV,
                    GraphSonTokens.OutV,
                    GraphSonTokens.Label
                };

            var vertexKeys = new HashSet<string> {GraphSonTokens.Id};

            using (var stream = new MemoryStream())
            {
                var writer = new GraphSonWriter(graph);
                writer.OutputGraph(stream, vertexKeys, edgeKeys, GraphSonMode.COMPACT);

                stream.Position = 0;

                GraphSonReader.InputGraph(emptyGraph, stream);
            }

            Assert.AreEqual(7, emptyGraph.GetVertices().Count());
            Assert.AreEqual(6, emptyGraph.GetEdges().Count());

            foreach (var v in graph.GetVertices())
            {
                var found = emptyGraph.GetVertex(v.Id);

                Assert.NotNull(v);

                foreach (var key in found.GetPropertyKeys())
                {
                    Assert.AreEqual(v.GetProperty(key), found.GetProperty(key));
                }

                // no properties should be here
                Assert.AreEqual(null, found.GetProperty("name"));
            }

            foreach (var e in graph.GetEdges())
            {
                var found = emptyGraph.GetEdge(e.Id);

                Assert.NotNull(e);

                foreach (var key in found.GetPropertyKeys())
                {
                    Assert.AreEqual(e.GetProperty(key), found.GetProperty(key));
                }

                // no properties should be here
                Assert.AreEqual(null, found.GetProperty("weight"));
            }
        }

        [Test]
        public void InputGraphCompactFullCycleBroken()
        {
            try
            {
                var graph = TinkerGraphFactory.CreateTinkerGraph();
                var emptyGraph = new TinkerGraph();
                var edgeKeys = new HashSet<string> {GraphSonTokens.InV, GraphSonTokens.OutV, GraphSonTokens.Label};
                var vertexKeys = new HashSet<string> {"init"};
                vertexKeys.Remove("init");

                using (var stream = new MemoryStream())
                {
                    var writer = new GraphSonWriter(graph);
                    writer.OutputGraph(stream, vertexKeys, edgeKeys, GraphSonMode.COMPACT);

                    stream.Position = 0;

                    GraphSonReader.InputGraph(emptyGraph, stream);
                }

                Assert.Fail();
            }
            catch (Exception)
            {
                
            }
        }

        [Test]
        public void InputGraphExtendedFullCycle()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var emptyGraph = new TinkerGraph();

            using (var stream = new MemoryStream())
            {
                var writer = new GraphSonWriter(graph);
                writer.OutputGraph(stream, null, null, GraphSonMode.EXTENDED);

                stream.Position = 0;

                GraphSonReader.InputGraph(emptyGraph, stream);
            }

            Assert.AreEqual(7, emptyGraph.GetVertices().Count());
            Assert.AreEqual(6, emptyGraph.GetEdges().Count());

            foreach (var v in graph.GetVertices())
            {
                var found = emptyGraph.GetVertex(v.Id);

                Assert.NotNull(v);

                foreach (var key in found.GetPropertyKeys())
                {
                    Assert.AreEqual(v.GetProperty(key), found.GetProperty(key));
                }
            }

            foreach (var e in graph.GetEdges())
            {
                var found = emptyGraph.GetEdge(e.Id);

                Assert.NotNull(e);

                foreach (var key in found.GetPropertyKeys())
                {
                    Assert.AreEqual(e.GetProperty(key), found.GetProperty(key));
                }
            }
        }

        [Test]
        public void InputGraphModeCompact()
        {
            var graph = new TinkerGraph();

            const string json =
                "{ \"mode\":\"COMPACT\",\"vertices\": [ {\"_id\":1, \"test\": \"please work\", \"testlist\":[1, 2, 3, null], \"testmap\":{\"big\":10000000000, \"small\":0.4954959595959}}, {\"_id\":2, \"testagain\":\"please work again\"}], \"edges\":[{\"_id\":100, \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": \"please worke\"}]}";

            var bytes = Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSonReader.InputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            var v1 = graph.GetVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.GetProperty("test"));

            var map = (IDictionary<string, object>) v1.GetProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.Get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.Get("small").ToString()), 0);

            var list = (IList<object>) v1.GetProperty("testlist");
            Assert.AreEqual(4, list.Count);

            var foundNull = false;
            for (var ix = 0; ix < list.Count; ix++)
            {
                if (list.Get(ix) == null)
                {
                    foundNull = true;
                    break;
                }
            }

            Assert.True(foundNull);

            var v2 = graph.GetVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.GetProperty("testagain"));

            var e = graph.GetEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
            Assert.AreEqual("please worke", e.GetProperty("teste"));
            Assert.Null(e.GetProperty("keyNull"));
        }

        [Test]
        public void InputGraphModeExtended()
        {
            var graph = new TinkerGraph();

            const string json =
                "{ \"mode\":\"EXTENDED\", \"vertices\": [ {\"_id\":1, \"_type\":\"vertex\", \"test\": { \"type\":\"string\", \"value\":\"please work\"}, \"testlist\":{\"type\":\"list\", \"value\":[{\"type\":\"int\", \"value\":1}, {\"type\":\"int\",\"value\":2}, {\"type\":\"int\",\"value\":3}]}, \"testmap\":{\"type\":\"map\", \"value\":{\"big\":{\"type\":\"long\", \"value\":10000000000}, \"small\":{\"type\":\"double\", \"value\":0.4954959595959}}}}, {\"_id\":2, \"_type\":\"vertex\", \"testagain\":{\"type\":\"string\", \"value\":\"please work again\"}}], \"edges\":[{\"_id\":100, \"_type\":\"edge\", \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": {\"type\":\"string\", \"value\":\"please worke\"}}]}";

            var bytes = Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSonReader.InputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            var v1 = graph.GetVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.GetProperty("test"));

            var map = (IDictionary<string, object>) v1.GetProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.Get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.Get("small").ToString()), 0);

            var list = (IList<object>) v1.GetProperty("testlist");
            Assert.AreEqual(3, list.Count);

            var v2 = graph.GetVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.GetProperty("testagain"));

            var e = graph.GetEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
            Assert.AreEqual("please worke", e.GetProperty("teste"));
        }

        [Test]
        public void InputGraphModeNormal()
        {
            var graph = new TinkerGraph();

            const string json =
                "{ \"mode\":\"NORMAL\",\"vertices\": [ {\"_id\":1, \"_type\":\"vertex\", \"test\": \"please work\", \"testlist\":[1, 2, 3, null], \"testmap\":{\"big\":10000000000, \"small\":0.4954959595959}}, {\"_id\":2, \"_type\":\"vertex\", \"testagain\":\"please work again\"}], \"edges\":[{\"_id\":100, \"_type\":\"edge\", \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": \"please worke\"}]}";

            var bytes = Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSonReader.InputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.GetVertices().Count());
            Assert.AreEqual(1, graph.GetEdges().Count());

            var v1 = graph.GetVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.GetProperty("test"));

            var map = (IDictionary<string, object>) v1.GetProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.Get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.Get("small").ToString()), 0);
            Assert.Null(map.Get("nullKey"));

            var list = (IList<object>) v1.GetProperty("testlist");
            Assert.AreEqual(4, list.Count);

            var foundNull = false;
            for (var ix = 0; ix < list.Count; ix++)
            {
                if (list.Get(ix) == null)
                {
                    foundNull = true;
                    break;
                }
            }

            Assert.True(foundNull);

            var v2 = graph.GetVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.GetProperty("testagain"));

            var e = graph.GetEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
            Assert.AreEqual("please worke", e.GetProperty("teste"));
            Assert.Null(e.GetProperty("keyNull"));
        }
    }
}