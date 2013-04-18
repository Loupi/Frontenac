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
        public void testEdgeEquality()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v = graph.addVertex(convertId(graph, "1"));
            Vertex u = graph.addVertex(convertId(graph, "2"));
            Edge e = graph.addEdge(null, v, u, convertId(graph, "knows"));
            Assert.AreEqual(e.getLabel(), convertId(graph, "knows"));
            Assert.AreEqual(e.getVertex(Direction.IN), u);
            Assert.AreEqual(e.getVertex(Direction.OUT), v);
            Assert.AreEqual(e, v.getEdges(Direction.OUT).First());
            Assert.AreEqual(e, u.getEdges(Direction.IN).First());
            Assert.AreEqual(v.getEdges(Direction.OUT).First(), u.getEdges(Direction.IN).First());
            HashSet<Edge> set = new HashSet<Edge>();
            set.Add(e);
            set.Add(e);
            set.Add(v.getEdges(Direction.OUT).First());
            set.Add(v.getEdges(Direction.OUT).First());
            set.Add(u.getEdges(Direction.IN).First());
            set.Add(u.getEdges(Direction.IN).First());
            if (graph.getFeatures().supportsEdgeIteration.Value)
                set.Add(graph.getEdges().First());
            Assert.AreEqual(set.Count(), 1);
            graph.shutdown();
        }

        [Test]
        public void testAddEdges()
        {
            Graph graph = graphTest.generateGraph();

            this.stopWatch();
            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            Vertex v2 = graph.addVertex(convertId(graph, "2"));
            Vertex v3 = graph.addVertex(convertId(graph, "3"));
            graph.addEdge(null, v1, v2, convertId(graph, "knows"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            graph.addEdge(null, v2, v3, convertId(graph, "caresFor"));
            Assert.AreEqual(1, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(2, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(2, count(v3.getEdges(Direction.IN)));
            printPerformance(graph.ToString(), 6, "elements added and checked", this.stopWatch());
            graph.shutdown();
        }

        [Test]
        public void testAddManyEdges()
        {
            Graph graph = graphTest.generateGraph();
            int edgeCount = 100;
            int vertexCount = 200;
            long counter = 0;
            this.stopWatch();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex out_ = graph.addVertex(convertId(graph, "" + counter++));
                Vertex in_ = graph.addVertex(convertId(graph, "" + counter++));
                graph.addEdge(null, out_, in_, convertId(graph, Guid.NewGuid().ToString()));
            }
            printPerformance(graph.ToString(), vertexCount + edgeCount, "elements added", this.stopWatch());
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                this.stopWatch();
                Assert.AreEqual(edgeCount, count(graph.getEdges()));
                printPerformance(graph.ToString(), edgeCount, "edges counted", this.stopWatch());
            }
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                Assert.AreEqual(vertexCount, count(graph.getVertices()));
                printPerformance(graph.ToString(), vertexCount, "vertices counted", this.stopWatch());
                this.stopWatch();
                foreach (Vertex vertex in graph.getVertices())
                {
                    if (count(vertex.getEdges(Direction.OUT)) > 0) {
                        Assert.AreEqual(1, count(vertex.getEdges(Direction.OUT)));
                        Assert.False(count(vertex.getEdges(Direction.IN)) > 0);

                    } else {
                        Assert.AreEqual(1, count(vertex.getEdges(Direction.IN)));
                        Assert.False(count(vertex.getEdges(Direction.OUT)) > 0);
                    }
                }
                printPerformance(graph.ToString(), vertexCount, "vertices checked", this.stopWatch());
            }
            graph.shutdown();
        }

        [Test]
        public void testGetEdges()
        {
            Graph graph = graphTest.generateGraph();
            Vertex v1 = graph.addVertex(null);
            Vertex v2 = graph.addVertex(null);
            Vertex v3 = graph.addVertex(null);

            Edge e1 = graph.addEdge(null, v1, v2, convertId(graph, "test1"));
            Edge e2 = graph.addEdge(null, v2, v3, convertId(graph, "test2"));
            Edge e3 = graph.addEdge(null, v3, v1, convertId(graph, "test3"));

            if (graph.getFeatures().supportsEdgeRetrieval.Value)
            {
                this.stopWatch();
                Assert.AreEqual(graph.getEdge(e1.getId()), e1);
                Assert.AreEqual(graph.getEdge(e1.getId()).getVertex(Direction.IN), v2);
                Assert.AreEqual(graph.getEdge(e1.getId()).getVertex(Direction.OUT), v1);

                Assert.AreEqual(graph.getEdge(e2.getId()), e2);
                Assert.AreEqual(graph.getEdge(e2.getId()).getVertex(Direction.IN), v3);
                Assert.AreEqual(graph.getEdge(e2.getId()).getVertex(Direction.OUT), v2);

                Assert.AreEqual(graph.getEdge(e3.getId()), e3);
                Assert.AreEqual(graph.getEdge(e3.getId()).getVertex(Direction.IN), v1);
                Assert.AreEqual(graph.getEdge(e3.getId()).getVertex(Direction.OUT), v3);

                printPerformance(graph.ToString(), 3, "edges retrieved", this.stopWatch());
            }

            Assert.AreEqual(getOnlyElement(v1.getEdges(Direction.OUT)), e1);
            Assert.AreEqual(getOnlyElement(v1.getEdges(Direction.OUT)).getVertex(Direction.IN), v2);
            Assert.AreEqual(getOnlyElement(v1.getEdges(Direction.OUT)).getVertex(Direction.OUT), v1);

            Assert.AreEqual(getOnlyElement(v2.getEdges(Direction.OUT)), e2);
            Assert.AreEqual(getOnlyElement(v2.getEdges(Direction.OUT)).getVertex(Direction.IN), v3);
            Assert.AreEqual(getOnlyElement(v2.getEdges(Direction.OUT)).getVertex(Direction.OUT), v2);

            Assert.AreEqual(getOnlyElement(v3.getEdges(Direction.OUT)), e3);
            Assert.AreEqual(getOnlyElement(v3.getEdges(Direction.OUT)).getVertex(Direction.IN), v1);
            Assert.AreEqual(getOnlyElement(v3.getEdges(Direction.OUT)).getVertex(Direction.OUT), v3);

            graph.shutdown();
        }

        [Test]
        public void testGetNonExistantEdges()
        {
            Graph graph = graphTest.generateGraph();

            if (graph.getFeatures().supportsEdgeRetrieval.Value)
            {
                try
                {
                    graph.getEdge(null);
                    Assert.Fail("Getting an element with a null identifier must throw IllegalArgumentException");
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }

                Assert.Null(graph.getEdge("asbv"));
                Assert.Null(graph.getEdge(12.0d));
            }

            graph.shutdown();
        }

        [Test]
        public void testRemoveManyEdges() 
        {
            Graph graph = graphTest.generateGraph();
            long counter = 200000;
            int edgeCount = 10;
            HashSet<Edge> edges = new HashSet<Edge>();
            for (int i = 0; i < edgeCount; i++)
            {
                Vertex out_ = graph.addVertex(convertId(graph, "" + counter++));
                Vertex in_ = graph.addVertex(convertId(graph, "" + counter++));
                edges.Add(graph.addEdge(null, out_, in_, convertId(graph, "a" + Guid.NewGuid().ToString())));
            }
            Assert.AreEqual(edgeCount, edges.Count());

            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                Assert.AreEqual(edgeCount * 2, count(graph.getVertices()));
                printPerformance(graph.ToString(), edgeCount * 2, "vertices counted", this.stopWatch());
            }

            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                this.stopWatch();
                Assert.AreEqual(edgeCount, count(graph.getEdges()));
                printPerformance(graph.ToString(), edgeCount, "edges counted", this.stopWatch());

                int i = edgeCount;
                this.stopWatch();
                foreach (Edge edge in edges)
                {
                    graph.removeEdge(edge);
                    i--;
                    Assert.AreEqual(i, count(graph.getEdges()));
                    if (graph.getFeatures().supportsVertexIteration.Value)
                    {
                        int x = 0;
                        foreach (Vertex vertex in graph.getVertices())
                        {
                            if (count(vertex.getEdges(Direction.OUT)) > 0)
                            {
                                Assert.AreEqual(1, count(vertex.getEdges(Direction.OUT)));
                                Assert.False(count(vertex.getEdges(Direction.IN)) > 0);
                            }
                            else if (count(vertex.getEdges(Direction.IN)) > 0)
                            {
                                Assert.AreEqual(1, count(vertex.getEdges(Direction.IN)));
                                Assert.False(count(vertex.getEdges(Direction.OUT)) > 0);
                            }
                            else
                            {
                                x++;
                            }
                        }
                        Assert.AreEqual((edgeCount - i) * 2, x);
                    }
                }
                printPerformance(graph.ToString(), edgeCount, "edges removed and graph checked", this.stopWatch());
            }
            graph.shutdown();
        }

        [Test]
        public void testAddingDuplicateEdges()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            Vertex v2 = graph.addVertex(convertId(graph, "2"));
            Vertex v3 = graph.addVertex(convertId(graph, "3"));
            graph.addEdge(null, v1, v2, convertId(graph, "knows"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));

            if (graph.getFeatures().supportsDuplicateEdges.Value)
            {
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(3, count(graph.getVertices()));
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(5, count(graph.getEdges()));

                Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(v1.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
                Assert.AreEqual(4, count(v2.getEdges(Direction.OUT)));
                Assert.AreEqual(4, count(v3.getEdges(Direction.IN)));
                Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            }
            else
            {
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 3);
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(count(graph.getEdges()), 2);

                Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(v1.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(v2.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(v3.getEdges(Direction.IN)));
                Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            }
            graph.shutdown();
        }

        [Test]
        public void testRemoveEdgesByRemovingVertex()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            Vertex v2 = graph.addVertex(convertId(graph, "2"));
            Vertex v3 = graph.addVertex(convertId(graph, "3"));
            graph.addEdge(null, v1, v2, convertId(graph, "knows"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            graph.addEdge(null, v2, v3, convertId(graph, "pets"));

            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(1, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));

            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                v1 = graph.getVertex(convertId(graph, "1"));
                v2 = graph.getVertex(convertId(graph, "2"));
                v3 = graph.getVertex(convertId(graph, "3"));

                Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
                Assert.AreEqual(1, count(v1.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
                Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            }

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(3, count(graph.getVertices()));

            graph.removeVertex(v1);

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(2, count(graph.getVertices()));

            if (graph.getFeatures().supportsDuplicateEdges.Value)
                Assert.AreEqual(2, count(v2.getEdges(Direction.OUT)));
            else
                Assert.AreEqual(1, count(v2.getEdges(Direction.OUT)));

            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));

            if (graph.getFeatures().supportsDuplicateEdges.Value)
                Assert.AreEqual(2, count(v3.getEdges(Direction.IN)));
            else
                Assert.AreEqual(1, count(v3.getEdges(Direction.IN)));

            graph.shutdown();
        }

        [Test]
        public void testRemoveEdges()
        {
            Graph graph = graphTest.generateGraph();
            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            Vertex v2 = graph.addVertex(convertId(graph, "2"));
            Vertex v3 = graph.addVertex(convertId(graph, "3"));
            Edge e1 = graph.addEdge(null, v1, v2, convertId(graph, "knows"));
            Edge e2 = graph.addEdge(null, v2, v3, convertId(graph, "pets"));
            Edge e3 = graph.addEdge(null, v2, v3, convertId(graph, "cares_for"));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(3, count(graph.getVertices()));

            graph.removeEdge(e1);
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(2, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(2, count(v3.getEdges(Direction.IN)));
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                v1 = graph.getVertex(convertId(graph, "1"));
                v2 = graph.getVertex(convertId(graph, "2"));
                v3 = graph.getVertex(convertId(graph, "3"));
            }
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(2, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(2, count(v3.getEdges(Direction.IN)));

            graph.removeEdge(e2);
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(1, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(1, count(v3.getEdges(Direction.IN)));
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                v1 = graph.getVertex(convertId(graph, "1"));
                v2 = graph.getVertex(convertId(graph, "2"));
                v3 = graph.getVertex(convertId(graph, "3"));
            }
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(1, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(1, count(v3.getEdges(Direction.IN)));

            graph.removeEdge(e3);
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.IN)));
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                v1 = graph.getVertex(convertId(graph, "1"));
                v2 = graph.getVertex(convertId(graph, "2"));
                v3 = graph.getVertex(convertId(graph, "3"));
            }
            Assert.AreEqual(0, count(v1.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.OUT)));
            Assert.AreEqual(0, count(v1.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));
            Assert.AreEqual(0, count(v3.getEdges(Direction.IN)));

            graph.shutdown();
        }

        [Test]
        public void testAddingSelfLoops()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsSelfLoops.Value)
            {
                Vertex v1 = graph.addVertex(convertId(graph, "1"));
                Vertex v2 = graph.addVertex(convertId(graph, "2"));
                Vertex v3 = graph.addVertex(convertId(graph, "3"));
                graph.addEdge(null, v1, v1, convertId(graph, "is_self"));
                graph.addEdge(null, v2, v2, convertId(graph, "is_self"));
                graph.addEdge(null, v3, v3, convertId(graph, "is_self"));

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(3, count(graph.getVertices()));
                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(3, count(graph.getEdges()));
                    int counter = 0;
                    foreach (Edge edge in graph.getEdges())
                    {
                        counter++;
                        Assert.AreEqual(edge.getVertex(Direction.IN), edge.getVertex(Direction.OUT));
                        Assert.AreEqual(edge.getVertex(Direction.IN).getId(), edge.getVertex(Direction.OUT).getId());
                    }
                    Assert.AreEqual(counter, 3);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testRemoveSelfLoops()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsSelfLoops.Value)
            {
                Vertex v1 = graph.addVertex(convertId(graph, "1"));
                Vertex v2 = graph.addVertex(convertId(graph, "2"));
                Vertex v3 = graph.addVertex(convertId(graph, "3"));
                Edge e1 = graph.addEdge(null, v1, v1, convertId(graph, "is_self"));
                Edge e2 = graph.addEdge(null, v2, v2, convertId(graph, "is_self"));
                Edge e3 = graph.addEdge(null, v3, v3, convertId(graph, "is_self"));

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(3, count(graph.getVertices()));
                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(3, count(graph.getEdges()));
                    foreach (Edge edge in graph.getEdges())
                    {
                        Assert.AreEqual(edge.getVertex(Direction.IN), edge.getVertex(Direction.OUT));
                        Assert.AreEqual(edge.getVertex(Direction.IN).getId(), edge.getVertex(Direction.OUT).getId());
                    }
                }

                graph.removeVertex(v1);
                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(2, count(graph.getEdges()));
                    foreach (Edge edge in graph.getEdges())
                    {
                        Assert.AreEqual(edge.getVertex(Direction.IN), edge.getVertex(Direction.OUT));
                        Assert.AreEqual(edge.getVertex(Direction.IN).getId(), edge.getVertex(Direction.OUT).getId());
                    }
                }

                Assert.AreEqual(1, count(v2.getEdges(Direction.OUT)));
                Assert.AreEqual(1, count(v2.getEdges(Direction.IN)));
                graph.removeEdge(e2);
                Assert.AreEqual(0, count(v2.getEdges(Direction.OUT)));
                Assert.AreEqual(0, count(v2.getEdges(Direction.IN)));

                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(count(graph.getEdges()), 1);
                    foreach (Edge edge in graph.getEdges())
                    {
                        Assert.AreEqual(edge.getVertex(Direction.IN), edge.getVertex(Direction.OUT));
                        Assert.AreEqual(edge.getVertex(Direction.IN).getId(), edge.getVertex(Direction.OUT).getId());
                    }
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testEdgeIterator()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                Vertex v1 = graph.addVertex(convertId(graph, "1"));
                Vertex v2 = graph.addVertex(convertId(graph, "2"));
                Vertex v3 = graph.addVertex(convertId(graph, "3"));
                Edge e1 = graph.addEdge(null, v1, v2, convertId(graph, "test"));
                Edge e2 = graph.addEdge(null, v2, v3, convertId(graph, "test"));
                Edge e3 = graph.addEdge(null, v3, v1, convertId(graph, "test"));

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(3, BaseTest.count(graph.getVertices()));
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(3, BaseTest.count(graph.getEdges()));

                HashSet<string> edgeIds = new HashSet<string>();
                int count = 0;
                foreach (Edge e in graph.getEdges())
                {
                    count++;
                    edgeIds.Add(e.getId().ToString());
                    Assert.AreEqual(convertId(graph, "test"), e.getLabel());
                    if (e.getId().ToString().Equals(e1.getId().ToString()))
                    {
                        Assert.AreEqual(v1, e.getVertex(Direction.OUT));
                        Assert.AreEqual(v2, e.getVertex(Direction.IN));
                    }
                    else if (e.getId().ToString().Equals(e2.getId().ToString()))
                    {
                        Assert.AreEqual(v2, e.getVertex(Direction.OUT));
                        Assert.AreEqual(v3, e.getVertex(Direction.IN));
                    }
                    else if (e.getId().ToString().Equals(e3.getId().ToString()))
                    {
                        Assert.AreEqual(v3, e.getVertex(Direction.OUT));
                        Assert.AreEqual(v1, e.getVertex(Direction.IN));
                    }
                    else
                        Assert.True(false);
                    //System.out.println(e);
                }
                Assert.AreEqual(3, count);
                Assert.AreEqual(3, edgeIds.Count());
                Assert.True(edgeIds.Contains(e1.getId().ToString()));
                Assert.True(edgeIds.Contains(e2.getId().ToString()));
                Assert.True(edgeIds.Contains(e3.getId().ToString()));
            }
            graph.shutdown();
        }

        [Test]
        public void testAddingRemovingEdgeProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeProperties.Value)
            {
                Vertex a = graph.addVertex(convertId(graph, "1"));
                Vertex b = graph.addVertex(convertId(graph, "2"));
                Edge edge = graph.addEdge(convertId(graph, "3"), a, b, "knows");
                Assert.AreEqual(edge.getPropertyKeys().Count(), 0);
                Assert.Null(edge.getProperty("weight"));

                if (graph.getFeatures().supportsDoubleProperty.Value)
                {
                    edge.setProperty("weight", 0.5);
                    Assert.AreEqual(edge.getPropertyKeys().Count(), 1);
                    Assert.AreEqual(edge.getProperty("weight"), 0.5);

                    edge.setProperty("weight", 0.6);
                    Assert.AreEqual(edge.getPropertyKeys().Count(), 1);
                    Assert.AreEqual(edge.getProperty("weight"), 0.6);
                    Assert.AreEqual(edge.removeProperty("weight"), 0.6);
                    Assert.Null(edge.getProperty("weight"));
                    Assert.AreEqual(edge.getPropertyKeys().Count(), 0);
                }

                if (graph.getFeatures().supportsStringProperty.Value)
                {
                    edge.setProperty("blah", "marko");
                    edge.setProperty("blah2", "josh");
                    Assert.AreEqual(edge.getPropertyKeys().Count(), 2);
                }
            }

            graph.shutdown();
        }

        [Test]
        public void testAddingLabelAndIdProperty()
        {
            Graph graph = graphTest.generateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the id or label properties.
            if (graph.getFeatures().supportsEdgeProperties.Value)
            {

                Edge edge = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                try
                {
                    edge.setProperty("id", "123");
                    Assert.Fail();
                }
                catch (Exception)
                {
                }
                try
                {
                    edge.setProperty("label", "hates");
                    Assert.Fail();
                }
                catch (Exception)
                {
                }

            }
            graph.shutdown();
        }

        [Test]
        public void testNoConcurrentModificationException()
        {
            Graph graph = graphTest.generateGraph();
            for (int i = 0; i < 25; i++) {
                graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "test"));
            }
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(BaseTest.count(graph.getVertices()), 50);
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                Assert.AreEqual(BaseTest.count(graph.getEdges()), 25);
                foreach (Edge edge in graph.getEdges())
                    graph.removeEdge(edge);
                
                Assert.AreEqual(BaseTest.count(graph.getEdges()), 0);
            }
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(BaseTest.count(graph.getVertices()), 50);

            graph.shutdown();
        }

        [Test]
        public void testEmptyKeyProperty()
        {
            Graph graph = graphTest.generateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.getFeatures().supportsEdgeProperties.Value)
            {
                Edge e = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "friend");
                try
                {
                    e.setProperty("", "value");
                    Assert.Fail();
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testEdgeCentricRemoving()
        {
            Graph graph = graphTest.generateGraph();

            Edge a = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));
            Edge b = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));
            Edge c = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "knows"));

            object cId = c.getId();

            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 3);

            a.remove();
            b.remove();

            if (graph.getFeatures().supportsEdgeRetrieval.Value)
                Assert.NotNull(graph.getEdge(cId));

            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 1);

            graph.shutdown();
        }

        [Test]
        public void testSettingBadVertexProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Edge edge = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                try
                {
                    edge.setProperty(null, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.setProperty("", -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.setProperty(StringFactory.ID, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.setProperty(StringFactory.LABEL, "friend");
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    edge.setProperty(convertId(graph, "good"), null);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }
            }
            graph.shutdown();
        }
    }
}
