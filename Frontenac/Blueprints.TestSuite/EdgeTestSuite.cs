using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util;
using NUnit.Framework;

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
            try
            {
                var v = graph.AddVertex(ConvertId(graph, "1"));
                var u = graph.AddVertex(ConvertId(graph, "2"));
                var e = graph.AddEdge(null, v, u, ConvertId(graph, "knows"));
                Assert.AreEqual(e.Label, ConvertId(graph, "knows"));
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

                if (graph.Features.SupportsEdgeIteration)
                    set.Add(graph.GetEdges().First());
                Assert.AreEqual(set.Count(), 1);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
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
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddManyEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                const int edgeCount = 100;
                const int vertexCount = 200;
                long counter = 0;
                StopWatch();
                for (var i = 0; i < edgeCount; i++)
                {
                    var out_ = graph.AddVertex(null);
                    var in_ = graph.AddVertex(null);
                    graph.AddEdge(null, out_, in_, ConvertId(graph, Guid.NewGuid().ToString()));
                }
                PrintPerformance(graph.ToString(), vertexCount + edgeCount, "elements added", StopWatch());

                if (graph.Features.SupportsEdgeIteration)
                {
                    StopWatch();
                    Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                    PrintPerformance(graph.ToString(), edgeCount, "edges counted", StopWatch());
                }

                if (!graph.Features.SupportsVertexIteration) return;

                StopWatch();
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), vertexCount, "vertices counted", StopWatch());
                StopWatch();

                foreach (var vertex in graph.GetVertices())
                {
                    if (Count(vertex.GetEdges(Direction.Out)) > 0)
                    {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.Out)));
                        Assert.False(Count(vertex.GetEdges(Direction.In)) > 0);
                    }
                    else
                    {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.In)));
                        Assert.False(Count(vertex.GetEdges(Direction.Out)) > 0);
                    }
                }
                PrintPerformance(graph.ToString(), vertexCount, "vertices checked", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGetEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var v1 = graph.AddVertex(null);
                var v2 = graph.AddVertex(null);
                var v3 = graph.AddVertex(null);

                var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test1"));
                var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test2"));
                var e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test3"));

                if (graph.Features.SupportsEdgeRetrieval)
                {
                    StopWatch();
                    Assert.AreEqual(graph.GetEdge(e1.Id), e1);
                    Assert.AreEqual(graph.GetEdge(e1.Id).GetVertex(Direction.In), v2);
                    Assert.AreEqual(graph.GetEdge(e1.Id).GetVertex(Direction.Out), v1);

                    Assert.AreEqual(graph.GetEdge(e2.Id), e2);
                    Assert.AreEqual(graph.GetEdge(e2.Id).GetVertex(Direction.In), v3);
                    Assert.AreEqual(graph.GetEdge(e2.Id).GetVertex(Direction.Out), v2);

                    Assert.AreEqual(graph.GetEdge(e3.Id), e3);
                    Assert.AreEqual(graph.GetEdge(e3.Id).GetVertex(Direction.In), v1);
                    Assert.AreEqual(graph.GetEdge(e3.Id).GetVertex(Direction.Out), v3);

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
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGetNonExistantEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeRetrieval) return;

                try
                {
                    graph.GetEdge(null);
                    Assert.Fail();
                }
                catch (Exception x)
                {
                    if (x.GetType().FullName != GraphHelpers.ContractExceptionName)
                    {
                        throw;
                    }
                }

                Assert.Null(graph.GetEdge("asbv"));
                Assert.Null(graph.GetEdge(12.0d));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemoveManyEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var counter = 200000;
                const int edgeCount = 10;
                var edges = new HashSet<IEdge>();
                for (var i = 0; i < edgeCount; i++)
                {
                    var out_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                    var in_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                    edges.Add(graph.AddEdge(null, out_, in_, ConvertId(graph, "a" + Guid.NewGuid().ToString())));
                }
                Assert.AreEqual(edgeCount, edges.Count());

                if (graph.Features.SupportsVertexIteration)
                {
                    StopWatch();
                    Assert.AreEqual(edgeCount*2, Count(graph.GetVertices()));
                    PrintPerformance(graph.ToString(), edgeCount*2, "vertices counted", StopWatch());
                }

                if (!graph.Features.SupportsEdgeIteration) return;

                StopWatch();
                Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                PrintPerformance(graph.ToString(), edgeCount, "edges counted", StopWatch());

                var edgeCountdown = edgeCount;
                StopWatch();
                foreach (var edge in edges)
                {
                    graph.RemoveEdge(edge);
                    edgeCountdown--;
                    Assert.AreEqual(edgeCountdown, Count(graph.GetEdges()));
                    if (!graph.Features.SupportsVertexIteration) continue;

                    var x = 0;
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
                    Assert.AreEqual((edgeCount - edgeCountdown)*2, x);
                }
                PrintPerformance(graph.ToString(), edgeCount, "edges removed and InnerGraph checked", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddingDuplicateEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
                graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
                graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
                graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
                graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));

                if (graph.Features.SupportsDuplicateEdges)
                {
                    if (graph.Features.SupportsVertexIteration)
                        Assert.AreEqual(3, Count(graph.GetVertices()));

                    if (graph.Features.SupportsEdgeIteration)
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
                    if (graph.Features.SupportsVertexIteration)
                        Assert.AreEqual(Count(graph.GetVertices()), 3);

                    if (graph.Features.SupportsEdgeIteration)
                        Assert.AreEqual(Count(graph.GetEdges()), 2);

                    Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(v3.GetEdges(Direction.In)));
                    Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemoveEdgesByRemovingVertex()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
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

                if (!graph.Features.IgnoresSuppliedIds)
                {
                    v1 = graph.GetVertex(ConvertId(graph, "1"));
                    v2 = graph.GetVertex(ConvertId(graph, "2"));
                    v3 = graph.GetVertex(ConvertId(graph, "3"));

                    Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                    Assert.AreEqual(1, Count(v1.GetEdges(Direction.Out)));
                    Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                    Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
                }

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                graph.RemoveVertex(v1);

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(2, Count(graph.GetVertices()));

                Assert.AreEqual(graph.Features.SupportsDuplicateEdges ? 2 : 1, Count(v2.GetEdges(Direction.Out)));

                Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));

                Assert.AreEqual(graph.Features.SupportsDuplicateEdges ? 2 : 1, Count(v3.GetEdges(Direction.In)));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemoveEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
                var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
                var e3 = graph.AddEdge(null, v2, v3, ConvertId(graph, "cares_for"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                graph.RemoveEdge(e1);
                Assert.AreEqual(0, Count(v1.GetEdges(Direction.Out)));
                Assert.AreEqual(2, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.Out)));
                Assert.AreEqual(0, Count(v1.GetEdges(Direction.In)));
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));
                Assert.AreEqual(2, Count(v3.GetEdges(Direction.In)));

                if (!graph.Features.IgnoresSuppliedIds)
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

                if (!graph.Features.IgnoresSuppliedIds)
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

                if (!graph.Features.IgnoresSuppliedIds)
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
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddingSelfLoops()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsSelfLoops) return;

                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                if (!graph.Features.SupportsEdgeIteration) return;

                Assert.AreEqual(3, Count(graph.GetEdges()));
                var counter = 0;
                foreach (var edge in graph.GetEdges())
                {
                    counter++;
                    Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                    Assert.AreEqual(edge.GetVertex(Direction.In).Id, edge.GetVertex(Direction.Out).Id);
                }
                Assert.AreEqual(counter, 3);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestRemoveSelfLoops()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsSelfLoops) return;

                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                var e2 = graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                if (graph.Features.SupportsEdgeIteration)
                {
                    Assert.AreEqual(3, Count(graph.GetEdges()));
                    foreach (var edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).Id, edge.GetVertex(Direction.Out).Id);
                    }
                }

                graph.RemoveVertex(v1);

                if (graph.Features.SupportsEdgeIteration)
                {
                    Assert.AreEqual(2, Count(graph.GetEdges()));
                    foreach (var edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).Id, edge.GetVertex(Direction.Out).Id);
                    }
                }

                Assert.AreEqual(1, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.In)));
                graph.RemoveEdge(e2);
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.Out)));
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.In)));

                if (graph.Features.SupportsEdgeIteration)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 1);
                    foreach (var edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out));
                        Assert.AreEqual(edge.GetVertex(Direction.In).Id, edge.GetVertex(Direction.Out).Id);
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestEdgeIterator()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIteration) return;

                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));
                var v3 = graph.AddVertex(ConvertId(graph, "3"));
                var e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test"));
                var e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test"));
                var e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test"));

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(3, Count(graph.GetVertices()));

                Assert.AreEqual(3, Count(graph.GetEdges()));

                var edgeIds = new HashSet<string>();
                var count = 0;
                foreach (var e in graph.GetEdges())
                {
                    count++;
                    edgeIds.Add(e.Id.ToString());
                    Assert.AreEqual(ConvertId(graph, "test"), e.Label);
                    if (e.Id.ToString().Equals(e1.Id.ToString()))
                    {
                        Assert.AreEqual(v1, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v2, e.GetVertex(Direction.In));
                    }
                    else if (e.Id.ToString().Equals(e2.Id.ToString()))
                    {
                        Assert.AreEqual(v2, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v3, e.GetVertex(Direction.In));
                    }
                    else if (e.Id.ToString().Equals(e3.Id.ToString()))
                    {
                        Assert.AreEqual(v3, e.GetVertex(Direction.Out));
                        Assert.AreEqual(v1, e.GetVertex(Direction.In));
                    }
                    else
                        Assert.True(false);
                }
                Assert.AreEqual(3, count);
                Assert.AreEqual(3, edgeIds.Count());
                Assert.True(edgeIds.Contains(e1.Id.ToString()));
                Assert.True(edgeIds.Contains(e2.Id.ToString()));
                Assert.True(edgeIds.Contains(e3.Id.ToString()));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddingRemovingEdgeProperties()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeProperties) return;

                var a = graph.AddVertex(ConvertId(graph, "1"));
                var b = graph.AddVertex(ConvertId(graph, "2"));
                var edge = graph.AddEdge(ConvertId(graph, "3"), a, b, "knows");
                Assert.AreEqual(edge.GetPropertyKeys().Count(), 0);
                Assert.Null(edge.GetProperty("weight"));

                if (graph.Features.SupportsDoubleProperty)
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

                if (graph.Features.SupportsStringProperty)
                {
                    edge.SetProperty("blah", "marko");
                    edge.SetProperty("blah2", "josh");
                    Assert.AreEqual(edge.GetPropertyKeys().Count(), 2);
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAddingLabelAndIdProperty()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                // no point in testing InnerGraph features for setting string properties because the intent is for it to
                // fail based on the id or label properties.
                if (!graph.Features.SupportsEdgeProperties) return;

                var edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                if (!graph.Features.SupportsIdProperty)
                {
                    try
                    {
                        edge.SetProperty("id", "123");
                        Assert.Fail();
                    }
                    catch
                    {
                        Assert.True(true);
                    }
                }

                if (!graph.Features.SupportsLabelProperty)
                {
                    try
                    {
                        edge.SetProperty("label", "hates");
                        Assert.Fail();
                    }
                    catch
                    {
                        Assert.True(true);
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestNoConcurrentModificationException()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                for (var i = 0; i < 25; i++)
                {
                    graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
                }

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 50);

                if (graph.Features.SupportsEdgeIteration)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 25);
                    foreach (var edge in graph.GetEdges())
                        graph.RemoveEdge(edge);

                    Assert.AreEqual(Count(graph.GetEdges()), 0);
                }

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 50);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestEmptyKeyProperty()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                // no point in testing InnerGraph features for setting string properties because the intent is for it to
                // fail based on the empty key.
                if (!graph.Features.SupportsEdgeProperties) return;

                var e = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "friend");
                try
                {
                    e.SetProperty("", "value");
                    Assert.Fail();
                }
                catch (Exception x)
                {
                    if (x.GetType().FullName != GraphHelpers.ContractExceptionName)
                    {
                        throw;
                    }
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestEdgeCentricRemoving()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                var a = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
                var b = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
                var c = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));

                var cId = c.Id;

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 3);

                a.Remove();
                b.Remove();

                if (graph.Features.SupportsEdgeRetrieval)
                    Assert.NotNull(graph.GetEdge(cId));

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 1);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestSettingBadVertexProperties()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexProperties) return;

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
            finally
            {
                graph.Shutdown();
            }
        }
    }
}