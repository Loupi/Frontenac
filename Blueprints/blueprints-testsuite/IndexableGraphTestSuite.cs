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
        public void testNoIndicesOnStartup()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value)
            {
                Assert.AreEqual(count(graph.getIndices()), 0);
                graph.createIndex("myIdx", typeof(Vertex));
                Assert.AreEqual(count(graph.getIndices()), 1);

                // test to make sure its a semantically correct iterable
                IEnumerable<Index> idx = graph.getIndices();
                Assert.AreEqual(count(idx), 1);
                Assert.AreEqual(count(idx), 1);
                Assert.AreEqual(count(idx), 1);
            }
            graph.shutdown();
        }

        [Test]
        public void testKeyIndicesAreNotIndices()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            Assert.AreEqual(count(graph.getIndices()), 0);
            if (!graph.getFeatures().isWrapper.Value && graph.getFeatures().supportsKeyIndices.Value && graph.getFeatures().supportsVertexKeyIndex.Value)
            {
                ((KeyIndexableGraph)graph).createKeyIndex("name", typeof(Vertex));
                ((KeyIndexableGraph)graph).createKeyIndex("age", typeof(Vertex));
                Assert.AreEqual(((KeyIndexableGraph)graph).getIndexedKeys(typeof(Vertex)).Count(), 2);
            }
            if (!graph.getFeatures().isWrapper.Value && graph.getFeatures().supportsKeyIndices.Value && graph.getFeatures().supportsEdgeKeyIndex.Value)
            {
                ((KeyIndexableGraph)graph).createKeyIndex("weight", typeof(Edge));
                ((KeyIndexableGraph)graph).createKeyIndex("since", typeof(Edge));
                Assert.AreEqual(((KeyIndexableGraph)graph).getIndexedKeys(typeof(Edge)).Count(), 2);
            }
            Assert.AreEqual(count(graph.getIndices()), 0);
            graph.shutdown();
        }

        [Test]
        public void testCreateDropIndices()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value && graph.getFeatures().supportsIndices.Value)
            {
                this.stopWatch();
                for (int i = 0; i < 10; i++)
                    graph.createIndex(i + "blah", typeof(Vertex));

                Assert.AreEqual(count(graph.getIndices()), 10);
                for (int i = 0; i < 10; i++)
                    graph.dropIndex(i + "blah");

                Assert.AreEqual(count(graph.getIndices()), 0);
                printPerformance(graph.ToString(), 10, "indices created and then dropped", this.stopWatch());

                this.stopWatch();
                Index index1 = graph.createIndex("index1", typeof(Vertex));
                Index index2 = graph.createIndex("index2", typeof(Vertex));
                printPerformance(graph.ToString(), 2, "indices created", this.stopWatch());
                Assert.AreEqual(count(graph.getIndices()), 2);
                Assert.AreEqual(graph.getIndex("index1", typeof(Vertex)).getIndexName(), "index1");
                Assert.AreEqual(graph.getIndex("index2", typeof(Vertex)).getIndexName(), "index2");
                Assert.AreEqual(graph.getIndex("index1", typeof(Vertex)).getIndexClass(), typeof(Vertex));
                Assert.AreEqual(graph.getIndex("index2", typeof(Vertex)).getIndexClass(), typeof(Vertex));
                try
                {
                    Assert.AreEqual(graph.getIndex("index1", typeof(Edge)).getIndexClass(), typeof(Edge));
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                this.stopWatch();
                graph.dropIndex(index1.getIndexName());
                Assert.Null(graph.getIndex("index1", typeof(Vertex)));
                Assert.AreEqual(count(graph.getIndices()), 1);
                foreach (Index index in graph.getIndices())
                    Assert.AreEqual(index.getIndexName(), index2.getIndexName());

                graph.dropIndex(index2.getIndexName());
                Assert.Null(graph.getIndex("index1", typeof(Vertex)));
                Assert.Null(graph.getIndex("index2", typeof(Vertex)));
                Assert.AreEqual(count(graph.getIndices()), 0);

                printPerformance(graph.ToString(), 2, "indices dropped and index iterable checked for consistency", this.stopWatch());
            }
            graph.shutdown();
        }

        [Test]
        public void testNonExistentIndices()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value && graph.getFeatures().supportsEdgeIndex.Value && graph.getFeatures().supportsIndices.Value)
            {
                this.stopWatch();
                Assert.Null(graph.getIndex("bloop", typeof(Vertex)));
                Assert.Null(graph.getIndex("bam", typeof(Edge)));
                Assert.Null(graph.getIndex("blah blah", typeof(Edge)));
                printPerformance(graph.ToString(), 3, "non-existent indices retrieved", this.stopWatch());
            }
            graph.shutdown();
        }

        [Test]
        public void testIndexPersistence()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().isPersistent.Value && graph.getFeatures().supportsVertexIndex.Value && graph.getFeatures().supportsElementProperties() && graph.getFeatures().supportsIndices.Value)
            {
                this.stopWatch();
                graph.createIndex("testIndex", typeof(Vertex));
                Index manualIndex = graph.getIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(manualIndex.getIndexName(), "testIndex");
                Vertex vertex = graph.addVertex(null);
                vertex.setProperty("name", "marko");
                object id = vertex.getId();
                manualIndex.put("key", "value", vertex);
                Assert.AreEqual(count(manualIndex.get("key", "value")), 1);
                Assert.AreEqual(manualIndex.get("key", "value").First().getId(), id);
                printPerformance(graph.ToString(), 1, "index created and 1 vertex added and checked", this.stopWatch());
                graph.shutdown();

                graph = (IndexableGraph)graphTest.generateGraph();
                this.stopWatch();
                manualIndex = graph.getIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(count(manualIndex.get("key", "value")), 1);
                Assert.AreEqual(manualIndex.get("key", "value").First().getId(), id);
                printPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked", this.stopWatch());
                graph.shutdown();

                graph = (IndexableGraph)graphTest.generateGraph();
                this.stopWatch();
                manualIndex = graph.getIndex("testIndex", typeof(Vertex));
                vertex = (Vertex)manualIndex.get("key", "value").First();
                Assert.AreEqual(vertex.getId(), id);
                graph.removeVertex(vertex);
                Assert.AreEqual(0, count(manualIndex.get("key", "value")));
                printPerformance(graph.ToString(), 1, "index reloaded and 1 vertex checked and then removed", this.stopWatch());
                graph.shutdown();

                graph = (IndexableGraph)graphTest.generateGraph();
                this.stopWatch();
                manualIndex = graph.getIndex("testIndex", typeof(Vertex));
                Assert.AreEqual(count(manualIndex.get("key", "value")), 0);
                printPerformance(graph.ToString(), 1, "index reloaded and checked to ensure empty", this.stopWatch());
            }
            graph.shutdown();
        }

        [Test]
        public void testExceptionOnIndexOverwrite()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsIndices.Value && graph.getFeatures().supportsVertexIndex.Value)
            {
                int loop = 1;
                if (graph.getFeatures().isPersistent.Value)
                    loop = 5;
                graph.shutdown();
                this.stopWatch();
                string graphName = "";
                for (int i = 0; i < loop; i++)
                {
                    graph = (IndexableGraph)graphTest.generateGraph();
                    graph.createIndex(i + "atest", typeof(Vertex));
                    graphName = graph.ToString();
                    int counter = 0;
                    int exceptionCounter = 0;
                    foreach (Index index in graph.getIndices())
                    {
                        try
                        {
                            counter++;
                            graph.createIndex(index.getIndexName(), index.getIndexClass());
                        }
                        catch (Exception)
                        {
                            exceptionCounter++;
                        }
                    }
                    Assert.AreEqual(counter, exceptionCounter);
                    Assert.True(counter > 0);
                    graph.shutdown();
                }
                printPerformance(graphName, loop, "attempt(s) to overwrite existing indices", this.stopWatch());
            }

            graph.shutdown();
        }

        [Test]
        public void testIndexDropPersistence()
        {
            IndexableGraph graph = (IndexableGraph)graphTest.generateGraph();
            if (graph.getFeatures().isPersistent.Value && graph.getFeatures().supportsIndices.Value && graph.getFeatures().supportsVertexIndex.Value)
            {
                graph.createIndex("blah", typeof(Vertex));
                graph.createIndex("bleep", typeof(Vertex));
                HashSet<string> indexNames = new HashSet<string>();
                foreach (Index index in graph.getIndices())
                {
                    indexNames.Add(index.getIndexName());
                }
                Assert.AreEqual(count(graph.getIndices()), 2);
                Assert.AreEqual(count(graph.getIndices()), indexNames.Count());
                this.stopWatch();
                foreach (string indexName in indexNames)
                {
                    graph.dropIndex(indexName);
                }
                printPerformance(graph.ToString(), indexNames.Count(), "indices dropped", this.stopWatch());
                Assert.AreEqual(count(graph.getIndices()), 0);
                graph.shutdown();

                graph = (IndexableGraph)graphTest.generateGraph();
                Assert.AreEqual(count(graph.getIndices()), 0);
            }
            graph.shutdown();
        }
    }
}
