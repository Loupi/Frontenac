using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints
{
    public abstract class EdgeTestSuite : TestSuite
    {
        public EdgeTestSuite(GraphTest graphTest)
            : base("EdgeTestSuite", graphTest)
        {

        }

        [Test]
        public void TestEdgeEquality()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v = graph.AddVertex(ConvertId(graph, "1"));
            Vertex u = graph.AddVertex(ConvertId(graph, "2"));
            Edge e = graph.AddEdge(null, v, u, ConvertId(graph, "knows"));
            Assert.AreEqual(e.GetLabel(), ConvertId(graph, "knows"));
            Assert.AreEqual(e.GetVertex(Direction.IN), u);
            Assert.AreEqual(e.GetVertex(Direction.OUT), v);
            Assert.AreEqual(e, v.GetEdges(Direction.OUT).First());
            Assert.AreEqual(e, u.GetEdges(Direction.IN).First());
            Assert.AreEqual(v.GetEdges(Direction.OUT).First(), u.GetEdges(Direction.IN).First());
            HashSet<Edge> set = new HashSet<Edge>();
            set.Add(e);
            set.Add(e);
            set.Add(v.GetEdges(Direction.OUT).First());
            set.Add(v.GetEdges(Direction.OUT).First());
            set.Add(u.GetEdges(Direction.IN).First());
            set.Add(u.GetEdges(Direction.IN).First());
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                set.Add(graph.GetEdges().First());
            Assert.AreEqual(set.Count(), 1);
            graph.Shutdown();
        }

        [Test]
        public void TestAddEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();

            this.StopWatch();
            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "caresFor"));
            Assert.AreEqual(1, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.IN)));
            PrintPerformance(graph.ToString(), 6, "elements added and checked", this.StopWatch());
            graph.Shutdown();
        }

        [Test]
        public void TestAddManyEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            int edgeCount = 100;
            int vertexCount = 200;
            long counter = 0;
            this.StopWatch();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex out_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                Vertex in_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                graph.AddEdge(null, out_, in_, ConvertId(graph, Guid.NewGuid().ToString()));
            }
            PrintPerformance(graph.ToString(), vertexCount + edgeCount, "elements added", this.StopWatch());
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                this.StopWatch();
                Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                PrintPerformance(graph.ToString(), edgeCount, "edges counted", this.StopWatch());
            }
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                this.StopWatch();
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), vertexCount, "vertices counted", this.StopWatch());
                this.StopWatch();
                foreach (Vertex vertex in graph.GetVertices())
                {
                    if (Count(vertex.GetEdges(Direction.OUT)) > 0) {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.OUT)));
                        Assert.False(Count(vertex.GetEdges(Direction.IN)) > 0);

                    } else {
                        Assert.AreEqual(1, Count(vertex.GetEdges(Direction.IN)));
                        Assert.False(Count(vertex.GetEdges(Direction.OUT)) > 0);
                    }
                }
                PrintPerformance(graph.ToString(), vertexCount, "vertices checked", this.StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGetEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex v1 = graph.AddVertex(null);
            Vertex v2 = graph.AddVertex(null);
            Vertex v3 = graph.AddVertex(null);

            Edge e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test1"));
            Edge e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test2"));
            Edge e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test3"));

            if (graph.GetFeatures().SupportsEdgeRetrieval.Value)
            {
                this.StopWatch();
                Assert.AreEqual(graph.GetEdge(e1.GetId()), e1);
                Assert.AreEqual(graph.GetEdge(e1.GetId()).GetVertex(Direction.IN), v2);
                Assert.AreEqual(graph.GetEdge(e1.GetId()).GetVertex(Direction.OUT), v1);

                Assert.AreEqual(graph.GetEdge(e2.GetId()), e2);
                Assert.AreEqual(graph.GetEdge(e2.GetId()).GetVertex(Direction.IN), v3);
                Assert.AreEqual(graph.GetEdge(e2.GetId()).GetVertex(Direction.OUT), v2);

                Assert.AreEqual(graph.GetEdge(e3.GetId()), e3);
                Assert.AreEqual(graph.GetEdge(e3.GetId()).GetVertex(Direction.IN), v1);
                Assert.AreEqual(graph.GetEdge(e3.GetId()).GetVertex(Direction.OUT), v3);

                PrintPerformance(graph.ToString(), 3, "edges retrieved", this.StopWatch());
            }

            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.OUT)), e1);
            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.OUT)).GetVertex(Direction.IN), v2);
            Assert.AreEqual(GetOnlyElement(v1.GetEdges(Direction.OUT)).GetVertex(Direction.OUT), v1);

            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.OUT)), e2);
            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.OUT)).GetVertex(Direction.IN), v3);
            Assert.AreEqual(GetOnlyElement(v2.GetEdges(Direction.OUT)).GetVertex(Direction.OUT), v2);

            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.OUT)), e3);
            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.OUT)).GetVertex(Direction.IN), v1);
            Assert.AreEqual(GetOnlyElement(v3.GetEdges(Direction.OUT)).GetVertex(Direction.OUT), v3);

            graph.Shutdown();
        }

        [Test]
        public void TestGetNonExistantEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeRetrieval.Value)
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
            Graph graph = _GraphTest.GenerateGraph();
            long counter = 200000;
            int edgeCount = 10;
            HashSet<Edge> edges = new HashSet<Edge>();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex out_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                Vertex in_ = graph.AddVertex(ConvertId(graph, "" + counter++));
                edges.Add(graph.AddEdge(null, out_, in_, ConvertId(graph, "a" + Guid.NewGuid().ToString())));
            }
            Assert.AreEqual(edgeCount, edges.Count());

            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                this.StopWatch();
                Assert.AreEqual(edgeCount * 2, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), edgeCount * 2, "vertices counted", this.StopWatch());
            }

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                this.StopWatch();
                Assert.AreEqual(edgeCount, Count(graph.GetEdges()));
                PrintPerformance(graph.ToString(), edgeCount, "edges counted", this.StopWatch());

                int i = edgeCount;
                this.StopWatch();
                foreach (Edge edge in edges)
                {
                    graph.RemoveEdge(edge);
                    i--;
                    Assert.AreEqual(i, Count(graph.GetEdges()));
                    if (graph.GetFeatures().SupportsVertexIteration.Value)
                    {
                        int x = 0;
                        foreach (Vertex vertex in graph.GetVertices())
                        {
                            if (Count(vertex.GetEdges(Direction.OUT)) > 0)
                            {
                                Assert.AreEqual(1, Count(vertex.GetEdges(Direction.OUT)));
                                Assert.False(Count(vertex.GetEdges(Direction.IN)) > 0);
                            }
                            else if (Count(vertex.GetEdges(Direction.IN)) > 0)
                            {
                                Assert.AreEqual(1, Count(vertex.GetEdges(Direction.IN)));
                                Assert.False(Count(vertex.GetEdges(Direction.OUT)) > 0);
                            }
                            else
                            {
                                x++;
                            }
                        }
                        Assert.AreEqual((edgeCount - i) * 2, x);
                    }
                }
                PrintPerformance(graph.ToString(), edgeCount, "edges removed and graph checked", this.StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddingDuplicateEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));

            if (graph.GetFeatures().SupportsDuplicateEdges.Value)
            {
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(3, Count(graph.GetVertices()));
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(5, Count(graph.GetEdges()));

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
                Assert.AreEqual(4, Count(v2.GetEdges(Direction.OUT)));
                Assert.AreEqual(4, Count(v3.GetEdges(Direction.IN)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            }
            else
            {
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 3);
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(Count(graph.GetEdges()), 2);

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(v3.GetEdges(Direction.IN)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveEdgesByRemovingVertex()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));

            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(1, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));

            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));

                Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
                Assert.AreEqual(1, Count(v1.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
                Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            }

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(3, Count(graph.GetVertices()));

            graph.RemoveVertex(v1);

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(2, Count(graph.GetVertices()));

            if (graph.GetFeatures().SupportsDuplicateEdges.Value)
                Assert.AreEqual(2, Count(v2.GetEdges(Direction.OUT)));
            else
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.OUT)));

            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));

            if (graph.GetFeatures().SupportsDuplicateEdges.Value)
                Assert.AreEqual(2, Count(v3.GetEdges(Direction.IN)));
            else
                Assert.AreEqual(1, Count(v3.GetEdges(Direction.IN)));

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
            Edge e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            Edge e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "pets"));
            Edge e3 = graph.AddEdge(null, v2, v3, ConvertId(graph, "cares_for"));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(3, Count(graph.GetVertices()));

            graph.RemoveEdge(e1);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.IN)));
            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(2, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(2, Count(v3.GetEdges(Direction.IN)));

            graph.RemoveEdge(e2);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(1, Count(v3.GetEdges(Direction.IN)));
            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(1, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(1, Count(v3.GetEdges(Direction.IN)));

            graph.RemoveEdge(e3);
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.IN)));
            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                v1 = graph.GetVertex(ConvertId(graph, "1"));
                v2 = graph.GetVertex(ConvertId(graph, "2"));
                v3 = graph.GetVertex(ConvertId(graph, "3"));
            }
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.OUT)));
            Assert.AreEqual(0, Count(v1.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));
            Assert.AreEqual(0, Count(v3.GetEdges(Direction.IN)));

            graph.Shutdown();
        }

        [Test]
        public void TestAddingSelfLoops()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsSelfLoops.Value)
            {
                Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
                Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
                Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
                graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(3, Count(graph.GetVertices()));
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(3, Count(graph.GetEdges()));
                    int counter = 0;
                    foreach (Edge edge in graph.GetEdges())
                    {
                        counter++;
                        Assert.AreEqual(edge.GetVertex(Direction.IN), edge.GetVertex(Direction.OUT));
                        Assert.AreEqual(edge.GetVertex(Direction.IN).GetId(), edge.GetVertex(Direction.OUT).GetId());
                    }
                    Assert.AreEqual(counter, 3);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveSelfLoops()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsSelfLoops.Value)
            {
                Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
                Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
                Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
                Edge e1 = graph.AddEdge(null, v1, v1, ConvertId(graph, "is_self"));
                Edge e2 = graph.AddEdge(null, v2, v2, ConvertId(graph, "is_self"));
                Edge e3 = graph.AddEdge(null, v3, v3, ConvertId(graph, "is_self"));

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(3, Count(graph.GetVertices()));
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(3, Count(graph.GetEdges()));
                    foreach (Edge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.IN), edge.GetVertex(Direction.OUT));
                        Assert.AreEqual(edge.GetVertex(Direction.IN).GetId(), edge.GetVertex(Direction.OUT).GetId());
                    }
                }

                graph.RemoveVertex(v1);
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(2, Count(graph.GetEdges()));
                    foreach (Edge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.IN), edge.GetVertex(Direction.OUT));
                        Assert.AreEqual(edge.GetVertex(Direction.IN).GetId(), edge.GetVertex(Direction.OUT).GetId());
                    }
                }

                Assert.AreEqual(1, Count(v2.GetEdges(Direction.OUT)));
                Assert.AreEqual(1, Count(v2.GetEdges(Direction.IN)));
                graph.RemoveEdge(e2);
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.OUT)));
                Assert.AreEqual(0, Count(v2.GetEdges(Direction.IN)));

                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 1);
                    foreach (Edge edge in graph.GetEdges())
                    {
                        Assert.AreEqual(edge.GetVertex(Direction.IN), edge.GetVertex(Direction.OUT));
                        Assert.AreEqual(edge.GetVertex(Direction.IN).GetId(), edge.GetVertex(Direction.OUT).GetId());
                    }
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestEdgeIterator()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
                Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
                Vertex v3 = graph.AddVertex(ConvertId(graph, "3"));
                Edge e1 = graph.AddEdge(null, v1, v2, ConvertId(graph, "test"));
                Edge e2 = graph.AddEdge(null, v2, v3, ConvertId(graph, "test"));
                Edge e3 = graph.AddEdge(null, v3, v1, ConvertId(graph, "test"));

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(3, Count(graph.GetVertices()));
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(3, Count(graph.GetEdges()));

                HashSet<string> edgeIds = new HashSet<string>();
                int count = 0;
                foreach (Edge e in graph.GetEdges())
                {
                    count++;
                    edgeIds.Add(e.GetId().ToString());
                    Assert.AreEqual(ConvertId(graph, "test"), e.GetLabel());
                    if (e.GetId().ToString().Equals(e1.GetId().ToString()))
                    {
                        Assert.AreEqual(v1, e.GetVertex(Direction.OUT));
                        Assert.AreEqual(v2, e.GetVertex(Direction.IN));
                    }
                    else if (e.GetId().ToString().Equals(e2.GetId().ToString()))
                    {
                        Assert.AreEqual(v2, e.GetVertex(Direction.OUT));
                        Assert.AreEqual(v3, e.GetVertex(Direction.IN));
                    }
                    else if (e.GetId().ToString().Equals(e3.GetId().ToString()))
                    {
                        Assert.AreEqual(v3, e.GetVertex(Direction.OUT));
                        Assert.AreEqual(v1, e.GetVertex(Direction.IN));
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
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {
                Vertex a = graph.AddVertex(ConvertId(graph, "1"));
                Vertex b = graph.AddVertex(ConvertId(graph, "2"));
                Edge edge = graph.AddEdge(ConvertId(graph, "3"), a, b, "knows");
                Assert.AreEqual(edge.GetPropertyKeys().Count(), 0);
                Assert.Null(edge.GetProperty("weight"));

                if (graph.GetFeatures().SupportsDoubleProperty.Value)
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

                if (graph.GetFeatures().SupportsStringProperty.Value)
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
            Graph graph = _GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the id or label properties.
            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {

                Edge edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                try
                {
                    edge.SetProperty("id", "123");
                    Assert.Fail();
                }
                catch (Exception)
                {
                }
                try
                {
                    edge.SetProperty("label", "hates");
                    Assert.Fail();
                }
                catch (Exception)
                {
                }

            }
            graph.Shutdown();
        }

        [Test]
        public void TestNoConcurrentModificationException()
        {
            Graph graph = _GraphTest.GenerateGraph();
            for (int i = 0; i < 25; i++) {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "test"));
            }
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(BaseTest.Count(graph.GetVertices()), 50);
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
            {
                Assert.AreEqual(BaseTest.Count(graph.GetEdges()), 25);
                foreach (Edge edge in graph.GetEdges())
                    graph.RemoveEdge(edge);
                
                Assert.AreEqual(BaseTest.Count(graph.GetEdges()), 0);
            }
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(BaseTest.Count(graph.GetVertices()), 50);

            graph.Shutdown();
        }

        [Test]
        public void TestEmptyKeyProperty()
        {
            Graph graph = _GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {
                Edge e = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "friend");
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
            Graph graph = _GraphTest.GenerateGraph();

            Edge a = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
            Edge b = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));
            Edge c = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), ConvertId(graph, "knows"));

            object cId = c.GetId();

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), 3);

            a.Remove();
            b.Remove();

            if (graph.GetFeatures().SupportsEdgeRetrieval.Value)
                Assert.NotNull(graph.GetEdge(cId));

            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestSettingBadVertexProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Edge edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
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
                    edge.SetProperty(StringFactory.ID, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.SetProperty(StringFactory.LABEL, "friend");
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
