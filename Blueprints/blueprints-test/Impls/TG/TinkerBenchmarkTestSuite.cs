using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    public abstract class TinkerBenchmarkTestSuite : TestSuite
    {
        const int TOTAL_RUNS = 10;

        protected TinkerBenchmarkTestSuite(GraphTest graphTest)
            : base("TinkerBenchmarkTestSuite", graphTest)
        {

        }

        [Test]
        public void testTinkerGraph()
        {
            double totalTime = 0.0d;
            Graph graph = graphTest.generateGraph();
            using (var stream = typeof(GraphMLReader).Assembly.GetManifestResourceStream(typeof(GraphMLReader), "graph-example-2.xml"))
            {
                GraphMLReader.inputGraph(graph, stream);
            }

            for (int i = 0; i < TOTAL_RUNS; i++)
            {
                this.stopWatch();
                int counter = 0;
                foreach (Vertex vertex in graph.getVertices())
                {
                    counter++;
                    foreach (Edge edge in vertex.getEdges(Direction.OUT))
                    {
                        counter++;
                        Vertex vertex2 = edge.getVertex(Direction.IN);
                        counter++;
                        foreach (Edge edge2 in vertex2.getEdges(Direction.OUT))
                        {
                            counter++;
                            Vertex vertex3 = edge2.getVertex(Direction.IN);
                            counter++;
                            foreach (Edge edge3 in vertex3.getEdges(Direction.OUT))
                            {
                                counter++;
                                edge3.getVertex(Direction.OUT);
                                counter++;
                            }
                        }
                    }
                }
                long currentTime = this.stopWatch();
                totalTime = totalTime + currentTime;
                BaseTest.printPerformance(graph.ToString(), counter, "TinkerGraph elements touched", currentTime);
                graph.shutdown();
            }
            BaseTest.printPerformance("TinkerGraph", 1, "TinkerGraph experiment average", (long)(totalTime / TOTAL_RUNS));
        }
    }
}
