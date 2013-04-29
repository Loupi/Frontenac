using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints
{
    public abstract class EdgeTestSuite : TestSuite
    {
        protected EdgeTestSuite(GraphTest graphTest)
            : base("EdgeTestSuite", graphTest)
        {

        }

        [Test]
        public void TestEdgeEquality()
        {
            var graph = GraphTest.GenerateGraph();

            var v = graph.AddVertex(ConvertId(graph, "1"));
            var u = graph.AddVertex(ConvertId(graph, "2"));
            var e = graph.AddEdge(null, v, u, ConvertId(graph, "knows"));
            Assert.AreEqual(e.GetLabel(), ConvertId(graph, "knows"));
            Assert.AreEqual(e.GetVertex(Direction.In), u);
            Assert.AreEqual(e.GetVertex(Direction.Out), v);
            Assert.AreEqual(e, v.GetEdges(Direction.Out).First());
            Assert.AreEqual(e, u.GetEdges(Direction.In).First());
            Assert.AreEqual(v.GetEdges(Direction.Out).First(), u.GetEdges(Direction.In).First());
            var set = new HashSet<IEdge>
            {
                e,
                e,
                v.GetEdges(Direction.Out).First(),
                v.GetEdges(Direction.Out).First(),
                u.GetEdges(Direction.In).First(),
                u.GetEdges(Direction.In).First()
            };

            if (graph.GetFeatures().SupportsEdgeIteration)
                set.Add(graph.GetEdges().First());
            Assert.AreEqual(set.Count(), 1);
            graph.Shutdown();
        }

        [Test]
        public void TestAddEdges()
        {
            var graph = GraphTest.GenerateGraph();

            StopWatch();
            var v1 = graph.AddVertex(ConvertId(graph, "1"));
            var v2 = graph.AddVertex(ConvertId(graph, "2"));
            var v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "caresFor"));
            Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.In)));
            PrintPerformance(graph.ToString(), 6, "elements added and checked", StopWatch());
            graph.Shutdown();
        }

        [Test]
        public void TestAddManyEdges()
        {
            var graph = GraphTest.GenerateGraph();
            const int edgeCount = 100;
            const int vertexCount = 200;
            long counter = 0;
            StopWatch();
            for (int i = 0; i < edgeCount; i++)
            {
                IVertex out_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                IVertex in_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                graph.AddEdge(null, out_, in_, ConvertId(graph, Guid.NewGuid().ToString()));
            }
            PrintPerformance(graph.ToString(), vertexCount + edgeCount, "elements added", StopWatch());

            if (graph.GetFeatures().SupportsEdgeIteration)
            {
                StopWatch();
                Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                PrintPerformance(graph.ToString(), edgeCount, "edges counted", StopWatch());
            }

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), vertexCount, "vertices counted", StopWatch());
                StopWatch();
                foreach (IVertex vertex in graph.GetVertices())
                {
                    if (Count(vertex.GetEdges(Direction.Out)) > 0) {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.Out)));
                        Assert.False(Count(vertex.GetEdges(Direction.In)) > 0);

                    } else {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.In)));
                        Assert.False(Count(vertex.GetEdges(Direction.Out)) > 0);
                    }
                }
                PrintPerformance(graph.ToString(), vertexCount, "vertices checked", StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGetEdges()
        {
            var graph = GraphTest.GenerateGraph();
            var v1 = graph.AddVertex(null);
            var v2 = graph.AddVertex(null);
            var v3 = graph.AddVertex(null);

            var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test1"));
            var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test2"));
            var e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test3"));

            if (graph.GetFeatures().SupportsEdgeRetrieval)
            {
                StopWatch();
                Assert.AreEqual(graph.GetEdge(e1.GetId()), e1);
                Assert.AreEqual(graph.GetEdge(e1.GetId()).GetVertex(Direction.In), v2);
                Assert.AreEqual(graph.GetEdge(e1.GetId()).GetVertex(Direction.Out), v1);

                Assert.AreEqual(graph.GetEdge(e2.GetId()), e2);
                Assert.AreEqual(graph.GetEdge(e2.GetId()).GetVertex(Direction.In), v3);
                Assert.AreEqual(graph.GetEdge(e2.GetId()).GetVertex(Direction.Out), v2);

                Assert.AreEqual(graph.GetEdge(e3.GetId()), e3);
                Assert.AreEqual(graph.GetEdge(e3.GetId()).GetVertex(Direction.In), v1);
                Assert.AreEqual(graph.GetEdge(e3.GetId()).GetVertex(Direction.Out), v3);

                PrintPerformance(graph.ToString(), 3, "edges retrieved", StopWatch());
            }

            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.Out)), e1);
            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.Out)).GetVertex(Direction.In), v2);
            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.Out)).GetVertex(Direction.Out), v1);

            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.Out)), e2);
            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.Out)).GetVertex(Direction.In), v3);
            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.Out)).GetVertex(Direction.Out), v2);

            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.Out)), e3);
            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.Out)).GetVertex(Direction.In), v1);
            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.Out)).GetVertex(Direction.Out), v3);

            graph.Shutdown();
        }

        [Test]
        public void TestGetNonExistantEdges()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeRetrieval)
            {
                try
                {
                    graph.GetEdge(null);
                    Assert.Fail("Getting an element with a null identifier must throw IllegalArgumentException");
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }

                Assert.Null(graph.GetEdge("asbv"));
                Assert.Null(graph.GetEdge(12.0d));
            }

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveManyEdges() 
        {
            var graph = GraphTest.GenerateGraph();
            long counter = 200000;
            const int edgeCount = 10;
            var edges = new HashSet<IEdge>();
            for (int i = 0; i < edgeCount; i++)
            {
                var out_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                var in_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                edges.Add(graph.AddEdge(null, out_, in_, ConvertId(graph, "a" + Guid.NewGuid().ToString())));
            }
            Assert.AreEqual(edgeCount, edges.Count());

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                Assert.AreEqual(edgeCount * 2, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), edgeCount * 2, "vertices counted", StopWatch());
            }

            if (graph.GetFeatures().SupportsEdgeIteration)
            {
                StopWatch();
                Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                PrintPerformance(graph.ToString(), edgeCount, "edges counted", StopWatch());

                int i = edgeCount;
                StopWatch();
                foreach (var edge in edges)
                {
                    graph.RemoveEdge(edge);
                    i--;
                    Assert.AreEqual(i, Count(graph.GetEdges()));
                    if (graph.GetFeatures().SupportsVertexIteration)
                    {
                        int x = 0;
                        foreach (var vertex in graph.GetVertices())
                        {
                            if (Count(vertex.GetEdges(Direction.Out)) > 0)
                            {
                                Assert.AreEqual(1, Count(vertex.GetEdges(Direction.Out)));
                                Assert.False(Count(vertex.GetEdges(Direction.In)) > 0);
                            }
                            else if (Count(vertex.GetEdges(Direction.In)) > 0)
                            {
                                Assert.AreEqual(1, Count(vertex.GetEdges(Direction.In)));
                                Assert.False(Count(vertex.GetEdges(Direction.Out)) > 0);
                            }
                            else
                            {
                                x++;
                            }
                        }
                        Assert.AreEqual((edgeCount - i) * 2, x);
                    }
                }
                PrintPerformance(graph.ToString(), edgeCount, "edges removed and graph checked", StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddingDuplicateEdges()
        {
            var graph = GraphTest.GenerateGraph();

            var v1 = graph.AddVertex(ConvertId(graph, "1"));
            var v2 = graph.AddVertex(ConvertId(graph, "2"));
            var v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));

            if (graph.GetFeatures().SupportsDuplicateEdges)
            {
                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                if (graph.GetFeatures().SupportsEdgeIteration)
                    Assert.AreEqual(5, Count(graph.GetEdges()));

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                Assert.AreEqual(4, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(4, Count(v3.GetEdges(Direction.In)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            }
            else
            {
                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 3);

                if (graph.GetFeatures().SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 2);

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v3.GetEdges(Direction.In)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveEdgesByRemovingVertex()
        {
            var graph = GraphTest.GenerateGraph();

            var v1 = graph.AddVertex(ConvertId(graph, "1"));
            var v2 = graph.AddVertex(ConvertId(graph, "2"));
            var v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));

            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            }

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(3, Count(graph.GetVertices()));

            graph.RemoveVertex(v1);

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(2, Count(graph.GetVertices()));

            Assert.AreEqual(graph.GetFeatures().SupportsDuplicateEdges ? 2 : 1, Count(v2.GetEdges(Direction.Out)));

            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));

            Assert.AreEqual(graph.GetFeatures().SupportsDuplicateEdges ? 2 : 1, Count(v3.GetEdges(Direction.In)));

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveEdges()
        {
            var graph = GraphTest.GenerateGraph();
            var v1 = graph.AddVertex(ConvertId(graph, "1"));
            var v2 = graph.AddVertex(ConvertId(graph, "2"));
            var v3 = graph.AddVertex(ConvertId(graph, "3"));
            var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            var e3 = graph.AddEdge(null, v2, v3, ConvertId(graph, "cares_for"));

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(3, Count(graph.GetVertices()));

            graph.RemoveEdge(e1);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.In)));

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.In)));

            graph.RemoveEdge(e2);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(1, Count(v3.GetEdges(Direction.In)));

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(1, Count(v3.GetEdges(Direction.In)));

            graph.RemoveEdge(e3);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.In)));

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.In)));

            graph.Shutdown();
        }

        [Test]
        public void TestAddingSelfLoops()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsSelfLoops)
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                if (graph.GetFeatures().SupportsEdgeIteration)
                {
                    Assert.AreEqual(3, Count(graph.GetEdges()));
                    var counter = 0;
                    foreach (var edge in graph.GetEdges())
                    {
                        counter++;
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).GetId(), edge.GetVertex(Direction.Out).GetId());
                    }
                    Assert.AreEqual(counter, 3);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveSelfLoops()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsSelfLoops)
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                var e2 = graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                if (graph.GetFeatures().SupportsEdgeIteration)
                {
                    Assert.AreEqual(3, Count(graph.GetEdges()));
                    foreach (IEdge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).GetId(), edge.GetVertex(Direction.Out).GetId());
                    }
                }

                graph.RemoveVertex(v1);

                if (graph.GetFeatures().SupportsEdgeIteration)
                {
                    Assert.AreEqual(2, Count(graph.GetEdges()));
                    foreach (IEdge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).GetId(), edge.GetVertex(Direction.Out).GetId());
                    }
                }

                Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                graph.RemoveEdge(e2);
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));

                if (graph.GetFeatures().SupportsEdgeIteration)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 1);
                    foreach (IEdge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).GetId(), edge.GetVertex(Direction.Out).GetId());
                    }
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestEdgeIterator()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeIteration)
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test"));
                var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test"));
                var e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test"));

                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));
                
                Assert.AreEqual(3, Count(graph.GetEdges()));

                var edgeIds = new HashSet<string>();
                int count = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    count++;
                    edgeIds.Add(e.GetId().ToString());
                    Assert.AreEqual(ConvertId(graph, "test"), e.GetLabel());
                    if (e.GetId().ToString().Equals(e1.GetId().ToString()))
                    {
                        Assert.AreEqual(v1, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v2, e.GetVertex(Direction.In));
                    }
                    else if (e.GetId().ToString().Equals(e2.GetId().ToString()))
                    {
                        Assert.AreEqual(v2, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v3, e.GetVertex(Direction.In));
                    }
                    else if (e.GetId().ToString().Equals(e3.GetId().ToString()))
                    {
                        Assert.AreEqual(v3, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v1, e.GetVertex(Direction.In));
                    }
                    else
                        Assert.True(false);
                    //System.out.println(e);
                }
                Assert.AreEqual(3, count);
                Assert.AreEqual(3, edgeIds.Count());
                Assert.True(edgeIds.Contains(e1.GetId().ToString()));
                Assert.True(edgeIds.Contains(e2.GetId().ToString()));
                Assert.True(edgeIds.Contains(e3.GetId().ToString()));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddingRemovingEdgeProperties()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeProperties)
            {
                var a = graph.AddVertex(ConvertId(graph, "1"));
                var b = graph.AddVertex(ConvertId(graph, "2"));
                var edge = graph.AddEdge(ConvertId(graph, "3"), a, b, "knows");
                Assert.AreEqual(edge.GetPropertyKeys().Count(), 0);
                Assert.Null(edge.GetProperty("weight"));

                if (graph.GetFeatures().SupportsDoubleProperty)
                {
                    edge.SetProperty("weight", 0.5);
                    Assert.AreEqual(edge.GetPropertyKeys().Count(), 1);
                    Assert.AreEqual(edge.GetProperty("weight"), 0.5);

                    edge.SetProperty("weight", 0.6);
                    Assert.AreEqual(edge.GetPropertyKeys().Count(), 1);
                    Assert.AreEqual(edge.GetProperty("weight"), 0.6);
                    Assert.AreEqual(edge.RemoveProperty("weight"), 0.6);
                    Assert.Null(edge.GetProperty("weight"));
                    Assert.AreEqual(edge.GetPropertyKeys().Count(), 0);
                }

                if (graph.GetFeatures().SupportsStringProperty)
                {
                    edge.SetProperty("blah", "marko");
                    edge.SetProperty("blah2", "josh");
                    Assert.AreEqual(edge.GetPropertyKeys().Count(), 2);
                }
            }

            graph.Shutdown();
        }

        [Test]
        public void TestAddingLabelAndIdProperty()
        {
            var graph = GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the id or label properties.
            if (graph.GetFeatures().SupportsEdgeProperties)
            {

                var edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                try
                {
                    edge.SetProperty("id", "123");
                    Assert.Fail();
                }
                catch
                {
                }
                try
                {
                    edge.SetProperty("label", "hates");
                    Assert.Fail();
                }
                catch
                {
                }

            }
            graph.Shutdown();
        }

        [Test]
        public void TestNoConcurrentModificationException()
        {
            var graph = GraphTest.GenerateGraph();
            for (int i = 0; i < 25; i++) {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
            }

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 50);

            if (graph.GetFeatures().SupportsEdgeIteration)
            {
                Assert.AreEqual(Count(graph.GetEdges()), 25);
                foreach (IEdge edge in graph.GetEdges())
                    graph.RemoveEdge(edge);
                
                Assert.AreEqual(Count(graph.GetEdges()), 0);
            }

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 50);

            graph.Shutdown();
        }

        [Test]
        public void TestEmptyKeyProperty()
        {
            var graph = GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.GetFeatures().SupportsEdgeProperties)
            {
                IEdge e = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "friend");
                try
                {
                    e.SetProperty("", "value");
                    Assert.Fail();
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestEdgeCentricRemoving()
        {
            var graph = GraphTest.GenerateGraph();

            var a = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
            var b = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
            var c = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));

            object cId = c.GetId();

            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 3);

            a.Remove();
            b.Remove();

            if (graph.GetFeatures().SupportsEdgeRetrieval)
                Assert.NotNull(graph.GetEdge(cId));

            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestSettingBadVertexProperties()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexProperties)
            {
                var edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                try
                {
                    edge.SetProperty(null, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.SetProperty("", -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.SetProperty(StringFactory.Id, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.SetProperty(StringFactory.Label, "friend");
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.SetProperty(ConvertId(graph, "good"), null);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }
            }
            graph.Shutdown();
        }
    }
}
