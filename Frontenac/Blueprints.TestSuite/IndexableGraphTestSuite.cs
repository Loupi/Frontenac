using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Blueprints
{
    public abstract class IndexableGraphTestSuite : TestSuite
    {
        protected IndexableGraphTestSuite(GraphTest graphTest)
            : base("IndexableGraphTestSuite", graphTest)
        {
        }

        [Test]
        public void TestNoIndicesOnStartup()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex) return;

                Assert.AreEqual(Count(graph.GetIndices()), 0);
                graph.CreateIndex("myIdx", typeof (IVertex));
                Assert.AreEqual(Count(graph.GetIndices()), 1);

                // test to make sure its a semantically correct iterable
                var idx = graph.GetIndices().ToArray();
                Assert.AreEqual(Count(idx), 1);
                Assert.AreEqual(Count(idx), 1);
                Assert.AreEqual(Count(idx), 1);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestKeyIndicesAreNotIndices()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                Assert.AreEqual(Count(graph.GetIndices()), 0);
                if (!graph.Features.IsWrapper && graph.Features.SupportsKeyIndices &&
                    graph.Features.SupportsVertexKeyIndex)
                {
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("name", typeof (IVertex));
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("age", typeof (IVertex));
                    Assert.AreEqual(((IKeyIndexableGraph) graph).GetIndexedKeys(typeof (IVertex)).Count(), 2);
                }

                if (graph.Features.SupportsEdgeKeyIndex && !graph.Features.IsWrapper &&
                    graph.Features.SupportsEdgeKeyIndex)
                {
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("weight", typeof (IEdge));
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("since", typeof (IEdge));
                    Assert.AreEqual(((IKeyIndexableGraph) graph).GetIndexedKeys(typeof (IEdge)).Count(), 2);
                }
                Assert.AreEqual(Count(graph.GetIndices()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestCreateDropIndices()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex || !graph.Features.SupportsIndices) return;

                StopWatch();
                for (var i = 0; i < 10; i++)
                    graph.CreateIndex(i + "blah", typeof (IVertex));

                //Thread.Sleep(1000);
                Assert.AreEqual(10, Count(graph.GetIndices()));
                for (var i = 0; i < 10; i++)
                    graph.DropIndex(i + "blah");

                Assert.AreEqual(0, Count(graph.GetIndices()));
                PrintPerformance(graph.ToString(), 10, "indices created and then dropped", StopWatch());

                StopWatch();
                var index1 = graph.CreateIndex("index1", typeof (IVertex));
                var index2 = graph.CreateIndex("index2", typeof (IVertex));
                PrintPerformance(graph.ToString(), 2, "indices created", StopWatch());
                Assert.AreEqual(Count(graph.GetIndices()), 2);
                Assert.AreEqual(graph.GetIndex("index1", typeof (IVertex)).Name, "index1");
                Assert.AreEqual(graph.GetIndex("index2", typeof (IVertex)).Name, "index2");
                Assert.AreEqual(graph.GetIndex("index1", typeof (IVertex)).Type, typeof (IVertex));
                Assert.AreEqual(graph.GetIndex("index2", typeof (IVertex)).Type, typeof (IVertex));
                try
                {
                    Assert.AreEqual(graph.GetIndex("index1", typeof (IEdge)).Type, typeof (IEdge));
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                StopWatch();
                graph.DropIndex(index1.Name);
                Assert.Null(graph.GetIndex("index1", typeof (IVertex)));
                Assert.AreEqual(Count(graph.GetIndices()), 1);
                foreach (IIndex index in graph.GetIndices())
                    Assert.AreEqual(index.Name, index2.Name);

                graph.DropIndex(index2.Name);
                Assert.Null(graph.GetIndex("index1", typeof (IVertex)));
                Assert.Null(graph.GetIndex("index2", typeof (IVertex)));
                Assert.AreEqual(Count(graph.GetIndices()), 0);

                PrintPerformance(graph.ToString(), 2, "indices dropped and index iterable checked for consistency",
                                 StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestNonExistentIndices()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIndex || !graph.Features.SupportsEdgeIndex ||
                    !graph.Features.SupportsIndices) return;

                StopWatch();
                Assert.Null(graph.GetIndex("bloop", typeof (IVertex)));
                Assert.Null(graph.GetIndex("bam", typeof (IEdge)));
                Assert.Null(graph.GetIndex("blah blah", typeof (IEdge)));
                PrintPerformance(graph.ToString(), 3, "non-existent indices retrieved", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestIndexPersistence()
        {
            bool hasFeatures;
            IIndex manualIndex;
            IVertex vertex;
            object id = null;
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                hasFeatures = graph.Features.IsPersistent && graph.Features.SupportsVertexIndex &&
                              graph.Features.SupportsElementProperties() && graph.Features.SupportsIndices;

                if (hasFeatures)
                {
                    StopWatch();
                    graph.CreateIndex("testIndex", typeof (IVertex));
                    manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                    Assert.AreEqual(manualIndex.Name, "testIndex");
                    vertex = graph.AddVertex(null);
                    vertex.SetProperty("name", "marko");
                    id = vertex.Id;
                    manualIndex.Put("key", "value", vertex);
                    Thread.Sleep(1000);
                    Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                    Assert.AreEqual(manualIndex.Get("key", "value").First().Id, id);
                    PrintPerformance(graph.ToString(), 1, "index created and 1 vertex added and checked", StopWatch());
                }
            }
            finally
            {
                graph.Shutdown();
            }

            if (!hasFeatures) return;

            graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                Assert.AreEqual(manualIndex.Get("key", "value").First().Id, id);
                PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }

            graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                vertex = (IVertex) manualIndex.Get("key", "value").First();
                Assert.AreEqual(vertex.Id, id);
                graph.RemoveVertex(vertex);

                Assert.AreEqual(0, Count(manualIndex.Get("key", "value")));
                PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked and then removed",
                                 StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }

            graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                StopWatch();
                manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                Assert.AreEqual(Count(manualIndex.Get("key", "value")), 0);
                PrintPerformance(graph.ToString(), 1, "index reloaded and checked to ensure empty", StopWatch());
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestExceptionOnIndexOverwrite()
        {
            var loop = 0;
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsIndices && graph.Features.SupportsVertexIndex)
                {
                    loop = 1;
                    if (graph.Features.IsPersistent)
                        loop = 5;
                }
            }
            finally
            {
                graph.Shutdown();
            }

            if (loop <= 0) return;

            StopWatch();
            var graphName = "";
            for (var i = 0; i < loop; i++)
            {
                graph = (IIndexableGraph) GraphTest.GenerateGraph();
                try
                {
                    graph.CreateIndex(i + "atest", typeof (IVertex));
                    graphName = graph.ToString();
                    var counter = 0;
                    var exceptionCounter = 0;
                    foreach (var index in graph.GetIndices())
                    {
                        try
                        {
                            counter++;
                            graph.CreateIndex(index.Name, index.Type);
                        }
                        catch (Exception)
                        {
                            exceptionCounter++;
                        }
                    }
                    Assert.AreEqual(counter, exceptionCounter);
                    Assert.True(counter > 0);
                }
                finally
                {
                    graph.Shutdown();
                }
            }
            PrintPerformance(graphName, loop, "attempt(s) to overwrite existing indices", StopWatch());
        }

        [Test]
        public void TestIndexDropPersistence()
        {
            var graph = (IIndexableGraph) GraphTest.GenerateGraph();
            try
            {
                var hasFeatures = graph.Features.IsPersistent && graph.Features.SupportsIndices &&
                                  graph.Features.SupportsVertexIndex;
                if (!hasFeatures) return;

                graph.CreateIndex("blah", typeof (IVertex));
                graph.CreateIndex("bleep", typeof (IVertex));
                var indexNames = new HashSet<string>();
                foreach (var index in graph.GetIndices())
                {
                    indexNames.Add(index.Name);
                }
                Assert.AreEqual(Count(graph.GetIndices()), 2);
                Assert.AreEqual(Count(graph.GetIndices()), indexNames.Count);
                StopWatch();
                foreach (var indexName in indexNames)
                {
                    graph.DropIndex(indexName);
                }
                PrintPerformance(graph.ToString(), indexNames.Count, "indices dropped", StopWatch());
                Assert.AreEqual(Count(graph.GetIndices()), 0);

                var count = Count(graph.GetIndices());
                Assert.AreEqual(count, 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}