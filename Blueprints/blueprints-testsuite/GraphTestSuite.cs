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
        public void TestFeatureCompliance()
        {
            Graph graph = _GraphTest.GenerateGraph();
            graph.GetFeatures().CheckCompliance();
            Console.WriteLine(graph.GetFeatures());
            graph.Shutdown();
        }

        [Test]
        public void TestEmptyOnConstruction()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));
            graph.Shutdown();
        }

        [Test]
        public void TestStringRepresentation()
        {
            Graph graph = _GraphTest.GenerateGraph();
            try
            {
                this.StopWatch();
                Assert.NotNull(graph.ToString());
                Assert.True(graph.ToString().StartsWith(graph.GetType().Name.ToLower()));
                PrintPerformance(graph.ToString(), 1, "graph string representation generated", this.StopWatch());
            }
            catch (Exception)
            {
                Assert.False(true);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestStringRepresentationOfVertexId()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            object id = a.GetId();
            Vertex b = graph.GetVertex(id);
            Vertex c = graph.GetVertex(id.ToString());
            Assert.AreEqual(a, b);
            Assert.AreEqual(b, c);
            Assert.AreEqual(c, a);
            graph.Shutdown();
        }

        [Test]
        public void TestSemanticallyCorrectIterables()
        {
            Graph graph = _GraphTest.GenerateGraph();
            for (int i = 0; i < 15; i++)
            {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
            }
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                IEnumerable<Vertex> vertices = graph.GetVertices();
                Assert.AreEqual(Count(vertices), 30);
                Assert.AreEqual(Count(vertices), 30);
                int counter = vertices.Count();
                Assert.AreEqual(counter, 30);
            }
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                IEnumerable<Edge> edges = graph.GetEdges();
                Assert.AreEqual(Count(edges), 15);
                Assert.AreEqual(Count(edges), 15);
                int counter = edges.Count();
                Assert.AreEqual(counter, 15);
            }

            graph.Shutdown();
        }

        [Test]
        public void TestGettingVerticesAndEdgesWithKeyValue()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex v1 = graph.AddVertex(null);
                v1.SetProperty("name", "marko");
                v1.SetProperty("location", "everywhere");
                Vertex v2 = graph.AddVertex(null);
                v2.SetProperty("name", "stephen");
                v2.SetProperty("location", "everywhere");

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                {
                    Assert.AreEqual(Count(graph.GetVertices("location", "everywhere")), 2);
                    Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
                    Assert.AreEqual(Count(graph.GetVertices("name", "stephen")), 1);
                    Assert.AreEqual(GetOnlyElement(graph.GetVertices("name", "marko")), v1);
                    Assert.AreEqual(GetOnlyElement(graph.GetVertices("name", "stephen")), v2);
                }
            }

            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {
                Edge e1 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
                e1.SetProperty("name", "marko");
                e1.SetProperty("location", "everywhere");
                Edge e2 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
                e2.SetProperty("name", "stephen");
                e2.SetProperty("location", "everywhere");

                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(Count(graph.GetEdges("location", "everywhere")), 2);
                    Assert.AreEqual(Count(graph.GetEdges("name", "marko")), 1);
                    Assert.AreEqual(Count(graph.GetEdges("name", "stephen")), 1);
                    Assert.AreEqual(graph.GetEdges("name", "marko").First(), e1);
                    Assert.AreEqual(graph.GetEdges("name", "stephen").First(), e2);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddingVerticesAndEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);
            Edge edge = graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(1, Count(graph.GetEdges()));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(2, Count(graph.GetVertices()));

            graph.RemoveVertex(a);
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(1, Count(graph.GetVertices()));

            try
            {
                graph.RemoveEdge(edge);
                //TODO: doesn't work with wrapper graphs Assert.True(false);
            }
            catch (Exception)
            {
                Assert.True(true);
            }

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(1, Count(graph.GetVertices()));

            graph.Shutdown();
        }

        [Test]
        public void TestSettingProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {
                Vertex a = graph.AddVertex(null);
                Vertex b = graph.AddVertex(null);
                graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
                graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
                foreach (Edge edge in b.GetEdges(Direction.IN))
                    edge.SetProperty("key", "value");
            }
            graph.Shutdown();
        }

        [Test]
        public void TestDataTypeValidationOnProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsElementProperties() && !graph.GetFeatures().IsWrapper.Value)
            {
                Vertex vertexA = graph.AddVertex(null);
                Vertex vertexB = graph.AddVertex(null);
                Edge edge = graph.AddEdge(null, vertexA, vertexB, ConvertId(graph, "knows"));

                TrySetProperty(vertexA, "keyString", "value", graph.GetFeatures().SupportsStringProperty.Value);
                TrySetProperty(edge, "keyString", "value", graph.GetFeatures().SupportsStringProperty.Value);

                TrySetProperty(vertexA, "keyInteger", 100, graph.GetFeatures().SupportsIntegerProperty.Value);
                TrySetProperty(edge, "keyInteger", 100, graph.GetFeatures().SupportsIntegerProperty.Value);

                TrySetProperty(vertexA, "keyLong", 10000L, graph.GetFeatures().SupportsLongProperty.Value);
                TrySetProperty(edge, "keyLong", 10000L, graph.GetFeatures().SupportsLongProperty.Value);

                TrySetProperty(vertexA, "keyDouble", 100.321d, graph.GetFeatures().SupportsDoubleProperty.Value);
                TrySetProperty(edge, "keyDouble", 100.321d, graph.GetFeatures().SupportsDoubleProperty.Value);

                TrySetProperty(vertexA, "keyFloat", 100.321f, graph.GetFeatures().SupportsFloatProperty.Value);
                TrySetProperty(edge, "keyFloat", 100.321f, graph.GetFeatures().SupportsFloatProperty.Value);

                TrySetProperty(vertexA, "keyBoolean", true, graph.GetFeatures().SupportsBooleanProperty.Value);
                TrySetProperty(edge, "keyBoolean", true, graph.GetFeatures().SupportsBooleanProperty.Value);

                TrySetProperty(vertexA, "keyDate", new DateTime(), graph.GetFeatures().SupportsSerializableObjectProperty.Value);
                TrySetProperty(edge, "keyDate", new DateTime(), graph.GetFeatures().SupportsSerializableObjectProperty.Value);

                List<string> listA = new List<string>();
                listA.Add("try1");
                listA.Add("try2");

                TrySetProperty(vertexA, "keyListString", listA, graph.GetFeatures().SupportsUniformListProperty.Value);
                TrySetProperty(edge, "keyListString", listA, graph.GetFeatures().SupportsUniformListProperty.Value);

                List<object> listB = new List<object>();
                listB.Add("try1");
                listB.Add(2);

                TrySetProperty(vertexA, "keyListMixed", listB, graph.GetFeatures().SupportsMixedListProperty.Value);
                TrySetProperty(edge, "keyListMixed", listB, graph.GetFeatures().SupportsMixedListProperty.Value);

                TrySetProperty(vertexA, "keyArrayString", new string[] { "try1", "try2" }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayString", new string[] { "try1", "try2" }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayInteger", new int[] { 1, 2 }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayInteger", new int[] { 1, 2 }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayLong", new long[] { 1000, 2000 }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayLong", new long[] { 1000, 2000 }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayFloat", new float[] { 1000.321f, 2000.321f }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayFloat", new float[] { 1000.321f, 2000.321f }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayDouble", new double[] { 1000.321d, 2000.321d }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayDouble", new double[] { 1000.321d, 2000.321d }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayBoolean", new bool[] { false, true }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayBoolean", new bool[] { false, true }, graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                TrySetProperty(vertexA, "keyArrayEmpty", new int[0], graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);
                TrySetProperty(edge, "keyArrayEmpty", new int[0], graph.GetFeatures().SupportsPrimitiveArrayProperty.Value);

                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Put("testString", "try");
                map.Put("testInteger", "string");

                TrySetProperty(vertexA, "keyMap", map, graph.GetFeatures().SupportsMapProperty.Value);
                TrySetProperty(edge, "keyMap", map, graph.GetFeatures().SupportsMapProperty.Value);

                MockSerializable mockSerializable = new MockSerializable();
                mockSerializable.setTestField("test");
                TrySetProperty(vertexA, "keySerializable", mockSerializable, graph.GetFeatures().SupportsSerializableObjectProperty.Value);
                TrySetProperty(edge, "keySerializable", mockSerializable, graph.GetFeatures().SupportsSerializableObjectProperty.Value);

            }

            graph.Shutdown();
        }

        void TrySetProperty(Element element, string key, object value, bool allowDataType)
        {
            bool exceptionTossed = false;
            try
            {
                element.SetProperty(key, value);
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
        public void TestSimpleRemovingVerticesEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v = graph.AddVertex(null);
            Vertex u = graph.AddVertex(null);
            Edge e = graph.AddEdge(null, v, u, ConvertId(graph, "knows"));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), 2);
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), 1);

            Assert.AreEqual(v.GetEdges(Direction.OUT).First().GetVertex(Direction.IN), u);
            Assert.AreEqual(u.GetEdges(Direction.IN).First().GetVertex(Direction.OUT), v);
            Assert.AreEqual(v.GetEdges(Direction.OUT).First(), e);
            Assert.AreEqual(u.GetEdges(Direction.IN).First(), e);
            graph.RemoveVertex(v);
            //TODO: DEX
            //assertFalse(v.GetEdges(Direction.OUT).iterator().hasNext());

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), 1);
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), 0);

            graph.Shutdown();
        }

        [Test]
        public void TestRemovingEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            int vertexCount = 100;
            int edgeCount = 200;
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();
            Random random = new Random();
            this.StopWatch();
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.AddVertex(null));

            PrintPerformance(graph.ToString(), vertexCount, "vertices added", this.StopWatch());
            this.StopWatch();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex a = vertices.ElementAt(random.Next(vertices.Count()));
                Vertex b = vertices.ElementAt(random.Next(vertices.Count()));
                if (a != b)
                    edges.Add(graph.AddEdge(null, a, b, ConvertId(graph, string.Concat("a", Guid.NewGuid()))));
            }
            PrintPerformance(graph.ToString(), edgeCount, "edges added", this.StopWatch());
            this.StopWatch();
            int counter = 0;
            foreach (Edge e in edges)
            {
                counter = counter + 1;
                graph.RemoveEdge(e);
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(edges.Count() - counter, Count(graph.GetEdges()));

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(vertices.Count(), Count(graph.GetVertices()));
            }
            PrintPerformance(graph.ToString(), edgeCount, "edges deleted (with Count check on each delete)", this.StopWatch());
            graph.Shutdown();

        }

        [Test]
        public void TestRemovingVertices()
        {
            Graph graph = _GraphTest.GenerateGraph();
            int vertexCount = 500;
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();

            this.StopWatch();
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.AddVertex(null));

            PrintPerformance(graph.ToString(), vertexCount, "vertices added", this.StopWatch());

            this.StopWatch();
            for (int i = 0; i < vertexCount; i = i + 2)
            {
                Vertex a = vertices.ElementAt(i);
                Vertex b = vertices.ElementAt(i + 1);
                edges.Add(graph.AddEdge(null, a, b, ConvertId(graph, string.Concat("a", Guid.NewGuid()))));

            }
            PrintPerformance(graph.ToString(), vertexCount / 2, "edges added", this.StopWatch());

            this.StopWatch();
            int counter = 0;
            foreach (Vertex v in vertices)
            {
                counter = counter + 1;
                graph.RemoveVertex(v);
                if ((counter + 1) % 2 == 0)
                {
                    if (graph.GetFeatures().SupportsEdgeIteration.Value)
                        Assert.AreEqual(edges.Count() - ((counter + 1) / 2), Count(graph.GetEdges()));
                }

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(vertices.Count() - counter, Count(graph.GetVertices()));
            }
            PrintPerformance(graph.ToString(), vertexCount, "vertices deleted (with Count check on each delete)", this.StopWatch());
            graph.Shutdown();
        }

        [Test]
        public void TestConnectivityPatterns()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex a = graph.AddVertex(ConvertId(graph, "1"));
            Vertex b = graph.AddVertex(ConvertId(graph, "2"));
            Vertex c = graph.AddVertex(ConvertId(graph, "3"));
            Vertex d = graph.AddVertex(ConvertId(graph, "4"));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(4, Count(graph.GetVertices()));

            Edge e = graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
            Edge f = graph.AddEdge(null, b, c, ConvertId(graph, "knows"));
            Edge g = graph.AddEdge(null, c, d, ConvertId(graph, "knows"));
            Edge h = graph.AddEdge(null, d, a, ConvertId(graph, "knows"));

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(4, Count(graph.GetEdges()));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                foreach (Vertex v in graph.GetVertices())
                {
                    Assert.AreEqual(1, Count(v.GetEdges(Direction.OUT)));
                    Assert.AreEqual(1, Count(v.GetEdges(Direction.IN)));
                }
            }

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                foreach (Edge x in graph.GetEdges())
                    Assert.AreEqual(ConvertId(graph, "knows"), x.GetLabel());
            }

            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                a = graph.GetVertex(ConvertId(graph, "1"));
                b = graph.GetVertex(ConvertId(graph, "2"));
                c = graph.GetVertex(ConvertId(graph, "3"));
                d = graph.GetVertex(ConvertId(graph, "4"));

                Assert.AreEqual(1, Count(a.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(a.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(b.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(b.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(c.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(c.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(d.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(d.GetEdges(Direction.OUT)));

                Edge i = graph.AddEdge(null, a, b, ConvertId(graph, "hates"));

                Assert.AreEqual(1, Count(a.GetEdges(Direction.IN)));
                Assert.AreEqual(2, Count(a.GetEdges(Direction.OUT)));
                Assert.AreEqual(2, Count(b.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(b.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(c.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(c.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(d.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(d.GetEdges(Direction.OUT)));

                Assert.AreEqual(1, Count(a.GetEdges(Direction.IN)));
                Assert.AreEqual(2, Count(a.GetEdges(Direction.OUT)));
                foreach (Edge x in a.GetEdges(Direction.OUT))
                    Assert.True(x.GetLabel() == ConvertId(graph, "knows") || x.GetLabel() == ConvertId(graph, "hates"));

                Assert.AreEqual(ConvertId(graph, "hates"), i.GetLabel());
                Assert.AreEqual(i.GetVertex(Direction.IN).GetId().ToString(), ConvertId(graph, "2"));
                Assert.AreEqual(i.GetVertex(Direction.OUT).GetId().ToString(), ConvertId(graph, "1"));
            }

            HashSet<object> vertexIds = new HashSet<object>();
            vertexIds.Add(a.GetId());
            vertexIds.Add(a.GetId());
            vertexIds.Add(b.GetId());
            vertexIds.Add(b.GetId());
            vertexIds.Add(c.GetId());
            vertexIds.Add(d.GetId());
            vertexIds.Add(d.GetId());
            vertexIds.Add(d.GetId());
            Assert.AreEqual(4, vertexIds.Count());
            graph.Shutdown();
        }

        [Test]
        public void TestVertexEdgeLabels()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);
            Vertex c = graph.AddVertex(null);
            Edge aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
            Edge aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
            Edge aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
            Edge cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
            Edge cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));

            IEnumerable<Edge> results = a.GetEdges(Direction.OUT);
            Assert.AreEqual(results.Count(), 3);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));
            Assert.True(results.Contains(aHateC));

            results = a.GetEdges(Direction.OUT, ConvertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 2);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));

            results = a.GetEdges(Direction.OUT, ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(aHateC));

            results = a.GetEdges(Direction.IN, ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateA));

            results = a.GetEdges(Direction.IN, ConvertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 0);

            results = b.GetEdges(Direction.IN, ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateB));

            results = b.GetEdges(Direction.IN, ConvertId(graph, "friend"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(aFriendB));

            graph.Shutdown();
        }

        [Test]
        public void TestVertexEdgeLabels2()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);
            Vertex c = graph.AddVertex(null);
            Edge aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
            Edge aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
            Edge aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
            Edge cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
            Edge cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));


            IEnumerable<Edge> results = a.GetEdges(Direction.OUT, ConvertId(graph, "friend"), ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 3);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(aFriendC));
            Assert.True(results.Contains(aHateC));

            results = a.GetEdges(Direction.IN, ConvertId(graph, "friend"), ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 1);
            Assert.True(results.Contains(cHateA));

            results = b.GetEdges(Direction.IN, ConvertId(graph, "friend"), ConvertId(graph, "hate"));
            Assert.AreEqual(results.Count(), 2);
            Assert.True(results.Contains(aFriendB));
            Assert.True(results.Contains(cHateB));

            results = b.GetEdges(Direction.IN, ConvertId(graph, "blah"), ConvertId(graph, "blah2"), ConvertId(graph, "blah3"));
            Assert.AreEqual(results.Count(), 0);

            graph.Shutdown();
        }

        [Test]
        public void TestTreeConnectivity()
        {
            Graph graph = _GraphTest.GenerateGraph();
            this.StopWatch();
            int branchSize = 11;
            Vertex start = graph.AddVertex(null);
            for (int i = 0; i < branchSize; i++)
            {
                Vertex a = graph.AddVertex(null);
                graph.AddEdge(null, start, a, ConvertId(graph, "test1"));
                for (int j = 0; j < branchSize; j++)
                {
                    Vertex b = graph.AddVertex(null);
                    graph.AddEdge(null, a, b, ConvertId(graph, "test2"));
                    for (int k = 0; k < branchSize; k++)
                    {
                        Vertex c = graph.AddVertex(null);
                        graph.AddEdge(null, b, c, ConvertId(graph, "test3"));
                    }
                }
            }

            Assert.AreEqual(0, Count(start.GetEdges(Direction.IN)));
            Assert.AreEqual(branchSize, Count(start.GetEdges(Direction.OUT)));
            foreach (Edge e in start.GetEdges(Direction.OUT))
            {
                Assert.AreEqual(ConvertId(graph, "test1"), e.GetLabel());
                Assert.AreEqual(branchSize, Count(e.GetVertex(Direction.IN).GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(e.GetVertex(Direction.IN).GetEdges(Direction.IN)));
                foreach (Edge f in e.GetVertex(Direction.IN).GetEdges(Direction.OUT))
                {
                    Assert.AreEqual(ConvertId(graph, "test2"), f.GetLabel());
                    Assert.AreEqual(branchSize, Count(f.GetVertex(Direction.IN).GetEdges(Direction.OUT)));
                    Assert.AreEqual(1, Count(f.GetVertex(Direction.IN).GetEdges(Direction.IN)));
                    foreach (Edge g in f.GetVertex(Direction.IN).GetEdges(Direction.OUT))
                    {
                        Assert.AreEqual(ConvertId(graph, "test3"), g.GetLabel());
                        Assert.AreEqual(0, Count(g.GetVertex(Direction.IN).GetEdges(Direction.OUT)));
                        Assert.AreEqual(1, Count(g.GetVertex(Direction.IN).GetEdges(Direction.IN)));
                    }
                }
            }

            int totalVertices = 0;
            for (int i = 0; i < 4; i++)
                totalVertices = totalVertices + (int)Math.Pow(branchSize, i);

            PrintPerformance(graph.ToString(), totalVertices, "vertices added in a tree structure", this.StopWatch());

            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                this.StopWatch();
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                foreach (Vertex v in graph.GetVertices())
                    vertices.Add(v);

                Assert.AreEqual(totalVertices, vertices.Count());
                PrintPerformance(graph.ToString(), totalVertices, "vertices iterated", this.StopWatch());
            }

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                this.StopWatch();
                HashSet<Edge> edges = new HashSet<Edge>();
                foreach (Edge e in graph.GetEdges())
                    edges.Add(e);

                Assert.AreEqual(totalVertices - 1, edges.Count());
                PrintPerformance(graph.ToString(), totalVertices - 1, "edges iterated", this.StopWatch());
            }

            graph.Shutdown();
        }

        [Test]
        public void TestConcurrentModification()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            graph.AddVertex(null);
            graph.AddVertex(null);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                foreach (Vertex vertex in graph.GetVertices())
                {
                    graph.AddEdge(null, vertex, a, ConvertId(graph, "x"));
                    graph.AddEdge(null, vertex, a, ConvertId(graph, "y"));
                }
                foreach (Vertex vertex in graph.GetVertices())
                {
                    Assert.AreEqual(BaseTest.Count(vertex.GetEdges(Direction.OUT)), 2);
                    foreach (Edge edge in vertex.GetEdges(Direction.OUT))
                        graph.RemoveEdge(edge);
                }
                foreach (Vertex vertex in graph.GetVertices())
                    graph.RemoveVertex(vertex);
            }
            else if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                for (int i = 0; i < 10; i++)
                    graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));

                foreach (Edge edge in graph.GetEdges())
                    graph.RemoveEdge(edge);
            }

            graph.Shutdown();
        }

        [Test]
        public void TestGraphDataPersists()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().IsPersistent.Value)
            {
                Vertex v = graph.AddVertex(null);
                Vertex u = graph.AddVertex(null);
                if (graph.GetFeatures().SupportsVertexProperties.Value)
                {
                    v.SetProperty("name", "marko");
                    u.SetProperty("name", "pavel");
                }
                Edge e = graph.AddEdge(null, v, u, ConvertId(graph, "collaborator"));
                if (graph.GetFeatures().SupportsEdgeProperties.Value)
                    e.SetProperty("location", "internet");

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 2);

                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(Count(graph.GetEdges()), 1);

                graph.Shutdown();

                this.StopWatch();
                graph = _GraphTest.GenerateGraph();
                PrintPerformance(graph.ToString(), 1, "graph loaded", this.StopWatch());
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                {
                    Assert.AreEqual(Count(graph.GetVertices()), 2);
                    if (graph.GetFeatures().SupportsVertexProperties.Value)
                    {
                        foreach (Vertex vertex in graph.GetVertices())
                            Assert.True(((string)vertex.GetProperty("name")) == "marko" || ((string)vertex.GetProperty("name")) == "pavel");
                    }
                }
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 1);
                    foreach (Edge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetLabel(), ConvertId(graph, "collaborator"));
                        if (graph.GetFeatures().SupportsEdgeProperties.Value)
                            Assert.AreEqual(edge.GetProperty("location"), "internet");
                    }
                }

            }
            graph.Shutdown();
        }

        [Test]
        public void TestAutotypingOfProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex v = graph.AddVertex(null);
                v.SetProperty(ConvertId(graph, "string"), "marko");
                v.SetProperty(ConvertId(graph, "integer"), 33);
                v.SetProperty(ConvertId(graph, "boolean"), true);

                string name = (string)v.GetProperty(ConvertId(graph, "string"));
                Assert.AreEqual(name, "marko");
                int age = (int)v.GetProperty(ConvertId(graph, "integer"));
                Assert.AreEqual(age, 33);
                bool best = (bool)v.GetProperty(ConvertId(graph, "boolean"));
                Assert.True(best);

                name = (string)v.RemoveProperty(ConvertId(graph, "string"));
                Assert.AreEqual(name, "marko");
                age = (int)v.RemoveProperty(ConvertId(graph, "integer"));
                Assert.AreEqual(age, 33);
                best = (bool)v.RemoveProperty(ConvertId(graph, "boolean"));
                Assert.True(best);
            }

            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {
                Edge e = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                e.SetProperty(ConvertId(graph, "string"), "friend");
                e.SetProperty(ConvertId(graph, "double"), 1.0d);

                string type = (string)e.GetProperty(ConvertId(graph, "string"));
                Assert.AreEqual(type, "friend");
                double weight = (double)e.GetProperty(ConvertId(graph, "double"));
                Assert.AreEqual(weight, 1.0d);

                type = (string)e.RemoveProperty(ConvertId(graph, "string"));
                Assert.AreEqual(type, "friend");
                weight = (double)e.RemoveProperty(ConvertId(graph, "double"));
                Assert.AreEqual(weight, 1.0d);
            }

            graph.Shutdown();
        }
    }
}
