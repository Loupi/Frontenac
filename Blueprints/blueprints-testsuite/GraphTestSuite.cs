using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util.IO;

namespace Frontenac.Blueprints
{
    public abstract class GraphTestSuite : TestSuite
    {
        protected GraphTestSuite(GraphTest graphTest)
            : base("GraphTestSuite", graphTest)
        {

        }

        [Test]
        public void TestFeatureCompliance()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                graph.Features.CheckCompliance();
                Console.WriteLine(graph.Features);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestEmptyOnConstruction()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(0, Count(graph.GetVertices()));
                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(0, Count(graph.GetEdges()));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestStringRepresentation()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                try
                {
                    StopWatch();
                    Assert.NotNull(graph.ToString());
                    Assert.True(graph.ToString().StartsWith(graph.GetType().Name.ToLower()));
                    PrintPerformance(graph.ToString(), 1, "graph string representation generated", StopWatch());
                }
                catch (Exception)
                {
                    Assert.False(true);
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestStringRepresentationOfVertexId()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                IVertex a = graph.AddVertex(null);
                object id = a.Id;
                IVertex b = graph.GetVertex(id);
                IVertex c = graph.GetVertex(id.ToString());
                Assert.AreEqual(a, b);
                Assert.AreEqual(b, c);
                Assert.AreEqual(c, a);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestSemanticallyCorrectIterables()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                for (int i = 0; i < 15; i++)
                {
                    graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
                }

                if (graph.Features.SupportsVertexIteration)
                {
                    IEnumerable<IVertex> vertices = graph.GetVertices().ToArray();
                    Assert.AreEqual(Count(vertices), 30);
                    Assert.AreEqual(Count(vertices), 30);
                    int counter = vertices.Count();
                    Assert.AreEqual(counter, 30);
                }

                if (graph.Features.SupportsEdgeIteration)
                {
                    IEnumerable<IEdge> edges = graph.GetEdges().ToArray();
                    Assert.AreEqual(Count(edges), 15);
                    Assert.AreEqual(Count(edges), 15);
                    int counter = edges.Count();
                    Assert.AreEqual(counter, 15);
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGettingVerticesAndEdgesWithKeyValue()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsVertexProperties)
                {
                    IVertex v1 = graph.AddVertex(null);
                    v1.SetProperty("name", "marko");
                    v1.SetProperty("location", "everywhere");
                    IVertex v2 = graph.AddVertex(null);
                    v2.SetProperty("name", "stephen");
                    v2.SetProperty("location", "everywhere");

                    if (graph.Features.SupportsVertexIteration)
                    {
                        Assert.AreEqual(Count(graph.GetVertices("location", "everywhere")), 2);
                        Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
                        Assert.AreEqual(Count(graph.GetVertices("name", "stephen")), 1);
                        Assert.AreEqual(GetOnlyElement(graph.GetVertices("name", "marko")), v1);
                        Assert.AreEqual(GetOnlyElement(graph.GetVertices("name", "stephen")), v2);
                    }
                }

                if (graph.Features.SupportsEdgeProperties)
                {
                    IEdge e1 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null),
                                             ConvertId(graph, "knows"));
                    e1.SetProperty("name", "marko");
                    e1.SetProperty("location", "everywhere");
                    IEdge e2 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null),
                                             ConvertId(graph, "knows"));
                    e2.SetProperty("name", "stephen");
                    e2.SetProperty("location", "everywhere");

                    if (graph.Features.SupportsEdgeIteration)
                    {
                        Assert.AreEqual(Count(graph.GetEdges("location", "everywhere")), 2);
                        Assert.AreEqual(Count(graph.GetEdges("name", "marko")), 1);
                        Assert.AreEqual(Count(graph.GetEdges("name", "stephen")), 1);
                        Assert.AreEqual(graph.GetEdges("name", "marko").First(), e1);
                        Assert.AreEqual(graph.GetEdges("name", "stephen").First(), e2);
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddingVerticesAndEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var a = graph.AddVertex(null);
                var b = graph.AddVertex(null);
                var edge = graph.AddEdge(null, a, b, ConvertId(graph, "knows"));

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(1, Count(graph.GetEdges()));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(2, Count(graph.GetVertices()));

                graph.RemoveVertex(a);
                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(0, Count(graph.GetEdges()));

                if (graph.Features.SupportsVertexIteration)
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

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(0, Count(graph.GetEdges()));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(1, Count(graph.GetVertices()));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestSettingProperties()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsEdgeProperties)
                {
                    IVertex a = graph.AddVertex(null);
                    IVertex b = graph.AddVertex(null);
                    graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
                    graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
                    foreach (IEdge edge in b.GetEdges(Direction.In))
                        edge.SetProperty("key", "value");
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestDataTypeValidationOnProperties()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsElementProperties() && !graph.Features.IsWrapper)
                {
                    IVertex vertexA = graph.AddVertex(null);
                    IVertex vertexB = graph.AddVertex(null);
                    IEdge edge = graph.AddEdge(null, vertexA, vertexB, ConvertId(graph, "knows"));

                    TrySetProperty(vertexA, "keyString", "value", graph.Features.SupportsStringProperty);
                    TrySetProperty(edge, "keyString", "value", graph.Features.SupportsStringProperty);

                    TrySetProperty(vertexA, "keyInteger", 100, graph.Features.SupportsIntegerProperty);
                    TrySetProperty(edge, "keyInteger", 100, graph.Features.SupportsIntegerProperty);

                    TrySetProperty(vertexA, "keyLong", 10000L, graph.Features.SupportsLongProperty);
                    TrySetProperty(edge, "keyLong", 10000L, graph.Features.SupportsLongProperty);

                    TrySetProperty(vertexA, "keyDouble", 100.321d, graph.Features.SupportsDoubleProperty);
                    TrySetProperty(edge, "keyDouble", 100.321d, graph.Features.SupportsDoubleProperty);

                    TrySetProperty(vertexA, "keyFloat", 100.321f, graph.Features.SupportsFloatProperty);
                    TrySetProperty(edge, "keyFloat", 100.321f, graph.Features.SupportsFloatProperty);

                    TrySetProperty(vertexA, "keyBoolean", true, graph.Features.SupportsBooleanProperty);
                    TrySetProperty(edge, "keyBoolean", true, graph.Features.SupportsBooleanProperty);

                    TrySetProperty(vertexA, "keyDate", new DateTime(),
                                   graph.Features.SupportsSerializableObjectProperty);
                    TrySetProperty(edge, "keyDate", new DateTime(),
                                   graph.Features.SupportsSerializableObjectProperty);

                    var listA = new List<string> {"try1", "try2"};

                    TrySetProperty(vertexA, "keyListString", listA, graph.Features.SupportsUniformListProperty);
                    TrySetProperty(edge, "keyListString", listA, graph.Features.SupportsUniformListProperty);

                    var listB = new List<object> {"try1", 2};

                    TrySetProperty(vertexA, "keyListMixed", listB, graph.Features.SupportsMixedListProperty);
                    TrySetProperty(edge, "keyListMixed", listB, graph.Features.SupportsMixedListProperty);

                    TrySetProperty(vertexA, "keyArrayString", new[] {"try1", "try2"},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayString", new[] {"try1", "try2"},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayInteger", new[] {1, 2},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayInteger", new[] {1, 2},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayLong", new long[] {1000, 2000},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayLong", new long[] {1000, 2000},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayFloat", new[] {1000.321f, 2000.321f},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayFloat", new[] {1000.321f, 2000.321f},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayDouble", new[] {1000.321d, 2000.321d},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayDouble", new[] {1000.321d, 2000.321d},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayBoolean", new[] {false, true},
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayBoolean", new[] {false, true},
                                   graph.Features.SupportsPrimitiveArrayProperty);

                    TrySetProperty(vertexA, "keyArrayEmpty", new int[0],
                                   graph.Features.SupportsPrimitiveArrayProperty);
                    TrySetProperty(edge, "keyArrayEmpty", new int[0], graph.Features.SupportsPrimitiveArrayProperty);

                    var map = new Dictionary<string, string>();
                    map.Put("testString", "try");
                    map.Put("testInteger", "string");

                    TrySetProperty(vertexA, "keyMap", map, graph.Features.SupportsMapProperty);
                    TrySetProperty(edge, "keyMap", map, graph.Features.SupportsMapProperty);

                    var mockSerializable = new MockSerializable("test");
                    TrySetProperty(vertexA, "keySerializable", mockSerializable,
                                   graph.Features.SupportsSerializableObjectProperty);
                    TrySetProperty(edge, "keySerializable", mockSerializable,
                                   graph.Features.SupportsSerializableObjectProperty);

                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        static void TrySetProperty(IElement element, string key, object value, bool allowDataType)
        {
            var exceptionTossed = false;
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
            var graph = GraphTest.GenerateGraph();
            try
            {
                IVertex v = graph.AddVertex(null);
                IVertex u = graph.AddVertex(null);
                IEdge e = graph.AddEdge(null, v, u, ConvertId(graph, "knows"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 2);

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 1);

                Assert.AreEqual(v.GetEdges(Direction.Out).First().GetVertex(Direction.In), u);
                Assert.AreEqual(u.GetEdges(Direction.In).First().GetVertex(Direction.Out), v);
                Assert.AreEqual(v.GetEdges(Direction.Out).First(), e);
                Assert.AreEqual(u.GetEdges(Direction.In).First(), e);
                graph.RemoveVertex(v);
                //TODO: DEX
                //assertFalse(v.getEdges(direction.OUT).iterator().hasNext());

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 1);

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemovingEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                const int vertexCount = 100;
                const int edgeCount = 200;
                var vertices = new List<IVertex>();
                var edges = new List<IEdge>();
                var random = new Random();
                StopWatch();
                for (int i = 0; i < vertexCount; i++)
                    vertices.Add(graph.AddVertex(null));

                PrintPerformance(graph.ToString(), vertexCount, "vertices added", StopWatch());
                StopWatch();
                for (int i = 0; i < edgeCount; i++)
                {
                    IVertex a = vertices.ElementAt(random.Next(vertices.Count()));
                    IVertex b = vertices.ElementAt(random.Next(vertices.Count()));
                    if (a != b)
                        edges.Add(graph.AddEdge(null, a, b, ConvertId(graph, string.Concat("a", Guid.NewGuid()))));
                }
                PrintPerformance(graph.ToString(), edgeCount, "edges added", StopWatch());
                StopWatch();
                int counter = 0;
                foreach (IEdge e in edges)
                {
                    counter = counter + 1;
                    graph.RemoveEdge(e);

                    if (graph.Features.SupportsEdgeIteration)
                        Assert.AreEqual(edges.Count() - counter, Count(graph.GetEdges()));

                    if (graph.Features.SupportsVertexIteration)
                        Assert.AreEqual(vertices.Count(), Count(graph.GetVertices()));
                }
                PrintPerformance(graph.ToString(), edgeCount, "edges deleted (with Count check on each delete)", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemovingVertices()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                const int vertexCount = 500;
                var vertices = new List<IVertex>();
                var edges = new List<IEdge>();

                StopWatch();
                for (int i = 0; i < vertexCount; i++)
                    vertices.Add(graph.AddVertex(null));

                PrintPerformance(graph.ToString(), vertexCount, "vertices added", StopWatch());

                StopWatch();
                for (int i = 0; i < vertexCount; i = i + 2)
                {
                    IVertex a = vertices.ElementAt(i);
                    IVertex b = vertices.ElementAt(i + 1);
                    edges.Add(graph.AddEdge(null, a, b, ConvertId(graph, string.Concat("a", Guid.NewGuid()))));

                }
                PrintPerformance(graph.ToString(), vertexCount/2, "edges added", StopWatch());

                StopWatch();
                int counter = 0;
                foreach (IVertex v in vertices)
                {
                    counter = counter + 1;
                    graph.RemoveVertex(v);
                    if ((counter + 1)%2 == 0)
                    {
                        if (graph.Features.SupportsEdgeIteration)
                            Assert.AreEqual(edges.Count() - ((counter + 1)/2), Count(graph.GetEdges()));
                    }

                    if (graph.Features.SupportsVertexIteration)
                        Assert.AreEqual(vertices.Count() - counter, Count(graph.GetVertices()));
                }
                PrintPerformance(graph.ToString(), vertexCount, "vertices deleted (with Count check on each delete)",
                                 StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestConnectivityPatterns()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                IVertex a = graph.AddVertex(ConvertId(graph, "1"));
                IVertex b = graph.AddVertex(ConvertId(graph, "2"));
                IVertex c = graph.AddVertex(ConvertId(graph, "3"));
                IVertex d = graph.AddVertex(ConvertId(graph, "4"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(4, Count(graph.GetVertices()));

                graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
                graph.AddEdge(null, b, c, ConvertId(graph, "knows"));
                graph.AddEdge(null, c, d, ConvertId(graph, "knows"));
                graph.AddEdge(null, d, a, ConvertId(graph, "knows"));

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(4, Count(graph.GetEdges()));

                if (graph.Features.SupportsVertexIteration)
                {
                    foreach (IVertex v in graph.GetVertices())
                    {
                        Assert.AreEqual(1, Count(v.GetEdges(Direction.Out)));
                        Assert.AreEqual(1, Count(v.GetEdges(Direction.In)));
                    }
                }

                if (graph.Features.SupportsEdgeIteration)
                {
                    foreach (IEdge x in graph.GetEdges())
                        Assert.AreEqual(ConvertId(graph, "knows"), x.Label);
                }

                if (!graph.Features.IgnoresSuppliedIds)
                {
                    a = graph.GetVertex(ConvertId(graph, "1"));
                    b = graph.GetVertex(ConvertId(graph, "2"));
                    c = graph.GetVertex(ConvertId(graph, "3"));
                    d = graph.GetVertex(ConvertId(graph, "4"));

                    Assert.AreEqual(1, Count(a.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(a.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(b.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(b.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(c.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(c.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(d.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(d.GetEdges(Direction.Out)));

                    IEdge i = graph.AddEdge(null, a, b, ConvertId(graph, "hates"));

                    Assert.AreEqual(1, Count(a.GetEdges(Direction.In)));
                    Assert.AreEqual(2, Count(a.GetEdges(Direction.Out)));
                    Assert.AreEqual(2, Count(b.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(b.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(c.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(c.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(d.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(d.GetEdges(Direction.Out)));

                    Assert.AreEqual(1, Count(a.GetEdges(Direction.In)));
                    Assert.AreEqual(2, Count(a.GetEdges(Direction.Out)));
                    foreach (IEdge x in a.GetEdges(Direction.Out))
                        Assert.True(x.Label == ConvertId(graph, "knows") ||
                                    x.Label == ConvertId(graph, "hates"));

                    Assert.AreEqual(ConvertId(graph, "hates"), i.Label);
                    Assert.AreEqual(i.GetVertex(Direction.In).Id.ToString(), ConvertId(graph, "2"));
                    Assert.AreEqual(i.GetVertex(Direction.Out).Id.ToString(), ConvertId(graph, "1"));
                }

                var vertexIds = new HashSet<object>
                    {
                        a.Id,
                        a.Id,
                        b.Id,
                        b.Id,
                        c.Id,
                        d.Id,
                        d.Id,
                        d.Id
                    };
                Assert.AreEqual(4, vertexIds.Count());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestVertexEdgeLabels()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                IVertex a = graph.AddVertex(null);
                IVertex b = graph.AddVertex(null);
                IVertex c = graph.AddVertex(null);
                IEdge aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
                IEdge aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
                IEdge aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
                IEdge cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
                IEdge cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));

                IEnumerable<IEdge> results = a.GetEdges(Direction.Out).ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));

                results = a.GetEdges(Direction.Out, ConvertId(graph, "friend")).ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));

                results = a.GetEdges(Direction.Out, ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aHateC));

                results = a.GetEdges(Direction.In, ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));

                results = a.GetEdges(Direction.In, ConvertId(graph, "friend")).ToArray();
                Assert.AreEqual(results.Count(), 0);

                results = b.GetEdges(Direction.In, ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateB));

                results = b.GetEdges(Direction.In, ConvertId(graph, "friend")).ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestVertexEdgeLabels2()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var a = graph.AddVertex(null);
                var b = graph.AddVertex(null);
                var c = graph.AddVertex(null);
                var aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
                var aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
                var aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
                var cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
                var cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));

                var results = a.GetEdges(Direction.Out, ConvertId(graph, "friend"),
                                                        ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));

                results = a.GetEdges(Direction.In, ConvertId(graph, "friend"), ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));

                results = b.GetEdges(Direction.In, ConvertId(graph, "friend"), ConvertId(graph, "hate")).ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(cHateB));

                results = b.GetEdges(Direction.In, ConvertId(graph, "blah"), ConvertId(graph, "blah2"),
                                     ConvertId(graph, "blah3")).ToArray();
                Assert.AreEqual(results.Count(), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestTreeConnectivity()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                StopWatch();
                const int branchSize = 11;
                IVertex start = graph.AddVertex(null);
                for (int i = 0; i < branchSize; i++)
                {
                    IVertex a = graph.AddVertex(null);
                    graph.AddEdge(null, start, a, ConvertId(graph, "test1"));
                    for (int j = 0; j < branchSize; j++)
                    {
                        IVertex b = graph.AddVertex(null);
                        graph.AddEdge(null, a, b, ConvertId(graph, "test2"));
                        for (int k = 0; k < branchSize; k++)
                        {
                            IVertex c = graph.AddVertex(null);
                            graph.AddEdge(null, b, c, ConvertId(graph, "test3"));
                        }
                    }
                }

                Assert.AreEqual(0, Count(start.GetEdges(Direction.In)));
                Assert.AreEqual(branchSize, Count(start.GetEdges(Direction.Out)));
                foreach (IEdge e in start.GetEdges(Direction.Out))
                {
                    Assert.AreEqual(ConvertId(graph, "test1"), e.Label);
                    Assert.AreEqual(branchSize, Count(e.GetVertex(Direction.In).GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(e.GetVertex(Direction.In).GetEdges(Direction.In)));
                    foreach (IEdge f in e.GetVertex(Direction.In).GetEdges(Direction.Out))
                    {
                        Assert.AreEqual(ConvertId(graph, "test2"), f.Label);
                        Assert.AreEqual(branchSize, Count(f.GetVertex(Direction.In).GetEdges(Direction.Out)));
                        Assert.AreEqual(1, Count(f.GetVertex(Direction.In).GetEdges(Direction.In)));
                        foreach (IEdge g in f.GetVertex(Direction.In).GetEdges(Direction.Out))
                        {
                            Assert.AreEqual(ConvertId(graph, "test3"), g.Label);
                            Assert.AreEqual(0, Count(g.GetVertex(Direction.In).GetEdges(Direction.Out)));
                            Assert.AreEqual(1, Count(g.GetVertex(Direction.In).GetEdges(Direction.In)));
                        }
                    }
                }

                int totalVertices = 0;
                for (int i = 0; i < 4; i++)
                    totalVertices = totalVertices + (int) Math.Pow(branchSize, i);

                PrintPerformance(graph.ToString(), totalVertices, "vertices added in a tree structure", StopWatch());

                if (graph.Features.SupportsVertexIteration)
                {
                    StopWatch();
                    var vertices = new HashSet<IVertex>();
                    foreach (IVertex v in graph.GetVertices())
                        vertices.Add(v);

                    Assert.AreEqual(totalVertices, vertices.Count());
                    PrintPerformance(graph.ToString(), totalVertices, "vertices iterated", StopWatch());
                }

                if (graph.Features.SupportsEdgeIteration)
                {
                    StopWatch();
                    var edges = new HashSet<IEdge>();
                    foreach (IEdge e in graph.GetEdges())
                        edges.Add(e);

                    Assert.AreEqual(totalVertices - 1, edges.Count());
                    PrintPerformance(graph.ToString(), totalVertices - 1, "edges iterated", StopWatch());
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestConcurrentModification()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                IVertex a = graph.AddVertex(null);
                graph.AddVertex(null);
                graph.AddVertex(null);

                if (graph.Features.SupportsVertexIteration)
                {
                    foreach (IVertex vertex in graph.GetVertices())
                    {
                        graph.AddEdge(null, vertex, a, ConvertId(graph, "x"));
                        graph.AddEdge(null, vertex, a, ConvertId(graph, "y"));
                    }
                    foreach (IVertex vertex in graph.GetVertices())
                    {
                        Assert.AreEqual(Count(vertex.GetEdges(Direction.Out)), 2);
                        foreach (IEdge edge in vertex.GetEdges(Direction.Out))
                            graph.RemoveEdge(edge);
                    }
                    foreach (IVertex vertex in graph.GetVertices())
                        graph.RemoveVertex(vertex);
                }
                else
                {
                    if (graph.Features.SupportsEdgeIteration)
                    {
                        for (int i = 0; i < 10; i++)
                            graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));

                        foreach (IEdge edge in graph.GetEdges())
                            graph.RemoveEdge(edge);
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGraphDataPersists()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IsPersistent)
                {
                    IVertex v = graph.AddVertex(null);
                    IVertex u = graph.AddVertex(null);

                    if (graph.Features.SupportsVertexProperties)
                    {
                        v.SetProperty("name", "marko");
                        u.SetProperty("name", "pavel");
                    }
                    IEdge e = graph.AddEdge(null, v, u, ConvertId(graph, "collaborator"));

                    if (graph.Features.SupportsEdgeProperties)
                        e.SetProperty("location", "internet");

                    if (graph.Features.SupportsVertexIteration)
                        Assert.AreEqual(Count(graph.GetVertices()), 2);

                    if (graph.Features.SupportsEdgeIteration)
                        Assert.AreEqual(Count(graph.GetEdges()), 1);
                }
            }
            finally
            {
                graph.Shutdown();
            }

            StopWatch();

            graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IsPersistent)
                {
                    PrintPerformance(graph.ToString(), 1, "graph loaded", StopWatch());

                    if (graph.Features.SupportsVertexIteration)
                    {
                        Assert.AreEqual(Count(graph.GetVertices()), 2);
                        if (graph.Features.SupportsVertexProperties)
                        {
                            foreach (IVertex vertex in graph.GetVertices())
                                Assert.True(((string) vertex.GetProperty("name")) == "marko" ||
                                            ((string) vertex.GetProperty("name")) == "pavel");
                        }
                    }

                    if (graph.Features.SupportsEdgeIteration)
                    {
                        Assert.AreEqual(Count(graph.GetEdges()), 1);
                        foreach (IEdge edge in graph.GetEdges())
                        {
                            Assert.AreEqual(edge.Label, ConvertId(graph, "collaborator"));
                            if (graph.Features.SupportsEdgeProperties)
                                Assert.AreEqual(edge.GetProperty("location"), "internet");
                        }
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAutotypingOfProperties()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsVertexProperties)
                {
                    IVertex v = graph.AddVertex(null);
                    v.SetProperty(ConvertId(graph, "string"), "marko");
                    v.SetProperty(ConvertId(graph, "integer"), 33);
                    v.SetProperty(ConvertId(graph, "boolean"), true);

                    var name = v.GetProperty(ConvertId(graph, "string"));
                    Assert.AreEqual(name, "marko");
                    var age = v.GetProperty(ConvertId(graph, "integer"));
                    Assert.AreEqual(age, 33);
                    var best = (bool) v.GetProperty(ConvertId(graph, "boolean"));
                    Assert.True(best);

                    name = v.RemoveProperty(ConvertId(graph, "string"));
                    Assert.AreEqual(name, "marko");
                    age = v.RemoveProperty(ConvertId(graph, "integer"));
                    Assert.AreEqual(age, 33);
                    best = (bool) v.RemoveProperty(ConvertId(graph, "boolean"));
                    Assert.True(best);
                }

                if (graph.Features.SupportsEdgeProperties)
                {
                    IEdge e = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                    e.SetProperty(ConvertId(graph, "string"), "friend");
                    e.SetProperty(ConvertId(graph, "double"), 1.0d);

                    var type = (string) e.GetProperty(ConvertId(graph, "string"));
                    Assert.AreEqual(type, "friend");
                    var weight = (double) e.GetProperty(ConvertId(graph, "double"));
                    Assert.AreEqual(weight, 1.0d);

                    type = (string) e.RemoveProperty(ConvertId(graph, "string"));
                    Assert.AreEqual(type, "friend");
                    weight = (double) e.RemoveProperty(ConvertId(graph, "double"));
                    Assert.AreEqual(weight, 1.0d);
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}
