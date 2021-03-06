﻿using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.TG
{
    public abstract class TinkerBenchmarkTestSuite : TestSuite
    {
        private const int TotalRuns = 10;

        protected TinkerBenchmarkTestSuite(GraphTest graphTest)
            : base("TinkerBenchmarkTestSuite", graphTest)
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

                for (int i = 0; i < TotalRuns; i++)
                {
                    StopWatch();
                    int counter = 0;
                    foreach (IVertex vertex in graph.GetVertices())
                    {
                        counter++;
                        foreach (IEdge edge in vertex.GetEdges(Direction.Out))
                        {
                            counter++;
                            IVertex vertex2 = edge.GetVertex(Direction.In);
                            counter++;
                            foreach (IEdge edge2 in vertex2.GetEdges(Direction.Out))
                            {
                                counter++;
                                IVertex vertex3 = edge2.GetVertex(Direction.In);
                                counter++;
                                foreach (IEdge edge3 in vertex3.GetEdges(Direction.Out))
                                {
                                    counter++;
                                    edge3.GetVertex(Direction.Out);
                                    counter++;
                                }
                            }
                        }
                    }
                    long currentTime = StopWatch();
                    totalTime = totalTime + currentTime;
                    PrintPerformance(graph.ToString(), counter, "TinkerGrapĥ elements touched", currentTime);
                }
            }
            finally
            {
                graph.Shutdown();
            }

            PrintPerformance("TinkerGrapĥ", 1, "TinkerGrapĥ experiment average", (long) (totalTime/TotalRuns));
        }
    }
}