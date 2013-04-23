using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public abstract class KeyIndexableGraphTestSuite : TestSuite
    {
        protected KeyIndexableGraphTestSuite(GraphTest graphTest)
            : base("KeyIndexableGraphTestSuite", graphTest)
        {
        }

        [Test]
        public void testAutoIndexKeyManagementWithPersistence()
        {
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexKeyIndex.Value)
            {
                Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 0);
                this.stopWatch();
                graph.createKeyIndex("name", typeof(Vertex));
                graph.createKeyIndex("location", typeof(Vertex));
                printPerformance(graph.ToString(), 2, "automatic index keys added", this.stopWatch());
                Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 2);
                Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("name"));
                Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("location"));
            }
            if (graph.getFeatures().supportsEdgeKeyIndex.Value)
            {
                Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 0);
                this.stopWatch();
                graph.createKeyIndex("weight", typeof(Edge));
                graph.createKeyIndex("since", typeof(Edge));
                printPerformance(graph.ToString(), 2, "automatic index keys added", this.stopWatch());
                Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 2);
                Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("weight"));
                Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("since"));
            }
            graph.shutdown();

            if (graph.getFeatures().isPersistent.Value)
            {
                graph = (KeyIndexableGraph)graphTest.generateGraph();
                if (graph.getFeatures().supportsVertexKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 2);
                    Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("name"));
                    Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("location"));
                }
                if (graph.getFeatures().supportsEdgeKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 2);
                    Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("weight"));
                    Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("since"));
                }
                graph.shutdown();
            }
        }

        [Test]
        public void testAutoIndexKeyDroppingWithPersistence()
        {
            testAutoIndexKeyManagementWithPersistence();
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().isPersistent.Value)
            {
                if (graph.getFeatures().supportsVertexKeyIndex.Value)
                {
                    graph.dropKeyIndex("name", typeof(Vertex));
                }
                if (graph.getFeatures().supportsEdgeKeyIndex.Value)
                {
                    graph.dropKeyIndex("weight", typeof(Edge));
                }
                graph.shutdown();

                graph = (KeyIndexableGraph)graphTest.generateGraph();
                if (graph.getFeatures().supportsVertexKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 1);
                    Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("location"));
                    graph.dropKeyIndex("location", typeof(Vertex));
                }
                if (graph.getFeatures().supportsEdgeKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 1);
                    Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("since"));
                    graph.dropKeyIndex("since", typeof(Edge));
                }
                graph.shutdown();
                graph = (KeyIndexableGraph)graphTest.generateGraph();
                if (graph.getFeatures().supportsVertexKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 0);
                }
                if (graph.getFeatures().supportsEdgeKeyIndex.Value)
                {
                    Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 0);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testGettingVerticesAndEdgesWithKeyValue()
        {
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value && graph.getFeatures().supportsVertexKeyIndex.Value)
            {
                graph.createKeyIndex("name", typeof(Vertex));
                Assert.AreEqual(graph.getIndexedKeys(typeof(Vertex)).Count(), 1);
                Assert.True(graph.getIndexedKeys(typeof(Vertex)).Contains("name"));
                Vertex v1 = graph.addVertex(null);
                v1.setProperty("name", "marko");
                v1.setProperty("location", "everywhere");
                Vertex v2 = graph.addVertex(null);
                v2.setProperty("name", "stephen");
                v2.setProperty("location", "everywhere");

                Assert.AreEqual(count(graph.getVertices("location", "everywhere")), 2);
                Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
                Assert.AreEqual(count(graph.getVertices("name", "stephen")), 1);
                Assert.AreEqual(graph.getVertices("name", "marko").First(), v1);
                Assert.AreEqual(graph.getVertices("name", "stephen").First(), v2);
            }

            if (graph.getFeatures().supportsEdgeIteration.Value && graph.getFeatures().supportsEdgeKeyIndex.Value)
            {
                graph.createKeyIndex("place", typeof(Edge));
                Assert.AreEqual(graph.getIndexedKeys(typeof(Edge)).Count(), 1);
                Assert.True(graph.getIndexedKeys(typeof(Edge)).Contains("place"));

                Edge e1 = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                e1.setProperty("name", "marko");
                e1.setProperty("place", "everywhere");
                Edge e2 = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                e2.setProperty("name", "stephen");
                e2.setProperty("place", "everywhere");

                Assert.AreEqual(count(graph.getEdges("place", "everywhere")), 2);
                Assert.AreEqual(count(graph.getEdges("name", "marko")), 1);
                Assert.AreEqual(count(graph.getEdges("name", "stephen")), 1);
                Assert.AreEqual(graph.getEdges("name", "marko").First(), e1);
                Assert.AreEqual(graph.getEdges("name", "stephen").First(), e2);
            }
            graph.shutdown();
        }

        [Test]
        public void testReIndexingOfElements()
        {
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexKeyIndex.Value)
            {
                Vertex vertex = graph.addVertex(null);
                vertex.setProperty("name", "marko");
                Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
                Assert.AreEqual(graph.getVertices("name", "marko").First(), vertex);
                graph.createKeyIndex("name", typeof(Vertex));
                Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
                Assert.AreEqual(graph.getVertices("name", "marko").First(), vertex);
            }

            if (graph.getFeatures().supportsEdgeKeyIndex.Value)
            {
                Edge edge = graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "knows");
                edge.setProperty("date", 2012);
                Assert.AreEqual(count(graph.getEdges("date", 2012)), 1);
                Assert.AreEqual(graph.getEdges("date", 2012).First(), edge);
                graph.createKeyIndex("date", typeof(Edge));
                Assert.AreEqual(count(graph.getEdges("date", 2012)), 1);
                Assert.AreEqual(graph.getEdges("date", 2012).First(), edge);
            }
            graph.shutdown();
        }

        [Test]
        public void testNoConcurrentModificationException()
        {
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeKeyIndex.Value)
            {
                graph.createKeyIndex("key", typeof(Edge));
                for (int i = 0; i < 25; i++)
                {
                    graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), "test").setProperty("key", "value");
                }
                if (graph.getFeatures().supportsVertexIteration.Value) Assert.AreEqual(count(graph.getVertices()), 50);
                if (graph.getFeatures().supportsEdgeIteration.Value) Assert.AreEqual(count(graph.getEdges()), 25);
                int counter = 0;
                foreach (Edge edge in graph.getEdges("key", "value"))
                {
                    graph.removeEdge(edge);
                    counter++;
                }
                Assert.AreEqual(counter, 25);
                if (graph.getFeatures().supportsVertexIteration.Value) Assert.AreEqual(count(graph.getVertices()), 50);
                if (graph.getFeatures().supportsEdgeIteration.Value) Assert.AreEqual(count(graph.getEdges()), 0);

            }
            graph.shutdown();
        }

        [Test]
        public void testKeyIndicesConsistentWithElementRemoval()
        {
            KeyIndexableGraph graph = (KeyIndexableGraph)graphTest.generateGraph();

            graph.createKeyIndex("foo", typeof(Vertex));

            Vertex v1 = graph.addVertex(null);
            v1.setProperty("foo", 42);
            vertexCount(graph, 1);

            graph.removeVertex(v1);
            vertexCount(graph, 0);
            Assert.AreEqual(0, count(graph.getVertices("foo", 42)));

            graph.shutdown();
        }
    }
}
