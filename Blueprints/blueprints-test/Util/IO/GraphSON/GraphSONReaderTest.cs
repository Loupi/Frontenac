using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Frontenac.Blueprints.Impls.TG;
using System.Collections;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONReaderTest")]
    public class GraphSONReaderTest
    {
        [Test]
        public void inputGraphModeExtended()
        {
            TinkerGraph graph = new TinkerGraph();

            string json = "{ \"mode\":\"EXTENDED\", \"vertices\": [ {\"_id\":1, \"_type\":\"vertex\", \"test\": { \"type\":\"string\", \"value\":\"please work\"}, \"testlist\":{\"type\":\"list\", \"value\":[{\"type\":\"int\", \"value\":1}, {\"type\":\"int\",\"value\":2}, {\"type\":\"int\",\"value\":3}]}, \"testmap\":{\"type\":\"map\", \"value\":{\"big\":{\"type\":\"long\", \"value\":10000000000}, \"small\":{\"type\":\"double\", \"value\":0.4954959595959}}}}, {\"_id\":2, \"_type\":\"vertex\", \"testagain\":{\"type\":\"string\", \"value\":\"please work again\"}}], \"edges\":[{\"_id\":100, \"_type\":\"edge\", \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": {\"type\":\"string\", \"value\":\"please worke\"}}]}";

            byte[] bytes = System.Text.Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSONReader.inputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.getVertices().Count());
            Assert.AreEqual(1, graph.getEdges().Count());

            Vertex v1 = graph.getVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.getProperty("test"));

            var map = (IDictionary<string, object>)v1.getProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.get("small").ToString()), 0);
            //Assert.assertNull(map.get("nullKey"));

            IList<object> list = (IList<object>)v1.getProperty("testlist");
            Assert.AreEqual(3, list.Count);

            //Porting Note: looking at the JSON input, there is no null value in that list. Just disable this test for now.
            /*bool foundNull = false;
            for (int ix = 0; ix < list.Count; ix++)
            {
                if (list.get(ix) == null)
                {
                    foundNull = true;
                    break;
                }
            }

            Assert.True(foundNull);*/

            Vertex v2 = graph.getVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.getProperty("testagain"));

            Edge e = graph.getEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
            Assert.AreEqual("please worke", e.getProperty("teste"));
            //Assert.assertNull(e.getProperty("keyNull"));
        }

        [Test]
        public void inputGraphModeNormal()
        {
            TinkerGraph graph = new TinkerGraph();

            string json = "{ \"mode\":\"NORMAL\",\"vertices\": [ {\"_id\":1, \"_type\":\"vertex\", \"test\": \"please work\", \"testlist\":[1, 2, 3, null], \"testmap\":{\"big\":10000000000, \"small\":0.4954959595959}}, {\"_id\":2, \"_type\":\"vertex\", \"testagain\":\"please work again\"}], \"edges\":[{\"_id\":100, \"_type\":\"edge\", \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": \"please worke\"}]}";

            byte[] bytes = System.Text.Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSONReader.inputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.getVertices().Count());
            Assert.AreEqual(1, graph.getEdges().Count());

            Vertex v1 = graph.getVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.getProperty("test"));

            var map = (IDictionary<string, object>)v1.getProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.get("small").ToString()), 0);
            Assert.Null(map.get("nullKey"));

            var list = (IList<object>)v1.getProperty("testlist");
            Assert.AreEqual(4, list.Count);

            bool foundNull = false;
            for (int ix = 0; ix < list.Count; ix++)
            {
                if (list.get(ix) == null)
                {
                    foundNull = true;
                    break;
                }
            }

            Assert.True(foundNull);

            Vertex v2 = graph.getVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.getProperty("testagain"));

            Edge e = graph.getEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
            Assert.AreEqual("please worke", e.getProperty("teste"));
            Assert.Null(e.getProperty("keyNull"));
        }

        [Test]
        public void inputGraphModeCompact()
        {
            TinkerGraph graph = new TinkerGraph();

            String json = "{ \"mode\":\"COMPACT\",\"vertices\": [ {\"_id\":1, \"test\": \"please work\", \"testlist\":[1, 2, 3, null], \"testmap\":{\"big\":10000000000, \"small\":0.4954959595959}}, {\"_id\":2, \"testagain\":\"please work again\"}], \"edges\":[{\"_id\":100, \"_outV\":1, \"_inV\":2, \"_label\":\"works\", \"teste\": \"please worke\"}]}";

            byte[] bytes = System.Text.Encoding.Default.GetBytes(json);
            using (var inputStream = new MemoryStream(bytes))
            {
                GraphSONReader.inputGraph(graph, inputStream);
            }

            Assert.AreEqual(2, graph.getVertices().Count());
            Assert.AreEqual(1, graph.getEdges().Count());

            Vertex v1 = graph.getVertex(1);
            Assert.NotNull(v1);
            Assert.AreEqual("please work", v1.getProperty("test"));

            var map = (IDictionary<string, object>)v1.getProperty("testmap");
            Assert.NotNull(map);
            Assert.AreEqual(10000000000, long.Parse(map.get("big").ToString()));
            Assert.AreEqual(0.4954959595959, double.Parse(map.get("small").ToString()), 0);
            // Assert.assertNull(map.get("nullKey"));

            var list = (IList<object>)v1.getProperty("testlist");
            Assert.AreEqual(4, list.Count);

            bool foundNull = false;
            for (int ix = 0; ix < list.Count; ix++)
            {
                if (list.get(ix) == null)
                {
                    foundNull = true;
                    break;
                }
            }

            Assert.True(foundNull);

            Vertex v2 = graph.getVertex(2);
            Assert.NotNull(v2);
            Assert.AreEqual("please work again", v2.getProperty("testagain"));

            Edge e = graph.getEdge(100);
            Assert.NotNull(e);
            Assert.AreEqual("works", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
            Assert.AreEqual("please worke", e.getProperty("teste"));
            Assert.Null(e.getProperty("keyNull"));

        }

        [Test]
        public void inputGraphExtendedFullCycle()
        {
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            TinkerGraph emptyGraph = new TinkerGraph();

            using (var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(graph);
                writer.outputGraph(stream, null, null, GraphSONMode.EXTENDED);

                stream.Position = 0;

                GraphSONReader.inputGraph(emptyGraph, stream);
            }

            Assert.AreEqual(7, emptyGraph.getVertices().Count());
            Assert.AreEqual(6, emptyGraph.getEdges().Count());

            foreach (Vertex v in graph.getVertices())
            {
                Vertex found = emptyGraph.getVertex(v.getId());

                Assert.NotNull(v);

                foreach (string key in found.getPropertyKeys())
                {
                    Assert.AreEqual(v.getProperty(key), found.getProperty(key));
                }
            }

            foreach (Edge e in graph.getEdges())
            {
                Edge found = emptyGraph.getEdge(e.getId());

                Assert.NotNull(e);

                foreach (string key in found.getPropertyKeys())
                {
                    Assert.AreEqual(e.getProperty(key), found.getProperty(key));
                }
            }
        }

        [Test]
        public void inputGraphCompactFullCycle()
        {
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            TinkerGraph emptyGraph = new TinkerGraph();

            HashSet<string> edgeKeys = new HashSet<string>();
            edgeKeys.Add(GraphSONTokens._ID);
            edgeKeys.Add(GraphSONTokens._IN_V);
            edgeKeys.Add(GraphSONTokens._OUT_V);
            edgeKeys.Add(GraphSONTokens._LABEL);

            HashSet<string> vertexKeys = new HashSet<string>();
            vertexKeys.Add(GraphSONTokens._ID);

            using (var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(graph);
                writer.outputGraph(stream, vertexKeys, edgeKeys, GraphSONMode.COMPACT);

                stream.Position = 0;

                GraphSONReader.inputGraph(emptyGraph, stream);
            }

            Assert.AreEqual(7, emptyGraph.getVertices().Count());
            Assert.AreEqual(6, emptyGraph.getEdges().Count());

            foreach (Vertex v in graph.getVertices())
            {
                Vertex found = emptyGraph.getVertex(v.getId());

                Assert.NotNull(v);

                foreach (string key in found.getPropertyKeys())
                {
                    Assert.AreEqual(v.getProperty(key), found.getProperty(key));
                }

                // no properties should be here
                Assert.AreEqual(null, found.getProperty("name"));
            }

            foreach (Edge e in graph.getEdges())
            {
                Edge found = emptyGraph.getEdge(e.getId());

                Assert.NotNull(e);

                foreach (string key in found.getPropertyKeys())
                {
                    Assert.AreEqual(e.getProperty(key), found.getProperty(key));
                }

                // no properties should be here
                Assert.AreEqual(null, found.getProperty("weight"));
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void inputGraphCompactFullCycleBroken()
        {
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            TinkerGraph emptyGraph = new TinkerGraph();

            HashSet<string> edgeKeys = new HashSet<string>();
            edgeKeys.Add(GraphSONTokens._IN_V);
            edgeKeys.Add(GraphSONTokens._OUT_V);
            edgeKeys.Add(GraphSONTokens._LABEL);

            HashSet<string> vertexKeys = new HashSet<string>();

            using (var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(graph);
                writer.outputGraph(stream, vertexKeys, edgeKeys, GraphSONMode.COMPACT);

                stream.Position = 0;

                GraphSONReader.inputGraph(emptyGraph, stream);
            }
        }
    }
}
