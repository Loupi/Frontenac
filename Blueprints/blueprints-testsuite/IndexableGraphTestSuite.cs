using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public abstract class IndexableGraphTestSuite : TestSuite
    {
        public IndexableGraphTestSuite(GraphTest graphTest)
            : base("IndexableGraphTestSuite", graphTest)
        {

        }

        [Test]
        public void TestNoIndicesOnStartup()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value)
            {
                Assert.AreEqual(Count(graph.GetIndices()), 0);
                graph.CreateIndex("myIdx", typeof(Vertex));
                Assert.AreEqual(Count(graph.GetIndices()), 1);

                // test to make sure its a semantically correct iterable
                IEnumerable<Index> idx = graph.GetIndices();
                Assert.AreEqual(Count(idx), 1);
                Assert.AreEqual(Count(idx), 1);
                Assert.AreEqual(Count(idx), 1);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestKeyIndicesAreNotIndices()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            Assert.AreEqual(Count(graph.GetIndices()), 0);
            if (!graph.GetFeatures().IsWrapper.Value && graph.GetFeatures().SupportsKeyIndices.Value && graph.GetFeatures().SupportsVertexKeyIndex.Value)
            {
                ((KeyIndexableGraph)graph).CreateKeyIndex("name", typeof(Vertex));
                ((KeyIndexableGraph)graph).CreateKeyIndex("age", typeof(Vertex));
                Assert.AreEqual(((KeyIndexableGraph)graph).GetIndexedKeys(typeof(Vertex)).Count(), 2);
            }
            if (!graph.GetFeatures().IsWrapper.Value && graph.GetFeatures().SupportsKeyIndices.Value && graph.GetFeatures().SupportsEdgeKeyIndex.Value)
            {
                ((KeyIndexableGraph)graph).CreateKeyIndex("weight", typeof(Edge));
                ((KeyIndexableGraph)graph).CreateKeyIndex("since", typeof(Edge));
                Assert.AreEqual(((KeyIndexableGraph)graph).GetIndexedKeys(typeof(Edge)).Count(), 2);
            }
            Assert.AreEqual(Count(graph.GetIndices()), 0);
            graph.Shutdown();
        }

        [Test]
        public void TestCreateDropIndices()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value && graph.GetFeatures().SupportsIndices.Value)
            {
                this.StopWatch();
                for (int i = 0; i < 10; i++)
                    graph.CreateIndex(i + "blah", typeof(Vertex));

                Assert.AreEqual(Count(graph.GetIndices()), 10);
                for (int i = 0; i < 10; i++)
                    graph.DropIndex(i + "blah");

                Assert.AreEqual(Count(graph.GetIndices()), 0);
                PrintPerformance(graph.ToString(), 10, "indices created and then dropped", this.StopWatch());

                this.StopWatch();
                Index index1 = graph.CreateIndex("index1", typeof(Vertex));
                Index index2 = graph.CreateIndex("index2", typeof(Vertex));
                PrintPerformance(graph.ToString(), 2, "indices created", this.StopWatch());
                Assert.AreEqual(Count(graph.GetIndices()), 2);
                Assert.AreEqual(graph.GetIndex("index1", typeof(Vertex)).GetIndexName(), "index1");
                Assert.AreEqual(graph.GetIndex("index2", typeof(Vertex)).GetIndexName(), "index2");
                Assert.AreEqual(graph.GetIndex("index1", typeof(Vertex)).GetIndexClass(), typeof(Vertex));
                Assert.AreEqual(graph.GetIndex("index2", typeof(Vertex)).GetIndexClass(), typeof(Vertex));
                try
                {
                    Assert.AreEqual(graph.GetIndex("index1", typeof(Edge)).GetIndexClass(), typeof(Edge));
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                this.StopWatch();
                graph.DropIndex(index1.GetIndexName());
                Assert.Null(graph.GetIndex("index1", typeof(Vertex)));
                Assert.AreEqual(Count(graph.GetIndices()), 1);
                foreach (Index index in graph.GetIndices())
                    Assert.AreEqual(index.GetIndexName(), index2.GetIndexName());

                graph.DropIndex(index2.GetIndexName());
                Assert.Null(graph.GetIndex("index1", typeof(Vertex)));
                Assert.Null(graph.GetIndex("index2", typeof(Vertex)));
                Assert.AreEqual(Count(graph.GetIndices()), 0);

                PrintPerformance(graph.ToString(), 2, "indices dropped and index iterable checked for consistency", this.StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestNonExistentIndices()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value && graph.GetFeatures().SupportsEdgeIndex.Value && graph.GetFeatures().SupportsIndices.Value)
            {
                this.StopWatch();
                Assert.Null(graph.GetIndex("bloop", typeof(Vertex)));
                Assert.Null(graph.GetIndex("bam", typeof(Edge)));
                Assert.Null(graph.GetIndex("blah blah", typeof(Edge)));
                PrintPerformance(graph.ToString(), 3, "non-existent indices retrieved", this.StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestIndexPersistence()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().IsPersistent.Value && graph.GetFeatures().SupportsVertexIndex.Value && graph.GetFeatures().SupportsElementProperties() && graph.GetFeatures().SupportsIndices.Value)
            {
                this.StopWatch();
                graph.CreateIndex("testIndex", typeof(Vertex));
                Index manualIndex = graph.GetIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(manualIndex.GetIndexName(), "testIndex");
                Vertex vertex = graph.AddVertex(null);
                vertex.SetProperty("name", "marko");
                object id = vertex.GetId();
                manualIndex.Put("key", "value", vertex);
                Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                Assert.AreEqual(manualIndex.Get("key", "value").First().GetId(), id);
                PrintPerformance(graph.ToString(), 1, "index created and 1 vertex added and checked", this.StopWatch());
                graph.Shutdown();

                graph = (IndexableGraph)_GraphTest.GenerateGraph();
                this.StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                Assert.AreEqual(manualIndex.Get("key", "value").First().GetId(), id);
                PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked", this.StopWatch());
                graph.Shutdown();

                graph = (IndexableGraph)_GraphTest.GenerateGraph();
                this.StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof(Vertex));
                vertex = (Vertex)manualIndex.Get("key", "value").First();
                Assert.AreEqual(vertex.GetId(), id);
                graph.RemoveVertex(vertex);
                Assert.AreEqual(0, Count(manualIndex.Get("key", "value")));
                PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked and then removed", this.StopWatch());
                graph.Shutdown();

                graph = (IndexableGraph)_GraphTest.GenerateGraph();
                this.StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(Count(manualIndex.Get("key", "value")), 0);
                PrintPerformance(graph.ToString(), 1, "index reloaded and checked to ensure empty", this.StopWatch());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestExceptionOnIndexOverwrite()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsIndices.Value && graph.GetFeatures().SupportsVertexIndex.Value)
            {
                int loop = 1;
                if (graph.GetFeatures().IsPersistent.Value)
                    loop = 5;
                graph.Shutdown();
                this.StopWatch();
                string graphName = "";
                for (int i = 0; i < loop; i++)
                {
                    graph = (IndexableGraph)_GraphTest.GenerateGraph();
                    graph.CreateIndex(i + "atest", typeof(Vertex));
                    graphName = graph.ToString();
                    int counter = 0;
                    int exceptionCounter = 0;
                    foreach (Index index in graph.GetIndices())
                    {
                        try
                        {
                            counter++;
                            graph.CreateIndex(index.GetIndexName(), index.GetIndexClass());
                        }
                        catch (Exception)
                        {
                            exceptionCounter++;
                        }
                    }
                    Assert.AreEqual(counter, exceptionCounter);
                    Assert.True(counter > 0);
                    graph.Shutdown();
                }
                PrintPerformance(graphName, loop, "attempt(s) to overwrite existing indices", this.StopWatch());
            }

            graph.Shutdown();
        }

        [Test]
        public void TestIndexDropPersistence()
        {
            IndexableGraph graph = (IndexableGraph)_GraphTest.GenerateGraph();
            if (graph.GetFeatures().IsPersistent.Value && graph.GetFeatures().SupportsIndices.Value && graph.GetFeatures().SupportsVertexIndex.Value)
            {
                graph.CreateIndex("blah", typeof(Vertex));
                graph.CreateIndex("bleep", typeof(Vertex));
                HashSet<string> indexNames = new HashSet<string>();
                foreach (Index index in graph.GetIndices())
                {
                    indexNames.Add(index.GetIndexName());
                }
                Assert.AreEqual(Count(graph.GetIndices()), 2);
                Assert.AreEqual(Count(graph.GetIndices()), indexNames.Count());
                this.StopWatch();
                foreach (string indexName in indexNames)
                {
                    graph.DropIndex(indexName);
                }
                PrintPerformance(graph.ToString(), indexNames.Count(), "indices dropped", this.StopWatch());
                Assert.AreEqual(Count(graph.GetIndices()), 0);
                graph.Shutdown();

                graph = (IndexableGraph)_GraphTest.GenerateGraph();
                Assert.AreEqual(Count(graph.GetIndices()), 0);
            }
            graph.Shutdown();
        }
    }
}
