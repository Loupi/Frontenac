using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public abstract class TransactionalGraphTestSuite : TestSuite
    {
        protected TransactionalGraphTestSuite(GraphTest graphTest)
            : base("TransactionalGraphTestSuite", graphTest)
        {
        }

        [Test]
        public void testRepeatedTransactionStopException()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            graph.commit();
            graph.rollback();
            graph.commit();
            graph.shutdown();
        }

        [Test]
        public void testAutoStartTransaction()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            Vertex v1 = graph.addVertex(null);
            vertexCount(graph, 1);
            Assert.AreEqual(v1.getId(), graph.getVertex(v1.getId()).getId());
            graph.commit();
            vertexCount(graph, 1);
            Assert.AreEqual(v1.getId(), graph.getVertex(v1.getId()).getId());
            graph.shutdown();
        }

        [Test]
        public void testTransactionsForVertices()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            List<Vertex> vin = new List<Vertex>();
            List<Vertex> vout = new List<Vertex>();
            vin.Add(graph.addVertex(null));
            graph.commit();
            vertexCount(graph, 1);
            containsVertices(graph, vin);

            this.stopWatch();
            vout.Add(graph.addVertex(null));
            vertexCount(graph, 2);
            containsVertices(graph, vin);
            containsVertices(graph, vout);
            graph.rollback();

            containsVertices(graph, vin);
            vertexCount(graph, 1);
            printPerformance(graph.ToString(), 1, "vertex not added in failed transaction", this.stopWatch());

            this.stopWatch();
            vin.Add(graph.addVertex(null));
            vertexCount(graph, 2);
            containsVertices(graph, vin);
            graph.commit();
            printPerformance(graph.ToString(), 1, "vertex added in successful transaction", this.stopWatch());
            vertexCount(graph, 2);
            containsVertices(graph, vin);

            graph.shutdown();
        }

        [Test]
        public void testBasicVertexEdgeTransactions()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            Vertex v = graph.addVertex(null);
            graph.addEdge(null, v, v, convertId(graph, "self"));
            Assert.AreEqual(count(v.getEdges(Direction.IN)), 1);
            Assert.AreEqual(count(v.getEdges(Direction.OUT)), 1);
            Assert.AreEqual(v.getEdges(Direction.IN).First(), v.getEdges(Direction.OUT).First());
            graph.commit();
            v = graph.getVertex(v.getId());
            Assert.AreEqual(count(v.getEdges(Direction.IN)), 1);
            Assert.AreEqual(count(v.getEdges(Direction.OUT)), 1);
            Assert.AreEqual(v.getEdges(Direction.IN).First(), v.getEdges(Direction.OUT).First());
            graph.commit();
            v = graph.getVertex(v.getId());
            Assert.AreEqual(count(v.getVertices(Direction.IN)), 1);
            Assert.AreEqual(count(v.getVertices(Direction.OUT)), 1);
            Assert.AreEqual(v.getVertices(Direction.IN).First(), v.getVertices(Direction.OUT).First());
            graph.commit();
            graph.shutdown();
        }

        [Test]
        public void testBruteVertexTransactions()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            List<Vertex> vin = new List<Vertex>(), vout = new List<Vertex>();
            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                vin.Add(graph.addVertex(null));
                graph.commit();
            }
            printPerformance(graph.ToString(), 100, "vertices added in 100 successful transactions", this.stopWatch());
            vertexCount(graph, 100);
            containsVertices(graph, vin);

            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                vout.Add(graph.addVertex(null));
                graph.rollback();
            }
            printPerformance(graph.ToString(), 100, "vertices not added in 100 failed transactions", this.stopWatch());

            vertexCount(graph, 100);
            containsVertices(graph, vin);
            graph.rollback();
            vertexCount(graph, 100);
            containsVertices(graph, vin);


            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                vin.Add(graph.addVertex(null));
            }
            vertexCount(graph, 200);
            containsVertices(graph, vin);
            graph.commit();
            printPerformance(graph.ToString(), 100, "vertices added in 1 successful transactions", this.stopWatch());
            vertexCount(graph, 200);
            containsVertices(graph, vin);

            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                vout.Add(graph.addVertex(null));
            }
            vertexCount(graph, 300);
            containsVertices(graph, vin);
            containsVertices(graph, vout.GetRange(100, 100));
            graph.rollback();
            printPerformance(graph.ToString(), 100, "vertices not added in 1 failed transactions", this.stopWatch());
            vertexCount(graph, 200);
            containsVertices(graph, vin);
            graph.shutdown();
        }

        [Test]
        public void testTransactionsForEdges()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();

            Vertex v = graph.addVertex(null);
            Vertex u = graph.addVertex(null);
            graph.commit();

            this.stopWatch();
            Edge e = graph.addEdge(null, graph.getVertex(v.getId()), graph.getVertex(u.getId()), convertId(graph, "test"));


            Assert.AreEqual(graph.getVertex(v.getId()), v);
            Assert.AreEqual(graph.getVertex(u.getId()), u);
            if (graph.getFeatures().supportsEdgeRetrieval.Value)
                Assert.AreEqual(graph.getEdge(e.getId()), e);

            vertexCount(graph, 2);
            edgeCount(graph, 1);

            graph.rollback();
            printPerformance(graph.ToString(), 1, "edge not added in failed transaction (w/ iteration)", this.stopWatch());

            Assert.AreEqual(graph.getVertex(v.getId()), v);
            Assert.AreEqual(graph.getVertex(u.getId()), u);
            if (graph.getFeatures().supportsEdgeRetrieval.Value)
                Assert.Null(graph.getEdge(e.getId()));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 2);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 0);

            this.stopWatch();

            e = graph.addEdge(null, graph.getVertex(u.getId()), graph.getVertex(v.getId()), convertId(graph, "test"));

            Assert.AreEqual(graph.getVertex(v.getId()), v);
            Assert.AreEqual(graph.getVertex(u.getId()), u);
            if (graph.getFeatures().supportsEdgeRetrieval.Value)
                Assert.AreEqual(graph.getEdge(e.getId()), e);

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 2);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 1);
            Assert.AreEqual(e, getOnlyElement(graph.getVertex(u.getId()).getEdges(Direction.OUT)));
            graph.commit();
            printPerformance(graph.ToString(), 1, "edge added in successful transaction (w/ iteration)", this.stopWatch());

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 2);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 1);

            Assert.AreEqual(graph.getVertex(v.getId()), v);
            Assert.AreEqual(graph.getVertex(u.getId()), u);
            if (graph.getFeatures().supportsEdgeRetrieval.Value)
                Assert.AreEqual(graph.getEdge(e.getId()), e);
            Assert.AreEqual(e, getOnlyElement(graph.getVertex(u.getId()).getEdges(Direction.OUT)));

            graph.shutdown();
        }

        [Test]
        public void testBruteEdgeTransactions()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                Vertex v = graph.addVertex(null);
                Vertex u = graph.addVertex(null);
                graph.addEdge(null, v, u, convertId(graph, "test"));
                graph.commit();
            }
            printPerformance(graph.ToString(), 100, "edges added in 100 successful transactions (2 vertices added for each edge)", this.stopWatch());
            vertexCount(graph, 200);
            edgeCount(graph, 100);

            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                Vertex v = graph.addVertex(null);
                Vertex u = graph.addVertex(null);
                graph.addEdge(null, v, u, convertId(graph, "test"));
                graph.rollback();
            }
            printPerformance(graph.ToString(), 100, "edges not added in 100 failed transactions (2 vertices added for each edge)", this.stopWatch());
            vertexCount(graph, 200);
            edgeCount(graph, 100);

            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                Vertex v = graph.addVertex(null);
                Vertex u = graph.addVertex(null);
                graph.addEdge(null, v, u, convertId(graph, "test"));
            }
            vertexCount(graph, 400);
            edgeCount(graph, 200);
            graph.commit();
            printPerformance(graph.ToString(), 100, "edges added in 1 successful transactions (2 vertices added for each edge)", this.stopWatch());
            vertexCount(graph, 400);
            edgeCount(graph, 200);

            this.stopWatch();
            for (int i = 0; i < 100; i++)
            {
                Vertex v = graph.addVertex(null);
                Vertex u = graph.addVertex(null);
                graph.addEdge(null, v, u, convertId(graph, "test"));
            }
            vertexCount(graph, 600);
            edgeCount(graph, 300);

            graph.rollback();
            printPerformance(graph.ToString(), 100, "edges not added in 1 failed transactions (2 vertices added for each edge)", this.stopWatch());
            vertexCount(graph, 400);
            edgeCount(graph, 200);


            graph.shutdown();
        }

        [Test]
        public void testPropertyTransactions()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsElementProperties())
            {
                this.stopWatch();
                Vertex v = graph.addVertex(null);
                Object id = v.getId();
                v.setProperty("name", "marko");
                graph.commit();
                printPerformance(graph.ToString(), 1, "vertex added with string property in a successful transaction", this.stopWatch());


                this.stopWatch();
                v = graph.getVertex(id);
                Assert.NotNull(v);
                Assert.AreEqual(v.getProperty("name"), "marko");
                v.setProperty("age", 30);
                Assert.AreEqual(v.getProperty("age"), 30);
                graph.rollback();
                printPerformance(graph.ToString(), 1, "integer property not added in a failed transaction", this.stopWatch());

                this.stopWatch();
                v = graph.getVertex(id);
                Assert.NotNull(v);
                Assert.AreEqual(v.getProperty("name"), "marko");
                Assert.Null(v.getProperty("age"));
                printPerformance(graph.ToString(), 2, "vertex properties checked in a successful transaction", this.stopWatch());

                Edge edge = graph.addEdge(null, v, graph.addVertex(null), "test");
                edgeCount(graph, 1);
                graph.commit();
                edgeCount(graph, 1);
                edge = getOnlyElement(graph.getVertex(v.getId()).getEdges(Direction.OUT));
                Assert.NotNull(edge);

                this.stopWatch();
                edge.setProperty("transaction-1", "success");
                Assert.AreEqual(edge.getProperty("transaction-1"), "success");
                graph.commit();
                printPerformance(graph.ToString(), 1, "edge property added and checked in a successful transaction", this.stopWatch());
                edge = getOnlyElement(graph.getVertex(v.getId()).getEdges(Direction.OUT));
                Assert.AreEqual(edge.getProperty("transaction-1"), "success");

                this.stopWatch();
                edge.setProperty("transaction-2", "failure");
                Assert.AreEqual(edge.getProperty("transaction-1"), "success");
                Assert.AreEqual(edge.getProperty("transaction-2"), "failure");
                graph.rollback();
                printPerformance(graph.ToString(), 1, "edge property added and checked in a failed transaction", this.stopWatch());
                edge = getOnlyElement(graph.getVertex(v.getId()).getEdges(Direction.OUT));
                Assert.AreEqual(edge.getProperty("transaction-1"), "success");
                Assert.Null(edge.getProperty("transaction-2"));
            }
            graph.shutdown();
        }

        [Test]
        public void testIndexTransactions()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value)
            {
                this.stopWatch();
                Index index = ((IndexableGraph)graph).createIndex("txIdx", typeof(Vertex));
                Vertex v = graph.addVertex(null);
                Object id = v.getId();
                v.setProperty("name", "marko");
                index.put("name", "marko", v);
                vertexCount(graph, 1);
                v = (Vertex)getOnlyElement(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "marko"));
                Assert.AreEqual(v.getId(), id);
                Assert.AreEqual(v.getProperty("name"), "marko");
                graph.commit();
                printPerformance(graph.ToString(), 1, "vertex added and retrieved from index in a successful transaction", this.stopWatch());


                this.stopWatch();
                vertexCount(graph, 1);
                v = (Vertex)getOnlyElement(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "marko"));
                Assert.AreEqual(v.getId(), id);
                Assert.AreEqual(v.getProperty("name"), "marko");
                printPerformance(graph.ToString(), 1, "vertex retrieved from index outside successful transaction", this.stopWatch());


                this.stopWatch();
                v = graph.addVertex(null);
                v.setProperty("name", "pavel");
                index.put("name", "pavel", v);
                vertexCount(graph, 2);
                v = (Vertex)getOnlyElement(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "marko"));
                Assert.AreEqual(v.getProperty("name"), "marko");
                v = (Vertex)getOnlyElement(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "pavel"));
                Assert.AreEqual(v.getProperty("name"), "pavel");
                graph.rollback();
                printPerformance(graph.ToString(), 1, "vertex not added in a failed transaction", this.stopWatch());

                this.stopWatch();
                vertexCount(graph, 1);
                Assert.AreEqual(count(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "pavel")), 0);
                printPerformance(graph.ToString(), 1, "vertex not retrieved in a successful transaction", this.stopWatch());
                v = (Vertex)getOnlyElement(((IndexableGraph)graph).getIndex("txIdx", typeof(Vertex)).get("name", "marko"));
                Assert.AreEqual(v.getProperty("name"), "marko");
            }
            graph.shutdown();
        }

        [Test]
        public void testAutomaticSuccessfulTransactionOnShutdown()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            if (graph.getFeatures().isPersistent.Value && graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v = graph.addVertex(null);
                Object id = v.getId();
                v.setProperty("count", "1");
                v.setProperty("count", "2");
                graph.shutdown();
                graph = (TransactionalGraph)graphTest.generateGraph();
                Vertex reloadedV = graph.getVertex(id);
                Assert.AreEqual("2", reloadedV.getProperty("count"));

            }
            graph.shutdown();
        }

        [Test]
        public void testVertexCountOnPreTransactionCommit()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            Vertex v1 = graph.addVertex(null);
            graph.commit();

            vertexCount(graph, 1);

            Vertex v2 = graph.addVertex(null);
            v1 = graph.getVertex(v1.getId());
            graph.addEdge(null, v1, v2, convertId(graph, "friend"));

            vertexCount(graph, 2);

            graph.commit();

            vertexCount(graph, 2);
            graph.shutdown();
        }

        [Test]
        public void testBulkTransactionsOnEdges()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            for (int i = 0; i < 5; i++)
            {
                graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "test"));
            }
            edgeCount(graph, 5);
            graph.rollback();
            edgeCount(graph, 0);

            for (int i = 0; i < 4; i++)
            {
                graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "test"));
            }
            edgeCount(graph, 4);
            graph.rollback();
            edgeCount(graph, 0);


            for (int i = 0; i < 3; i++)
            {
                graph.addEdge(null, graph.addVertex(null), graph.addVertex(null), convertId(graph, "test"));
            }
            edgeCount(graph, 3);
            graph.commit();
            edgeCount(graph, 3);

            graph.shutdown();
        }

        [Test]
        public void testCompetingThreads()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            int totalThreads = 250;
            long vertices = 0;
            long edges = 0;
            long completedThreads = 0;

            Parallel.For(0, totalThreads, i =>
            {
                Random random = new Random();
                if (random.Next() % 2 == 0)
                {
                    Vertex a = graph.addVertex(null);
                    Vertex b = graph.addVertex(null);
                    Edge e = graph.addEdge(null, a, b, convertId(graph, "friend"));

                    if (graph.getFeatures().supportsElementProperties())
                    {
                        a.setProperty("test", Task.CurrentId);
                        b.setProperty("blah", random.NextDouble());
                        e.setProperty("bloop", random.Next());
                    }
                    Interlocked.Add(ref vertices, 2);
                    Interlocked.Increment(ref edges);
                    graph.commit();
                }
                else
                {
                    Vertex a = graph.addVertex(null);
                    Vertex b = graph.addVertex(null);
                    Edge e = graph.addEdge(null, a, b, convertId(graph, "friend"));
                    if (graph.getFeatures().supportsElementProperties())
                    {
                        a.setProperty("test", Task.CurrentId);
                        b.setProperty("blah", random.NextDouble());
                        e.setProperty("bloop", random.Next());
                    }
                    if (random.Next() % 2 == 0)
                    {
                        graph.commit();
                        Interlocked.Add(ref vertices, 2);
                        Interlocked.Increment(ref edges);
                    }
                    else
                    {
                        graph.rollback();
                    }
                }

                Interlocked.Increment(ref completedThreads);
            });

            Assert.AreEqual(completedThreads, 250);
            edgeCount(graph, (int)edges);
            vertexCount(graph, (int)vertices);
            graph.shutdown();
        }

        [Test]
        public void testCompetingThreadsOnMultipleDbInstances()
        {
            // the idea behind this test is to simulate a rexster environment where two graphs of the same type
            // are being mutated by multiple threads. the test itself surfaced issues with OrientDB in such
            // an environment and remains relevant for any graph that might be exposed through rexster.

            TransactionalGraph graph1 = (TransactionalGraph)graphTest.generateGraph("first");
            TransactionalGraph graph2 = (TransactionalGraph)graphTest.generateGraph("second");

            if (!graph1.getFeatures().isRDFModel.Value)
            {
                Task threadModFirstGraph = Task.Factory.StartNew(() =>
                {
                    Vertex v = graph1.addVertex(null);
                    v.setProperty("name", "stephen");
                    graph1.commit();
                });

                Task threadReadBothGraphs = Task.Factory.StartNew(() =>
                {
                    int counter = 0;
                    foreach (Vertex v in graph1.getVertices())
                    {
                        counter++;
                    }

                    Assert.AreEqual(1, counter);

                    counter = 0;
                    foreach (Vertex v in graph2.getVertices())
                    {
                        counter++;
                    }

                    Assert.AreEqual(0, counter);
                });

                Task.WaitAll(threadModFirstGraph, threadReadBothGraphs);
            }

            graph1.shutdown();
            graph2.shutdown();
        }

        [Test]
        public void untestTransactionIsolationWithSeparateThreads()
        {
            // the purpose of this test is to simulate rexster access to a graph instance, where one thread modifies
            // the graph and a separate thread reads before the transaction is committed. the expectation is that
            // the changes in the transaction are isolated to the thread that made the change and the second thread
            // should not see the change until commit() in the first thread.
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();

            if (!graph.getFeatures().isRDFModel.Value)
            {
                CountdownEvent latchCommit = new CountdownEvent(1);
                CountdownEvent latchFirstRead = new CountdownEvent(1);
                CountdownEvent latchSecondRead = new CountdownEvent(1);

                Task threadMod = Task.Factory.StartNew(() =>
                {
                    Vertex v = graph.addVertex(null);
                    v.setProperty("name", "stephen");

                    Console.WriteLine("added vertex");

                    latchFirstRead.AddCount();
                    latchCommit.Wait();
                    graph.commit();

                    Console.WriteLine("committed vertex");

                    latchSecondRead.AddCount();
                });

                Task threadRead = Task.Factory.StartNew(() =>
                {

                    latchFirstRead.Wait();

                    Console.WriteLine("reading vertex before tx");
                    Assert.False(graph.getVertices().Any());
                    Console.WriteLine("read vertex before tx");

                    latchCommit.AddCount();
                    latchSecondRead.Wait();

                    Console.WriteLine("reading vertex after tx");
                    Assert.True(graph.getVertices().Any());
                    Console.WriteLine("read vertex after tx");
                });

                Task.WaitAll(threadMod, threadRead);
            }

            graph.shutdown();
        }

        [Test]
        public void testTransactionIsolationCommitCheck()
        {
            // the purpose of this test is to simulate rexster access to a graph instance, where one thread modifies
            // the graph and a separate thread cannot affect the transaction of the first
            TransactionalGraph graph = (TransactionalGraph) graphTest.generateGraph();

            if (!graph.getFeatures().isRDFModel.Value) {
                CountdownEvent latchCommittedInOtherThread = new CountdownEvent(1);
                CountdownEvent latchCommitInOtherThread = new CountdownEvent(1);

                // this thread starts a transaction then waits while the second thread tries to commit it.
                Task threadTxStarter = Task.Factory.StartNew(() => {
                        Vertex v = graph.addVertex(null);
                        v.setProperty("name", "stephen");

                        Console.WriteLine("added vertex");

                        latchCommitInOtherThread.AddCount();
                        latchCommittedInOtherThread.Wait();
                        graph.rollback();

                        // there should be no vertices here
                        Console.WriteLine("reading vertex before tx");
                        Assert.False(graph.getVertices().Any());
                        Console.WriteLine("read vertex before tx");
                    });

                // this thread tries to commit the transaction started in the first thread above.
                Task threadTryCommitTx = Task.Factory.StartNew(() =>
                {
                    latchCommitInOtherThread.Wait();

                    // try to commit the other transaction
                    graph.commit();

                    latchCommittedInOtherThread.AddCount();
                });

                Task.WaitAll(threadTxStarter, threadTryCommitTx);
            }

            graph.shutdown();
        }

        [Test]
        public void testRemoveInTransaction()
        {
            TransactionalGraph graph = (TransactionalGraph)graphTest.generateGraph();
            edgeCount(graph, 0);

            Vertex v1 = graph.addVertex(null);
            Object v1id = v1.getId();
            Vertex v2 = graph.addVertex(null);
            Edge e1 = graph.addEdge(null, v1, v2, convertId(graph, "test-edge"));
            graph.commit();

            edgeCount(graph, 1);
            e1 = getOnlyElement(graph.getVertex(v1id).getEdges(Direction.OUT));
            Assert.NotNull(e1);
            graph.removeEdge(e1);
            edgeCount(graph, 0);
            Assert.Null(getOnlyElement(graph.getVertex(v1id).getEdges(Direction.OUT)));
            graph.rollback();

            edgeCount(graph, 1);
            e1 = getOnlyElement(graph.getVertex(v1id).getEdges(Direction.OUT));
            Assert.NotNull(e1);

            graph.removeEdge(e1);
            graph.commit();

            edgeCount(graph, 0);
            Assert.Null(getOnlyElement(graph.getVertex(v1id).getEdges(Direction.OUT)));
            graph.shutdown();
        }
    }
}
