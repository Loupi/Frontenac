﻿using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;

namespace Frontenac.Redis.Test
{
    [TestFixture(Category = "TinkerGraphGremlinDocsTestSuite")]
    public abstract class RedisBenchmarkTestSuite : TestSuite
    {
        private const int TotalRuns = 10;

        private readonly RedisGraphTestSuite _suite = new RedisGraphTestSuite();

        [SetUp]
        public void SetUp()
        {
            _suite.SetUp(GraphTest);
        }

        [TearDown]
        public void TearDown()
        {
            _suite.TearDown();
        }

        protected RedisBenchmarkTestSuite()
            : base("RedisBenchmarkTestSuite", new RedisGraphTest())
        {
        }

        protected RedisBenchmarkTestSuite(GraphTest graphTest)
            : base("RedisBenchmarkTestSuite", graphTest)
        {
        }

        [Test]
        public void TestTinkerGraph()
        {
            var totalTime = 0.0d;
            var graph = GraphTest.GenerateGraph();
            try
            {
                using (var stream = GetResource<GraphMlReader>("graph-example-2.xml"))
                {
                    GraphMlReader.InputGraph(graph, stream);
                }

                for (var i = 0; i < TotalRuns; i++)
                {
                    StopWatch();
                    var counter = 0;
                    foreach (var vertex in graph.GetVertices())
                    {
                        counter++;
                        foreach (var edge in vertex.GetEdges(Direction.Out))
                        {
                            counter++;
                            var vertex2 = edge.GetVertex(Direction.In);
                            counter++;
                            foreach (var edge2 in vertex2.GetEdges(Direction.Out))
                            {
                                counter++;
                                var vertex3 = edge2.GetVertex(Direction.In);
                                counter++;
                                foreach (var edge3 in vertex3.GetEdges(Direction.Out))
                                {
                                    counter++;
                                    edge3.GetVertex(Direction.Out);
                                    counter++;
                                }
                            }
                        }
                    }
                    var currentTime = StopWatch();
                    totalTime = totalTime + currentTime;
                    PrintPerformance(graph.ToString(), counter, "TinkerGrapĥ elements touched", currentTime);
                }
            }
            finally
            {
                graph.Shutdown();
            }

            PrintPerformance("TinkerGraph", 1, "TinkerGrapĥ experiment average", (long) (totalTime/TotalRuns));
        }
    }
}