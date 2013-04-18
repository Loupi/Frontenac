using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util.IO;

namespace Frontenac.Blueprints
{
    public abstract class GraphTestSuite : TestSuite
    {
        public GraphTestSuite(GraphTest graphTest)
            : base("GraphTestSuite", graphTest)
        {

        }

        [Test]
        public void testFeatureCompliance()
        {
            Graph graph = graphTest.generateGraph();
            graph.getFeatures().checkCompliance();
            Console.WriteLine(graph.getFeatures());
            graph.shutdown();
        }

        [Test]
        public void testEmptyOnConstruction()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));
            graph.shutdown();
        }

        [Test]
        public void testStringRepresentation()
        {
            Graph graph = graphTest.generateGraph();
            try
            {
                this.stopWatch();
                Assert.NotNull(graph.ToString());
                Assert.True(graph.ToString().StartsWith(graph.GetType().Name.ToLower()));
                printPerformance(graph.ToString(), 1, "graph string representation generated", this.stopWatch());
            }
            catch (Exception)
            {
                Assert.False(true);
            }
            graph.shutdown();
        }

        [Test]
        public void testStringRepresentationOfVertexId()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            object id = a.getId();
            Vertex b = graph.getVertex(id);
            Vertex c = graph.getVertex(id.ToString());
            Assert.AreEqual(a, b);
            Assert.AreEqual(b, c);
            Assert.AreEqual(c, a);
            graph.shutdown();
        }

        [Test]
        public void testSemanticallyCorrectIterables()
        {
            Graph graph = graphTest.generateGraph();
            for (int i = 0; i < 15; i++)
            {
                graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));
            }
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                IEnumerable<Vertex> vertices = graph.getVertices();
                Assert.AreEqual(count(vertices), 30);
                Assert.AreEqual(count(vertices), 30);
                int counter = vertices.Count();
                Assert.AreEqual(counter, 30);
            }
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                IEnumerable<Edge> edges = graph.getEdges();
                Assert.AreEqual(count(edges), 15);
                Assert.AreEqual(count(edges), 15);
                int counter = edges.Count();
                Assert.AreEqual(counter, 15);
            }

            graph.shutdown();
        }

        [Test]
        public void testGettingVerticesAndEdgesWithKeyValue()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v1 = graph.addVertex(null);
                v1.setProperty("name", "marko");
                v1.setProperty("location", "everywhere");
                Vertex v2 = graph.addVertex(null);
                v2.setProperty("name", "stephen");
                v2.setProperty("location", "everywhere");

                if (graph.getFeatures().supportsVertexIteration.Value)
                {
                    Assert.AreEqual(count(graph.getVertices("location", "everywhere")), 2);
                    Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
                    Assert.AreEqual(count(graph.getVertices("name", "stephen")), 1);
                    Assert.AreEqual(getOnlyElement(graph.getVertices("name", "marko")), v1);
                    Assert.AreEqual(getOnlyElement(graph.getVertices("name", "stephen")), v2);
                }
            }

            if (graph.getFeatures().supportsEdgeProperties.Value)
            {
                Edge e1 = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));
                e1.setProperty("name", "marko");
                e1.setProperty("location", "everywhere");
                Edge e2 = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));
                e2.setProperty("name", "stephen");
                e2.setProperty("location", "everywhere");

                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(count(graph.getEdges("location", "everywhere")), 2);
                    Assert.AreEqual(count(graph.getEdges("name", "marko")), 1);
                    Assert.AreEqual(count(graph.getEdges("name", "stephen")), 1);
                    Assert.AreEqual(graph.getEdges("name", "marko").First(), e1);
                    Assert.AreEqual(graph.getEdges("name", "stephen").First(), e2);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testAddingVerticesAndEdges()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Edge edge = graph.addEdge(null, a, b, convertId(graph, "knows"));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(1, count(graph.getEdges()));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(2, count(graph.getVertices()));

            graph.removeVertex(a);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(1, count(graph.getVertices()));

            try
            {
                graph.removeEdge(edge);
                //TODO: doesn't work with wrapper graphs Assert.True(false);
            }
            catch (Exception)
            {
                Assert.True(true);
            }

            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(1, count(graph.getVertices()));

            graph.shutdown();
        }

        [Test]
        public void testSettingProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeProperties.Value)
            {
                Vertex a = graph.addVertex(null);
                Vertex b = graph.addVertex(null);
                graph.addEdge(null, a, b, convertId(graph, "knows"));
                graph.addEdge(null, a, b, convertId(graph, "knows"));
                foreach (Edge edge in b.getEdges(Direction.IN))
                    edge.setProperty("key", "value");
            }
            graph.shutdown();
        }

        [Test]
        public void testDataTypeValidationOnProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsElementProperties() && !graph.getFeatures().isWrapper.Value)
            {
                Vertex vertexA = graph.addVertex(null);
                Vertex vertexB = graph.addVertex(null);
                Edge edge = graph.addEdge(null, vertexA, vertexB, convertId(graph, "knows"));

                trySetProperty(vertexA, "keyString", "value", graph.getFeatures().supportsStringProperty.Value);
                trySetProperty(edge, "keyString", "value", graph.getFeatures().supportsStringProperty.Value);

                trySetProperty(vertexA, "keyInteger", 100, graph.getFeatures().supportsIntegerProperty.Value);
                trySetProperty(edge, "keyInteger", 100, graph.getFeatures().supportsIntegerProperty.Value);

                trySetProperty(vertexA, "keyLong", 10000L, graph.getFeatures().supportsLongProperty.Value);
                trySetProperty(edge, "keyLong", 10000L, graph.getFeatures().supportsLongProperty.Value);

                trySetProperty(vertexA, "keyDouble", 100.321d, graph.getFeatures().supportsDoubleProperty.Value);
                trySetProperty(edge, "keyDouble", 100.321d, graph.getFeatures().supportsDoubleProperty.Value);

                trySetProperty(vertexA, "keyFloat", 100.321f, graph.getFeatures().supportsFloatProperty.Value);
                trySetProperty(edge, "keyFloat", 100.321f, graph.getFeatures().supportsFloatProperty.Value);

                trySetProperty(vertexA, "keyBoolean", true, graph.getFeatures().supportsBooleanProperty.Value);
                trySetProperty(edge, "keyBoolean", true, graph.getFeatures().supportsBooleanProperty.Value);

                trySetProperty(vertexA, "keyDate", new DateTime(), graph.getFeatures().supportsSerializableObjectProperty.Value);
                trySetProperty(edge, "keyDate", new DateTime(), graph.getFeatures().supportsSerializableObjectProperty.Value);

                List<string> listA = new List<string>();
                listA.Add("try1");
                listA.Add("try2");

                trySetProperty(vertexA, "keyListString", listA, graph.getFeatures().supportsUniformListProperty.Value);
                trySetProperty(edge, "keyListString", listA, graph.getFeatures().supportsUniformListProperty.Value);

                List<object> listB = new List<object>();
                listB.Add("try1");
                listB.Add(2);

                trySetProperty(vertexA, "keyListMixed", listB, graph.getFeatures().supportsMixedListProperty.Value);
                trySetProperty(edge, "keyListMixed", listB, graph.getFeatures().supportsMixedListProperty.Value);

                trySetProperty(vertexA, "keyArrayString", new string[] { "try1", "try2" }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayString", new string[] { "try1", "try2" }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayInteger", new int[] { 1, 2 }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayInteger", new int[] { 1, 2 }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayLong", new long[] { 1000, 2000 }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayLong", new long[] { 1000, 2000 }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayFloat", new float[] { 1000.321f, 2000.321f }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayFloat", new float[] { 1000.321f, 2000.321f }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayDouble", new double[] { 1000.321d, 2000.321d }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayDouble", new double[] { 1000.321d, 2000.321d }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayBoolean", new bool[] { false, true }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayBoolean", new bool[] { false, true }, graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                trySetProperty(vertexA, "keyArrayEmpty", new int[0], graph.getFeatures().supportsPrimitiveArrayProperty.Value);
                trySetProperty(edge, "keyArrayEmpty", new int[0], graph.getFeatures().supportsPrimitiveArrayProperty.Value);

                Dictionary<string, string> map = new Dictionary<string, string>();
                map.put("testString", "try");
                map.put("testInteger", "string");

                trySetProperty(vertexA, "keyMap", map, graph.getFeatures().supportsMapProperty.Value);
                trySetProperty(edge, "keyMap", map, graph.getFeatures().supportsMapProperty.Value);

                MockSerializable mockSerializable = new MockSerializable();
                mockSerializable.setTestField("test");
                trySetProperty(vertexA, "keySerializable", mockSerializable, graph.getFeatures().supportsSerializableObjectProperty.Value);
                trySetProperty(edge, "keySerializable", mockSerializable, graph.getFeatures().supportsSerializableObjectProperty.Value);

            }

            graph.shutdown();
        }

        static void trySetProperty(Element element, string key, object value, bool allowDataType)
        {
            bool exceptionTossed = false;
            try
            {
                element.setProperty(key, value);
            }
            catch (Exception t)
            {
                exceptionTossed = true;
                if (!allowDataType)
                    Assert.True(t is ArgumentException);
                else
                    Assert.Fail("SetProperty should not have thrown an exception as this data type is accepted according to the GraphTest settings.");
            }

            if (!allowDataType && !exceptionTossed)
                Assert.Fail("SetProperty threw an exception but the data type should have been accepted.");
        }

        [Test]
        public void testSimpleRemovingVerticesEdges()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v = graph.addVertex(null);
            Vertex u = graph.addVertex(null);
            Edge e = graph.addEdge(null, v, u, convertId(graph, "knows"));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 2);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 1);

            Assert.AreEqual(v.getEdges(Direction.OUT).First().getVertex(Direction.IN), u);
            Assert.AreEqual(u.getEdges(Direction.IN).First().getVertex(Direction.OUT), v);
            Assert.AreEqual(v.getEdges(Direction.OUT).First(), e);
            Assert.AreEqual(u.getEdges(Direction.IN).First(), e);
            graph.removeVertex(v);
            //TODO: DEX
            //assertFalse(v.getEdges(direction.OUT).iterator().hasNext());

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 1);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 0);

            graph.shutdown();
        }

        [Test]
        public void testRemovingEdges()
        {
            Graph graph = graphTest.generateGraph();
            int vertexCount = 100;
            int edgeCount = 200;
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();
            Random random = new Random();
            this.stopWatch();
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.addVertex(null));

            printPerformance(graph.ToString(), vertexCount, "vertices added", this.stopWatch());
            this.stopWatch();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex a = vertices.ElementAt(random.Next(vertices.Count()));
                Vertex b = vertices.ElementAt(random.Next(vertices.Count()));
                if (a != b)
                    edges.Add(graph.addEdge(null, a, b, convertId(graph, string.Concat("a", Guid.NewGuid()))));
            }
            printPerformance(graph.ToString(), edgeCount, "edges added", this.stopWatch());
            this.stopWatch();
            int counter = 0;
            foreach (Edge e in edges)
            {
                counter = counter + 1;
                graph.removeEdge(e);
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(edges.Count() - counter, count(graph.getEdges()));

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(vertices.Count(), count(graph.getVertices()));
            }
            printPerformance(graph.ToString(), edgeCount, "edges deleted (with Count check on each delete)", this.stopWatch());
            graph.shutdown();

        }

        [Test]
        public void testRemovingVertices()
        {
            Graph graph = graphTest.generateGraph();
            int vertexCount = 500;
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();

            this.stopWatch();
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.addVertex(null));

            printPerformance(graph.ToString(), vertexCount, "vertices added", this.stopWatch());

            this.stopWatch();
            for (int i = 0; i < vertexCount; i = i + 2)
            {
                Vertex a = vertices.ElementAt(i);
                Vertex b = vertices.ElementAt(i + 1);
                edges.Add(graph.addEdge(null, a, b, convertId(graph, string.Concat("a", Guid.NewGuid()))));

            }
            printPerformance(graph.ToString(), vertexCount / 2, "edges added", this.stopWatch());

            this.stopWatch();
            int counter = 0;
            foreach (Vertex v in vertices)
            {
                counter = counter + 1;
                graph.removeVertex(v);
                if ((counter + 1) % 2 == 0)
                {
                    if (graph.getFeatures().supportsEdgeIteration.Value)
                        Assert.AreEqual(edges.Count() - ((counter + 1) / 2), count(graph.getEdges()));
                }

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(vertices.Count() - counter, count(graph.getVertices()));
            }
            printPerformance(graph.ToString(), vertexCount, "vertices deleted (with Count check on each delete)", this.stopWatch());
            graph.shutdown();
        }

        [Test]
        public void testConnectivityPatterns()
        {
            Graph graph = graphTest.generateGraph();

            Vertex a = graph.addVertex(convertId(graph, "1"));
            Vertex b = graph.addVertex(convertId(graph, "2"));
            Vertex c = graph.addVertex(convertId(graph, "3"));
            Vertex d = graph.addVertex(convertId(graph, "4"));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(4, count(graph.getVertices()));

            Edge e = graph.addEdge(null, a, b, convertId(graph, "knows"));
            Edge f = graph.addEdge(null, b, c, convertId(graph, "knows"));
            Edge g = graph.addEdge(null, c, d, convertId(graph, "knows"));
            Edge h = graph.addEdge(null, d, a, convertId(graph, "knows"));

            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(4, count(graph.getEdges()));

            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                foreach (Vertex v in graph.getVertices())
                {
                    Assert.AreEqual(1, count(v.getEdges(Direction.OUT)));
                    Assert.AreEqual(1, count(v.getEdges(Direction.IN)));
                }
            }

            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                foreach (Edge x in graph.getEdges())
                    Assert.AreEqual(convertId(graph, "knows"), x.getLabel());
            }

            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                a = graph.getVertex(convertId(graph, "1"));
                b = graph.getVertex(convertId(graph, "2"));
                c = graph.getVertex(convertId(graph, "3"));
                d = graph.getVertex(convertId(graph, "4"));

                Assert.AreEqual(1, count(a.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(a.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(b.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(b.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(c.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(c.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(d.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(d.getEdges(Direction.OUT)));

                Edge i = graph.addEdge(null, a, b, convertId(graph, "hates"));

                Assert.AreEqual(1, count(a.getEdges(Direction.IN)));
                Assert.AreEqual(2, count(a.getEdges(Direction.OUT)));
                Assert.AreEqual(2, count(b.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(b.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(c.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(c.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(d.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(d.getEdges(Direction.OUT)));

                Assert.AreEqual(1, count(a.getEdges(Direction.IN)));
                Assert.AreEqual(2, count(a.getEdges(Direction.OUT)));
                foreach (Edge x in a.getEdges(Direction.OUT))
                    Assert.True(x.getLabel() == convertId(graph, "knows") || x.getLabel() == convertId(graph, "hates"));

                Assert.AreEqual(convertId(graph, "hates"), i.getLabel());
                Assert.AreEqual(i.getVertex(Direction.IN).getId().ToString(), convertId(graph, "2"));
                Assert.AreEqual(i.getVertex(Direction.OUT).getId().ToString(), convertId(graph, "1"));
            }

            HashSet<object> vertexIds = new HashSet<object>();
            vertexIds.Add(a.getId());
            vertexIds.Add(a.getId());
            vertexIds.Add(b.getId());
            vertexIds.Add(b.getId());
            vertexIds.Add(c.getId());
            vertexIds.Add(d.getId());
            vertexIds.Add(d.getId());
            vertexIds.Add(d.getId());
            Assert.AreEqual(4, vertexIds.Count());
            graph.shutdown();
        }

        [Test]
        public void testVertexEdgeLabels()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Vertex c = graph.addVertex(null);
            Edge aFriendB = graph.addEdge(null, a, b, convertId(graph, "friend"));
            Edge aFriendC = graph.addEdge(null, a, c, convertId(graph, "friend"));
            Edge aHateC = graph.addEdge(null, a, c, convertId(graph, "hate"));
            Edge cHateA = graph.addEdge(null, c, a, convertId(graph, "hate"));
            Edge cHateB = graph.addEdge(null, c, b, convertId(graph, "hate"));

            IEnumerable<Edge> results = a.getEdges(Direction.OUT);
            Assert.AreEqual(results.Count(), 3);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));
            Assert.True(results.Contains(aHateC));

            results = a.getEdges(Direction.OUT, convertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 2);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));

            results = a.getEdges(Direction.OUT, convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(aHateC));

            results = a.getEdges(Direction.IN, convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateA));

            results = a.getEdges(Direction.IN, convertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 0);

            results = b.getEdges(Direction.IN, convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateB));

            results = b.getEdges(Direction.IN, convertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(aFriendB));

            graph.shutdown();
        }

        [Test]
        public void testVertexEdgeLabels2()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Vertex c = graph.addVertex(null);
            Edge aFriendB = graph.addEdge(null, a, b, convertId(graph, "friend"));
            Edge aFriendC = graph.addEdge(null, a, c, convertId(graph, "friend"));
            Edge aHateC = graph.addEdge(null, a, c, convertId(graph, "hate"));
            Edge cHateA = graph.addEdge(null, c, a, convertId(graph, "hate"));
            Edge cHateB = graph.addEdge(null, c, b, convertId(graph, "hate"));


            IEnumerable<Edge> results = a.getEdges(Direction.OUT, convertId(graph, "friend"), convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 3);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));
            Assert.True(results.Contains(aHateC));

            results = a.getEdges(Direction.IN, convertId(graph, "friend"), convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateA));

            results = b.getEdges(Direction.IN, convertId(graph, "friend"), convertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 2);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(cHateB));

            results = b.getEdges(Direction.IN, convertId(graph, "blah"), convertId(graph, "blah2"), convertId(graph, "blah3"));
            Assert.AreEqual(results.Count(), 0);

            graph.shutdown();
        }

        [Test]
        public void testTreeConnectivity()
        {
            Graph graph = graphTest.generateGraph();
            this.stopWatch();
            int branchSize = 11;
            Vertex start = graph.addVertex(null);
            for (int i = 0; i < branchSize; i++)
            {
                Vertex a = graph.addVertex(null);
                graph.addEdge(null, start, a, convertId(graph, "test1"));
                for (int j = 0; j < branchSize; j++)
                {
                    Vertex b = graph.addVertex(null);
                    graph.addEdge(null, a, b, convertId(graph, "test2"));
                    for (int k = 0; k < branchSize; k++)
                    {
                        Vertex c = graph.addVertex(null);
                        graph.addEdge(null, b, c, convertId(graph, "test3"));
                    }
                }
            }

            Assert.AreEqual(0, count(start.getEdges(Direction.IN)));
            Assert.AreEqual(branchSize, count(start.getEdges(Direction.OUT)));
            foreach (Edge e in start.getEdges(Direction.OUT))
            {
                Assert.AreEqual(convertId(graph, "test1"), e.getLabel());
                Assert.AreEqual(branchSize, count(e.getVertex(Direction.IN).getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(e.getVertex(Direction.IN).getEdges(Direction.IN)));
                foreach (Edge f in e.getVertex(Direction.IN).getEdges(Direction.OUT))
                {
                    Assert.AreEqual(convertId(graph, "test2"), f.getLabel());
                    Assert.AreEqual(branchSize, count(f.getVertex(Direction.IN).getEdges(Direction.OUT)));
                    Assert.AreEqual(1, count(f.getVertex(Direction.IN).getEdges(Direction.IN)));
                    foreach (Edge g in f.getVertex(Direction.IN).getEdges(Direction.OUT))
                    {
                        Assert.AreEqual(convertId(graph, "test3"), g.getLabel());
                        Assert.AreEqual(0, count(g.getVertex(Direction.IN).getEdges(Direction.OUT)));
                        Assert.AreEqual(1, count(g.getVertex(Direction.IN).getEdges(Direction.IN)));
                    }
                }
            }

            int totalVertices = 0;
            for (int i = 0; i < 4; i++)
                totalVertices = totalVertices + (int)Math.Pow(branchSize, i);

            printPerformance(graph.ToString(), totalVertices, "vertices added in a tree structure", this.stopWatch());

            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                foreach (Vertex v in graph.getVertices())
                    vertices.Add(v);

                Assert.AreEqual(totalVertices, vertices.Count());
                printPerformance(graph.ToString(), totalVertices, "vertices iterated", this.stopWatch());
            }

            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                this.stopWatch();
                HashSet<Edge> edges = new HashSet<Edge>();
                foreach (Edge e in graph.getEdges())
                    edges.Add(e);

                Assert.AreEqual(totalVertices - 1, edges.Count());
                printPerformance(graph.ToString(), totalVertices - 1, "edges iterated", this.stopWatch());
            }

            graph.shutdown();
        }

        [Test]
        public void testConcurrentModification()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            graph.addVertex(null);
            graph.addVertex(null);
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                foreach (Vertex vertex in graph.getVertices())
                {
                    graph.addEdge(null, vertex, a, convertId(graph, "x"));
                    graph.addEdge(null, vertex, a, convertId(graph, "y"));
                }
                foreach (Vertex vertex in graph.getVertices())
                {
                    Assert.AreEqual(BaseTest.count(vertex.getEdges(Direction.OUT)), 2);
                    foreach (Edge edge in vertex.getEdges(Direction.OUT))
                        graph.removeEdge(edge);
                }
                foreach (Vertex vertex in graph.getVertices())
                    graph.removeVertex(vertex);
            }
            else if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                for (int i = 0; i < 10; i++)
                    graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "test"));

                foreach (Edge edge in graph.getEdges())
                    graph.removeEdge(edge);
            }

            graph.shutdown();
        }

        [Test]
        public void testGraphDataPersists()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().isPersistent.Value)
            {
                Vertex v = graph.addVertex(null);
                Vertex u = graph.addVertex(null);
                if (graph.getFeatures().supportsVertexProperties.Value)
                {
                    v.setProperty("name", "marko");
                    u.setProperty("name", "pavel");
                }
                Edge e = graph.addEdge(null, v, u, convertId(graph, "collaborator"));
                if (graph.getFeatures().supportsEdgeProperties.Value)
                    e.setProperty("location", "internet");

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 2);

                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(count(graph.getEdges()), 1);

                graph.shutdown();

                this.stopWatch();
                graph = graphTest.generateGraph();
                printPerformance(graph.ToString(), 1, "graph loaded", this.stopWatch());
                if (graph.getFeatures().supportsVertexIteration.Value)
                {
                    Assert.AreEqual(count(graph.getVertices()), 2);
                    if (graph.getFeatures().supportsVertexProperties.Value)
                    {
                        foreach (Vertex vertex in graph.getVertices())
                            Assert.True(((string)vertex.getProperty("name")) == "marko" || ((string)vertex.getProperty("name")) == "pavel");
                    }
                }
                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(count(graph.getEdges()), 1);
                    foreach (Edge edge in graph.getEdges())
                    {
                        Assert.AreEqual(edge.getLabel(), convertId(graph, "collaborator"));
                        if (graph.getFeatures().supportsEdgeProperties.Value)
                            Assert.AreEqual(edge.getProperty("location"), "internet");
                    }
                }

            }
            graph.shutdown();
        }

        [Test]
        public void testAutotypingOfProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v = graph.addVertex(null);
                v.setProperty(convertId(graph, "string"), "marko");
                v.setProperty(convertId(graph, "integer"), 33);
                v.setProperty(convertId(graph, "boolean"), true);

                string name = (string)v.getProperty(convertId(graph, "string"));
                Assert.AreEqual(name, "marko");
                int age = (int)v.getProperty(convertId(graph, "integer"));
                Assert.AreEqual(age, 33);
                bool best = (bool)v.getProperty(convertId(graph, "boolean"));
                Assert.True(best);

                name = (string)v.removeProperty(convertId(graph, "string"));
                Assert.AreEqual(name, "marko");
                age = (int)v.removeProperty(convertId(graph, "integer"));
                Assert.AreEqual(age, 33);
                best = (bool)v.removeProperty(convertId(graph, "boolean"));
                Assert.True(best);
            }

            if (graph.getFeatures().supportsEdgeProperties.Value)
            {
                Edge e = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                e.setProperty(convertId(graph, "string"), "friend");
                e.setProperty(convertId(graph, "double"), 1.0d);

                string type = (string)e.getProperty(convertId(graph, "string"));
                Assert.AreEqual(type, "friend");
                double weight = (double)e.getProperty(convertId(graph, "double"));
                Assert.AreEqual(weight, 1.0d);

                type = (string)e.removeProperty(convertId(graph, "string"));
                Assert.AreEqual(type, "friend");
                weight = (double)e.removeProperty(convertId(graph, "double"));
                Assert.AreEqual(weight, 1.0d);
            }

            graph.shutdown();
        }
    }
}
