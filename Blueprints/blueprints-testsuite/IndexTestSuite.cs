using NUnit.Framework;
using System.Linq;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public abstract class IndexTestSuite : TestSuite
    {
        protected IndexTestSuite(GraphTest graphTest)
            : base("IndexTestSuite", graphTest)
        {

        }

        [Test]
        public void TestPutGetRemoveVertex()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex) return;

                StopWatch();
                var index = graph.CreateIndex("basic", typeof (IVertex));
                PrintPerformance(graph.ToString(), 1, "manual index created", StopWatch());
                var v1 = graph.AddVertex(null);
                var v2 = graph.AddVertex(null);

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 2);

                StopWatch();
                index.Put("dog", "puppy", v1);
                index.Put("dog", "mama", v2);
                PrintPerformance(graph.ToString(), 2, "vertices manually index", StopWatch());
                Assert.AreEqual(v1, index.Get("dog", "puppy").First());
                Assert.AreEqual(v2, index.Get("dog", "mama").First());
                Assert.AreEqual(1, index.Count("dog", "puppy"));

                v1.RemoveProperty("dog");
                Assert.AreEqual(v1, index.Get("dog", "puppy").First());
                Assert.AreEqual(v2, index.Get("dog", "mama").First());

                StopWatch();
                graph.RemoveVertex(v1);
                PrintPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index",
                                 StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(v2, index.Get("dog", "mama").First());

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 1);

                v2.SetProperty("dog", "mama2");
                Assert.AreEqual(v2, index.Get("dog", "mama").First());
                StopWatch();
                graph.RemoveVertex(v2);
                PrintPerformance(graph.ToString(), 1, "vertex removed and automatically removed from index",
                                 StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(Count(index.Get("dog", "mama")), 0);

                if (graph.Features.SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestIndexCount()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex) return;

                var index = graph.CreateIndex("basic", typeof (IVertex));
                for (var i = 0; i < 10; i++)
                {
                    var v1 = graph.AddVertex(null);
                    index.Put("dog", "puppy", v1);
                }
                Assert.AreEqual(10, index.Count("dog", "puppy"));
                var v = (IVertex) index.Get("dog", "puppy").First();
                graph.RemoveVertex(v);
                index.Remove("dog", "puppy", v);
                Assert.AreEqual(9, index.Count("dog", "puppy"));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestPutGetRemoveEdge()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIndex) return;

                StopWatch();
                var index = graph.CreateIndex("basic", typeof (IEdge));
                PrintPerformance(graph.ToString(), 1, "manual index created", StopWatch());
                var v1 = graph.AddVertex(null);
                var v2 = graph.AddVertex(null);
                var e1 = graph.AddEdge(null, v1, v2, "test1");
                var e2 = graph.AddEdge(null, v1, v2, "test2");

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 2);

                StopWatch();
                index.Put("dog", "puppy", e1);
                index.Put("dog", "mama", e2);
                PrintPerformance(graph.ToString(), 2, "edges manually index", StopWatch());
                Assert.AreEqual(e1, index.Get("dog", "puppy").First());
                Assert.AreEqual(e2, index.Get("dog", "mama").First());

                v1.RemoveProperty("dog");
                Assert.AreEqual(e1, index.Get("dog", "puppy").First());
                Assert.AreEqual(e2, index.Get("dog", "mama").First());

                StopWatch();
                graph.RemoveEdge(e1);
                PrintPerformance(graph.ToString(), 1, "edge removed and automatically removed from index",
                                 StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(e2, index.Get("dog", "mama").First());

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 1);

                v2.SetProperty("dog", "mama2");
                Assert.AreEqual(e2, index.Get("dog", "mama").First());
                StopWatch();
                graph.RemoveEdge(e2);
                PrintPerformance(graph.ToString(), 1, "edge removed and automatically removed from index",
                                 StopWatch());
                Assert.AreEqual(Count(index.Get("dog", "puppy")), 0);
                Assert.AreEqual(Count(index.Get("dog", "mama")), 0);

                if (graph.Features.SupportsEdgeIteration)
                    Assert.AreEqual(Count(graph.GetEdges()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestCloseableSequence()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex) return;

                var index = graph.CreateIndex("basic", typeof (IVertex));
                for (int i = 0; i < 10; i++)
                {
                    var v = graph.AddVertex(null);
                    index.Put("dog", "puppy", v);
                }
                var hits = index.Get("dog", "puppy");
                var counter = hits.Cast<IVertex>().Count();
                Assert.AreEqual(counter, 10);
                hits.Dispose();
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}
