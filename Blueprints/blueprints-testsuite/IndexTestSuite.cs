using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public abstract class IndexTestSuite : TestSuite
    {
        public IndexTestSuite(GraphTest graphTest)
            : base("IndexTestSuite", graphTest)
        {

        }

        [Test]
        public void testPutGetRemoveVertex()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value)
            {
                this.stopWatch();
                Index index = graph.createIndex("basic", typeof(Vertex));
                printPerformance(graph.ToString(), 1, "manual index created", this.stopWatch());
                Vertex v1 = graph.addVertex(null);
                Vertex v2 = graph.addVertex(null);
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 2);

                this.stopWatch();
                index.put("dog", "puppy", v1);
                index.put("dog", "mama", v2);
                printPerformance(graph.ToString(), 2, "vertices manually index", this.stopWatch());
                Assert.AreEqual(v1, index.get("dog", "puppy").First());
                Assert.AreEqual(v2, index.get("dog", "mama").First());
                Assert.AreEqual(1, index.count("dog", "puppy"));

                v1.removeProperty("dog");
                Assert.AreEqual(v1, index.get("dog", "puppy").First());
                Assert.AreEqual(v2, index.get("dog", "mama").First());

                this.stopWatch();
                graph.removeVertex(v1);
                printPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index", this.stopWatch());
                Assert.AreEqual(count(index.get("dog", "puppy")), 0);
                Assert.AreEqual(v2, index.get("dog", "mama").First());
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 1);

                v2.setProperty("dog", "mama2");
                Assert.AreEqual(v2, index.get("dog", "mama").First());
                this.stopWatch();
                graph.removeVertex(v2);
                printPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index", this.stopWatch());
                Assert.AreEqual(count(index.get("dog", "puppy")), 0);
                Assert.AreEqual(count(index.get("dog", "mama")), 0);
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 0);
            }
            graph.shutdown();
        }

        [Test]
        public void testIndexCount()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value)
            {
                Index index = graph.createIndex("basic", typeof(Vertex));
                for (int i = 0; i < 10; i++)
                {
                    Vertex v1 = graph.addVertex(null);
                    index.put("dog", "puppy", v1);
                }
                Assert.AreEqual(10, index.count("dog", "puppy"));
                Vertex v = (Vertex)index.get("dog", "puppy").First();
                graph.removeVertex(v);
                index.remove("dog", "puppy", v);
                Assert.AreEqual(9, index.count("dog", "puppy"));

            }
            graph.shutdown();
        }

        [Test]
        public void testPutGetRemoveEdge()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIndex.Value)
            {
                this.stopWatch();
                Index index = graph.createIndex("basic", typeof(Edge));
                printPerformance(graph.ToString(), 1, "manual index created", this.stopWatch());
                Vertex v1 = graph.addVertex(null);
                Vertex v2 = graph.addVertex(null);
                Edge e1 = graph.addEdge(null, v1, v2, "test1");
                Edge e2 = graph.addEdge(null, v1, v2, "test2");
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(count(graph.getEdges()), 2);

                this.stopWatch();
                index.put("dog", "puppy", e1);
                index.put("dog", "mama", e2);
                printPerformance(graph.ToString(), 2, "edges manually index", this.stopWatch());
                Assert.AreEqual(e1, index.get("dog", "puppy").First());
                Assert.AreEqual(e2, index.get("dog", "mama").First());

                v1.removeProperty("dog");
                Assert.AreEqual(e1, index.get("dog", "puppy").First());
                Assert.AreEqual(e2, index.get("dog", "mama").First());

                this.stopWatch();
                graph.removeEdge(e1);
                printPerformance(graph.ToString(), 1, "edge removed and automatically removed from index", this.stopWatch());
                Assert.AreEqual(count(index.get("dog", "puppy")), 0);
                Assert.AreEqual(e2, index.get("dog", "mama").First());
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(count(graph.getEdges()), 1);

                v2.setProperty("dog", "mama2");
                Assert.AreEqual(e2, index.get("dog", "mama").First());
                this.stopWatch();
                graph.removeEdge(e2);
                printPerformance(graph.ToString(), 1, "edge removed and automatically removed from index", this.stopWatch());
                Assert.AreEqual(count(index.get("dog", "puppy")), 0);
                Assert.AreEqual(count(index.get("dog", "mama")), 0);
                if (graph.getFeatures().supportsEdgeIteration.Value)
                    Assert.AreEqual(count(graph.getEdges()), 0);
            }
            graph.shutdown();
        }

        [Test]
        public void testCloseableSequence()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value)
            {
                Index index = graph.createIndex("basic", typeof(Vertex));
                for (int i = 0; i < 10; i++)
                {
                    Vertex v = graph.addVertex(null);
                    index.put("dog", "puppy", v);
                }
                CloseableIterable<Element> hits = index.get("dog", "puppy");
                int counter = 0;
                foreach (Vertex v in hits)
                {
                    counter++;
                }
                Assert.AreEqual(counter, 10);
                hits.Dispose(); // no exception should be thrown

            }
            graph.shutdown();
        }
    }
}
