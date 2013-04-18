using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "EdgeHelperTest")]
    public class EdgeHelperTest : BaseTest
    {
        [Test]
        public void testRelabelEdge()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            EdgeHelper.relabelEdge(graph, graph.getEdge(7), "1234", "use_to_know");
            Assert.AreEqual(count(graph.getVertices()), 7);
            Assert.AreEqual(count(graph.getEdges()), 6);
            int counter = 0;
            int counter2 = 0;
            Edge temp = null;
            foreach (Edge edge in graph.getVertex(1).getEdges(Direction.OUT))
            {
                if (edge.getLabel() == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.getId(), "1234");
                    Assert.AreEqual(edge.getProperty("weight"), 0.5f);
                    temp = edge;
                }

                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 3);

            counter = 0;
            counter2 = 0;
            foreach (Edge edge in graph.getVertex(2).getEdges(Direction.IN))
            {
                if (edge.getLabel() == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.getId(), "1234");
                    Assert.AreEqual(edge.getProperty("weight"), 0.5f);
                    Assert.AreEqual(edge, temp);
                }
                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 1);
        }

        [Test]
        public void testRelabelEdges()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            EdgeHelper.relabelEdges(graph, new Edge[]{graph.getEdge(7)}, "use_to_know");
            Assert.AreEqual(count(graph.getVertices()), 7);
            Assert.AreEqual(count(graph.getEdges()), 6);
            int counter = 0;
            int counter2 = 0;
            Edge temp = null;
            foreach (Edge edge in graph.getVertex(1).getEdges(Direction.OUT))
            {
                if (edge.getLabel() == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.getProperty("weight"), 0.5f);
                    temp = edge;
                }

                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 3);

            counter = 0;
            counter2 = 0;
            foreach (Edge edge in graph.getVertex(2).getEdges(Direction.IN))
            {
                if (edge.getLabel() == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.getProperty("weight"), 0.5f);
                    Assert.AreEqual(edge, temp);
                }
                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 1);
        }
    }
}
