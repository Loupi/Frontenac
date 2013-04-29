using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System.Linq;

namespace Frontenac.Blueprints
{
    public abstract class KeyIndexableGraphTestSuite : TestSuite
    {
        protected KeyIndexableGraphTestSuite(GraphTest graphTest)
            : base("KeyIndexableGraphTestSuite", graphTest)
        {
        }

        [Test]
        public void TestAutoIndexKeyManagementWithPersistence()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexKeyIndex)
            {
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 0);
                StopWatch();
                graph.CreateKeyIndex("name", typeof(IVertex));
                graph.CreateKeyIndex("location", typeof(IVertex));
                PrintPerformance(graph.ToString(), 2, "automatic index keys added", StopWatch());
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 2);
                Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("name"));
                Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("location"));
            }
            if (graph.GetFeatures().SupportsEdgeKeyIndex)
            {
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 0);
                StopWatch();
                graph.CreateKeyIndex("weight", typeof(IEdge));
                graph.CreateKeyIndex("since", typeof(IEdge));
                PrintPerformance(graph.ToString(), 2, "automatic index keys added", StopWatch());
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 2);
                Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("weight"));
                Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("since"));
            }
            graph.Shutdown();

            if (graph.GetFeatures().IsPersistent)
            {
                graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
                if (graph.GetFeatures().SupportsVertexKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 2);
                    Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("name"));
                    Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("location"));
                }
                
                if (graph.GetFeatures().SupportsEdgeKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 2);
                    Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("weight"));
                    Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("since"));
                }
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAutoIndexKeyDroppingWithPersistence()
        {
            TestAutoIndexKeyManagementWithPersistence();
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
            if (graph.GetFeatures().IsPersistent)
            {
                if (graph.GetFeatures().SupportsVertexKeyIndex)
                {
                    graph.DropKeyIndex("name", typeof(IVertex));
                }

                if (graph.GetFeatures().SupportsEdgeKeyIndex)
                {
                    graph.DropKeyIndex("weight", typeof(IEdge));
                }
                graph.Shutdown();

                graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();

                if (graph.GetFeatures().SupportsVertexKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 1);
                    Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("location"));
                    graph.DropKeyIndex("location", typeof(IVertex));
                }

                if (graph.GetFeatures().SupportsEdgeKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 1);
                    Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("since"));
                    graph.DropKeyIndex("since", typeof(IEdge));
                }
                graph.Shutdown();
                graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();

                if (graph.GetFeatures().SupportsVertexKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 0);
                }

                if (graph.GetFeatures().SupportsEdgeKeyIndex)
                {
                    Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 0);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGettingVerticesAndEdgesWithKeyValue()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
            
            if (graph.GetFeatures().SupportsVertexIteration && graph.GetFeatures().SupportsVertexKeyIndex)
            {
                graph.CreateKeyIndex("name", typeof(IVertex));
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IVertex)).Count(), 1);
                Assert.True(graph.GetIndexedKeys(typeof(IVertex)).Contains("name"));
                IVertex v1 = graph.AddVertex(null);
                v1.SetProperty("name", "marko");
                v1.SetProperty("location", "everywhere");
                IVertex v2 = graph.AddVertex(null);
                v2.SetProperty("name", "stephen");
                v2.SetProperty("location", "everywhere");

                Assert.AreEqual(Count(graph.GetVertices("location", "everywhere")), 2);
                Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
                Assert.AreEqual(Count(graph.GetVertices("name", "stephen")), 1);
                Assert.AreEqual(graph.GetVertices("name", "marko").First(), v1);
                Assert.AreEqual(graph.GetVertices("name", "stephen").First(), v2);
            }

            if (graph.GetFeatures().SupportsEdgeIteration && graph.GetFeatures().SupportsEdgeKeyIndex)
            {
                graph.CreateKeyIndex("place", typeof(IEdge));
                Assert.AreEqual(graph.GetIndexedKeys(typeof(IEdge)).Count(), 1);
                Assert.True(graph.GetIndexedKeys(typeof(IEdge)).Contains("place"));

                IEdge e1 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                e1.SetProperty("name", "marko");
                e1.SetProperty("place", "everywhere");
                IEdge e2 = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                e2.SetProperty("name", "stephen");
                e2.SetProperty("place", "everywhere");

                Assert.AreEqual(Count(graph.GetEdges("place", "everywhere")), 2);
                Assert.AreEqual(Count(graph.GetEdges("name", "marko")), 1);
                Assert.AreEqual(Count(graph.GetEdges("name", "stephen")), 1);
                Assert.AreEqual(graph.GetEdges("name", "marko").First(), e1);
                Assert.AreEqual(graph.GetEdges("name", "stephen").First(), e2);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestReIndexingOfElements()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexKeyIndex)
            {
                IVertex vertex = graph.AddVertex(null);
                vertex.SetProperty("name", "marko");
                Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
                Assert.AreEqual(graph.GetVertices("name", "marko").First(), vertex);
                graph.CreateKeyIndex("name", typeof(IVertex));
                Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
                Assert.AreEqual(graph.GetVertices("name", "marko").First(), vertex);
            }

            if (graph.GetFeatures().SupportsEdgeKeyIndex)
            {
                IEdge edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows");
                edge.SetProperty("date", 2012);
                Assert.AreEqual(Count(graph.GetEdges("date", 2012)), 1);
                Assert.AreEqual(graph.GetEdges("date", 2012).First(), edge);
                graph.CreateKeyIndex("date", typeof(IEdge));
                Assert.AreEqual(Count(graph.GetEdges("date", 2012)), 1);
                Assert.AreEqual(graph.GetEdges("date", 2012).First(), edge);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestNoConcurrentModificationException()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeKeyIndex)
            {
                graph.CreateKeyIndex("key", typeof(IEdge));
                for (int i = 0; i < 25; i++)
                {
                    graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "test").SetProperty("key", "value");
                }

                if (graph.GetFeatures().SupportsVertexIteration) Assert.AreEqual(Count(graph.GetVertices()), 50);
                if (graph.GetFeatures().SupportsEdgeIteration) Assert.AreEqual(Count(graph.GetEdges()), 25);
                int counter = 0;
                foreach (var edge in graph.GetEdges("key", "value"))
                {
                    graph.RemoveEdge(edge);
                    counter++;
                }
                Assert.AreEqual(counter, 25);
                if (graph.GetFeatures().SupportsVertexIteration) Assert.AreEqual(Count(graph.GetVertices()), 50);
                if (graph.GetFeatures().SupportsEdgeIteration) Assert.AreEqual(Count(graph.GetEdges()), 0);

            }
            graph.Shutdown();
        }

        [Test]
        public void TestKeyIndicesConsistentWithElementRemoval()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();

            graph.CreateKeyIndex("foo", typeof(IVertex));

            IVertex v1 = graph.AddVertex(null);
            v1.SetProperty("foo", 42);
            VertexCount(graph, 1);

            graph.RemoveVertex(v1);
            VertexCount(graph, 0);
            Assert.AreEqual(0, Count(graph.GetVertices("foo", 42)));

            graph.Shutdown();
        }
    }
}
