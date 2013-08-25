using NUnit.Framework;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "EdgeHelperTest")]
    public class EdgeHelperTest : BaseTest
    {
        [Test]
        public void TestRelabelEdge()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            EdgeHelper.RelabelEdge(graph, graph.GetEdge(7), "1234", "use_to_know");
            Assert.AreEqual(7, Count(graph.GetVertices()));
            Assert.AreEqual(6, Count(graph.GetEdges()));
            int counter = 0;
            int counter2 = 0;
            IEdge temp = null;
            foreach (IEdge edge in graph.GetVertex(1).GetEdges(Direction.Out))
            {
                if (edge.Label == "use_to_know")
                {
                    counter++;
                    if(!graph.Features.IgnoresSuppliedIds)
                        Assert.AreEqual("1234", edge.Id);
                    Assert.AreEqual(0.5, edge.GetProperty("weight"));
                    temp = edge;
                }

                counter2++;
            }
            Assert.AreEqual(1, counter);
            Assert.AreEqual(3, counter2);

            counter = 0;
            counter2 = 0;
            foreach (IEdge edge in graph.GetVertex(2).GetEdges(Direction.In))
            {
                if (edge.Label == "use_to_know")
                {
                    counter++;
                    if (!graph.Features.IgnoresSuppliedIds)
                        Assert.AreEqual("1234", edge.Id);
                    Assert.AreEqual(0.5, edge.GetProperty("weight"));
                    Assert.AreEqual(edge, temp);
                }
                counter2++;
            }
            Assert.AreEqual(1, counter);
            Assert.AreEqual(1, counter2);
        }

        [Test]
        public void TestRelabelEdges()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            EdgeHelper.RelabelEdges(graph, new[]{graph.GetEdge(7)}, "use_to_know");
            Assert.AreEqual(Count(graph.GetVertices()), 7);
            Assert.AreEqual(Count(graph.GetEdges()), 6);
            int counter = 0;
            int counter2 = 0;
            IEdge temp = null;
            foreach (IEdge edge in graph.GetVertex(1).GetEdges(Direction.Out))
            {
                if (edge.Label == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.GetProperty("weight"), 0.5);
                    temp = edge;
                }

                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 3);

            counter = 0;
            counter2 = 0;
            foreach (IEdge edge in graph.GetVertex(2).GetEdges(Direction.In))
            {
                if (edge.Label == "use_to_know")
                {
                    counter++;
                    Assert.AreEqual(edge.GetProperty("weight"), 0.5);
                    Assert.AreEqual(edge, temp);
                }
                counter2++;
            }
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(counter2, 1);
        }
    }
}
