using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;

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
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                if (graph.GetFeatures().SupportsVertexIndex)
                {
                    Assert.AreEqual(Count(graph.GetIndices()), 0);
                    graph.CreateIndex("myIdx", typeof (IVertex));
                    Assert.AreEqual(Count(graph.GetIndices()), 1);

                    // test to make sure its a semantically correct iterable
                    IEnumerable<IIndex> idx = graph.GetIndices();
                    Assert.AreEqual(Count(idx), 1);
                    Assert.AreEqual(Count(idx), 1);
                    Assert.AreEqual(Count(idx), 1);
                }
            }
        }

        [Test]
        public void TestKeyIndicesAreNotIndices()
        {
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                Assert.AreEqual(Count(graph.GetIndices()), 0);
                if (!graph.GetFeatures().IsWrapper && graph.GetFeatures().SupportsKeyIndices &&
                    graph.GetFeatures().SupportsVertexKeyIndex)
                {
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("name", typeof (IVertex));
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("age", typeof (IVertex));
                    Assert.AreEqual(((IKeyIndexableGraph) graph).GetIndexedKeys(typeof (IVertex)).Count(), 2);
                }

                if (graph.GetFeatures().SupportsEdgeKeyIndex && !graph.GetFeatures().IsWrapper &&
                    graph.GetFeatures().SupportsEdgeKeyIndex)
                {
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("weight", typeof (IEdge));
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("since", typeof (IEdge));
                    Assert.AreEqual(((IKeyIndexableGraph) graph).GetIndexedKeys(typeof (IEdge)).Count(), 2);
                }
                Assert.AreEqual(Count(graph.GetIndices()), 0);
            }
        }

        [Test]
        public void TestCreateDropIndices()
        {
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                if (graph.GetFeatures().SupportsVertexIndex && graph.GetFeatures().SupportsIndices)
                {
                    StopWatch();
                    for (int i = 0; i < 10; i++)
                        graph.CreateIndex(i + "blah", typeof (IVertex));

                    Assert.AreEqual(Count(graph.GetIndices()), 10);
                    for (int i = 0; i < 10; i++)
                        graph.DropIndex(i + "blah");

                    Assert.AreEqual(Count(graph.GetIndices()), 0);
                    PrintPerformance(graph.ToString(), 10, "indices created and then dropped", StopWatch());

                    StopWatch();
                    var index1 = graph.CreateIndex("index1", typeof (IVertex));
                    var index2 = graph.CreateIndex("index2", typeof (IVertex));
                    PrintPerformance(graph.ToString(), 2, "indices created", StopWatch());
                    Assert.AreEqual(Count(graph.GetIndices()), 2);
                    Assert.AreEqual(graph.GetIndex("index1", typeof (IVertex)).GetIndexName(), "index1");
                    Assert.AreEqual(graph.GetIndex("index2", typeof (IVertex)).GetIndexName(), "index2");
                    Assert.AreEqual(graph.GetIndex("index1", typeof (IVertex)).GetIndexClass(), typeof (IVertex));
                    Assert.AreEqual(graph.GetIndex("index2", typeof (IVertex)).GetIndexClass(), typeof (IVertex));
                    try
                    {
                        Assert.AreEqual(graph.GetIndex("index1", typeof (IEdge)).GetIndexClass(), typeof (IEdge));
                        Assert.False(true);
                    }
                    catch (Exception)
                    {
                        Assert.True(true);
                    }

                    StopWatch();
                    graph.DropIndex(index1.GetIndexName());
                    Assert.Null(graph.GetIndex("index1", typeof (IVertex)));
                    Assert.AreEqual(Count(graph.GetIndices()), 1);
                    foreach (IIndex index in graph.GetIndices())
                        Assert.AreEqual(index.GetIndexName(), index2.GetIndexName());

                    graph.DropIndex(index2.GetIndexName());
                    Assert.Null(graph.GetIndex("index1", typeof (IVertex)));
                    Assert.Null(graph.GetIndex("index2", typeof (IVertex)));
                    Assert.AreEqual(Count(graph.GetIndices()), 0);

                    PrintPerformance(graph.ToString(), 2, "indices dropped and index iterable checked for consistency",
                                     StopWatch());
                }
            }
        }

        [Test]
        public void TestNonExistentIndices()
        {
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                if (graph.GetFeatures().SupportsVertexIndex && graph.GetFeatures().SupportsEdgeIndex &&
                    graph.GetFeatures().SupportsIndices)
                {
                    StopWatch();
                    Assert.Null(graph.GetIndex("bloop", typeof (IVertex)));
                    Assert.Null(graph.GetIndex("bam", typeof (IEdge)));
                    Assert.Null(graph.GetIndex("blah blah", typeof (IEdge)));
                    PrintPerformance(graph.ToString(), 3, "non-existent indices retrieved", StopWatch());
                }
            }
        }

        [Test]
        public void TestIndexPersistence()
        {
            bool hasFeatures;
            IIndex manualIndex;
            IVertex vertex;
            object id = null;
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                hasFeatures = (graph.GetFeatures().IsPersistent && graph.GetFeatures().SupportsVertexIndex &&
                               graph.GetFeatures().SupportsElementProperties() && graph.GetFeatures().SupportsIndices);

                if (hasFeatures)
                {
                    StopWatch();
                    graph.CreateIndex("testIndex", typeof (IVertex));
                    manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                    Assert.AreEqual(manualIndex.GetIndexName(), "testIndex");
                    vertex = graph.AddVertex(null);
                    vertex.SetProperty("name", "marko");
                    id = vertex.GetId();
                    manualIndex.Put("key", "value", vertex);
                    Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                    Assert.AreEqual(manualIndex.Get("key", "value").First().GetId(), id);
                    PrintPerformance(graph.ToString(), 1, "index created and 1 vertex added and checked", StopWatch());
                }
            }

            if (hasFeatures)
            {
                using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
                {
                    StopWatch();
                    manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                    Assert.AreEqual(Count(manualIndex.Get("key", "value")), 1);
                    Assert.AreEqual(manualIndex.Get("key", "value").First().GetId(), id);
                    PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked", StopWatch());
                }

                using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
                {
                    StopWatch();
                    manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                    vertex = (IVertex) manualIndex.Get("key", "value").First();
                    Assert.AreEqual(vertex.GetId(), id);
                    graph.RemoveVertex(vertex);
                    Assert.AreEqual(0, Count(manualIndex.Get("key", "value")));
                    PrintPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked and then removed",
                                     StopWatch());
                }

                using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
                {
                    StopWatch();
                    manualIndex = graph.GetIndex("testIndex", typeof (IVertex));
                    Assert.AreEqual(Count(manualIndex.Get("key", "value")), 0);
                    PrintPerformance(graph.ToString(), 1, "index reloaded and checked to ensure empty", StopWatch());

                }
            }
        }

        [Test]
        public void TestExceptionOnIndexOverwrite()
        {
            int loop = 0;
            using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
            {
                if(graph.GetFeatures().SupportsIndices && graph.GetFeatures().SupportsVertexIndex)
                {
                    loop = 1;
                    if (graph.GetFeatures().IsPersistent)
                        loop = 5;
                }
            }

            if(loop > 0)
            {
                StopWatch();
                string graphName = "";
                for (int i = 0; i < loop; i++)
                {
                    using (var graph = (IIndexableGraph) GraphTest.GenerateGraph())
                    {
                        graph.CreateIndex(i + "atest", typeof (IVertex));
                        graphName = graph.ToString();
                        int counter = 0;
                        int exceptionCounter = 0;
                        foreach (IIndex index in graph.GetIndices())
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
                    }
                }
                PrintPerformance(graphName, loop, "attempt(s) to overwrite existing indices", StopWatch());
            }
        }

        [Test]
        public void TestIndexDropPersistence()
        {
            bool hasFeatures;
            int count = 0;
            using (var graph = (IIndexableGraph)GraphTest.GenerateGraph())
            {
                hasFeatures = (graph.GetFeatures().IsPersistent && graph.GetFeatures().SupportsIndices &&
                                    graph.GetFeatures().SupportsVertexIndex);
                if (hasFeatures)
                {
                    graph.CreateIndex("blah", typeof (IVertex));
                    graph.CreateIndex("bleep", typeof (IVertex));
                    var indexNames = new HashSet<string>();
                    foreach (IIndex index in graph.GetIndices())
                    {
                        indexNames.Add(index.GetIndexName());
                    }
                    Assert.AreEqual(Count(graph.GetIndices()), 2);
                    Assert.AreEqual(Count(graph.GetIndices()), indexNames.Count());
                    StopWatch();
                    foreach (string indexName in indexNames)
                    {
                        graph.DropIndex(indexName);
                    }
                    PrintPerformance(graph.ToString(), indexNames.Count(), "indices dropped", StopWatch());
                    Assert.AreEqual(Count(graph.GetIndices()), 0);

                    count = Count(graph.GetIndices());
                }
            }
            if (hasFeatures)
            {
                using (var graph2 = (IIndexableGraph) GraphTest.GenerateGraph())
                {
                    Assert.AreEqual(count, 0);
                }
            }
        }
    }
}
