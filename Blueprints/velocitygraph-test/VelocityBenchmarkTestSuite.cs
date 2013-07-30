using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.VG
{
    public abstract class VelocityBenchmarkTestSuite : TestSuite
    {
        const int TotalRuns = 10;

        protected VelocityBenchmarkTestSuite(GraphTest graphTest)
            : base("VelocityBenchmarkTestSuite", graphTest)
        {

        }

        [Test]
        public void TestVelocityGraph()
        {
            double totalTime = 0.0d;
            IGraph graph = GraphTest.GenerateGraph();
            using (var stream = typeof(GraphMlReader).Assembly.GetManifestResourceStream(typeof(GraphMlReader), "graph-example-2.xml"))
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
                PrintPerformance(graph.ToString(), counter, "VelocityGraph elements touched", currentTime);
                graph.Shutdown();
            }
            PrintPerformance("VelocityGraph", 1, "VelocityGraph experiment average", (long)(totalTime / TotalRuns));
        }
    }
}
