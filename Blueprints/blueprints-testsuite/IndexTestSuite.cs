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
        public void TestPutGetRemoveVertex()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value)
            {
                this.StopWatch();
                Index index = graph.CreateIndex("basic", typeof(Vertex));
                PrintPerformance(graph.ToString(), 1, "manual index created", this.StopWatch());
                Vertex v1 = graph.AddVertex(null);
                Vertex v2 = graph.AddVertex(null);
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 2);

                this.StopWatch();
                index.Put("dog", "puppy", v1);
                index.Put("dog", "mama", v2);
                PrintPerformance(graph.ToString(), 2, "vertices manually index", this.StopWatch());
                Assert.AreEqual(v1, index.Get("dog", "puppy").First());
                Assert.AreEqual(v2, index.Get("dog", "mama").First());
                Assert.AreEqual(1, index.Count("dog", "puppy"));

                v1.RemoveProperty("dog");
                Assert.AreEqual(v1, index.Get("dog", "puppy").First());
                Assert.AreEqual(v2, index.Get("dog", "mama").First());

                this.StopWatch();
                graph.RemoveVertex(v1);
                PrintPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index", this.StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(v2, index.Get("dog", "mama").First());
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 1);

                v2.SetProperty("dog", "mama2");
                Assert.AreEqual(v2, index.Get("dog", "mama").First());
                this.StopWatch();
                graph.RemoveVertex(v2);
                PrintPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index", this.StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(Count(index.Get("dog", "mama")), 0);
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 0);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestIndexCount()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value)
            {
                Index index = graph.CreateIndex("basic", typeof(Vertex));
                for (int i = 0; i < 10; i++)
                {
                    Vertex v1 = graph.AddVertex(null);
                    index.Put("dog", "puppy", v1);
                }
                Assert.AreEqual(10, index.Count("dog", "puppy"));
                Vertex v = (Vertex)index.Get("dog", "puppy").First();
                graph.RemoveVertex(v);
                index.Remove("dog", "puppy", v);
                Assert.AreEqual(9, index.Count("dog", "puppy"));

            }
            graph.Shutdown();
        }

        [Test]
        public void TestPutGetRemoveEdge()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeIndex.Value)
            {
                this.StopWatch();
                Index index = graph.CreateIndex("basic", typeof(Edge));
                PrintPerformance(graph.ToString(), 1, "manual index created", this.StopWatch());
                Vertex v1 = graph.AddVertex(null);
                Vertex v2 = graph.AddVertex(null);
                Edge e1 = graph.AddEdge(null, v1, v2, "test1");
                Edge e2 = graph.AddEdge(null, v1, v2, "test2");
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(Count(graph.GetEdges()), 2);

                this.StopWatch();
                index.Put("dog", "puppy", e1);
                index.Put("dog", "mama", e2);
                PrintPerformance(graph.ToString(), 2, "edges manually index", this.StopWatch());
                Assert.AreEqual(e1, index.Get("dog", "puppy").First());
                Assert.AreEqual(e2, index.Get("dog", "mama").First());

                v1.RemoveProperty("dog");
                Assert.AreEqual(e1, index.Get("dog", "puppy").First());
                Assert.AreEqual(e2, index.Get("dog", "mama").First());

                this.StopWatch();
                graph.RemoveEdge(e1);
                PrintPerformance(graph.ToString(), 1, "edge removed and automatically removed from index", this.StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(e2, index.Get("dog", "mama").First());
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(Count(graph.GetEdges()), 1);

                v2.SetProperty("dog", "mama2");
                Assert.AreEqual(e2, index.Get("dog", "mama").First());
                this.StopWatch();
                graph.RemoveEdge(e2);
                PrintPerformance(graph.ToString(), 1, "edge removed and automatically removed from index", this.StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(Count(index.Get("dog", "mama")), 0);
                if (graph.GetFeatures().SupportsEdgeIteration.Value)
                    Assert.AreEqual(Count(graph.GetEdges()), 0);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestCloseableSequence()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value)
            {
                Index index = graph.CreateIndex("basic", typeof(Vertex));
                for (int i = 0; i < 10; i++)
                {
                    Vertex v = graph.AddVertex(null);
                    index.Put("dog", "puppy", v);
                }
                CloseableIterable<Element> hits = index.Get("dog", "puppy");
                int counter = 0;
                foreach (Vertex v in hits)
                {
                    counter++;
                }
                Assert.AreEqual(counter, 10);
                hits.Dispose(); // no exception should be thrown

            }
            graph.Shutdown();
        }
    }
}
