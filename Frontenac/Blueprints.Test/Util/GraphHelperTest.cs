using System;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "GraphHelperTest")]
    public class GraphHelperTest : BaseTest
    {
        [Test]
        public void TestAddEdge()
        {
            var graph = new TinkerGraph();
            var edge = graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows", "weight", 10.0);
            Assert.AreEqual(edge.GetProperty("weight"), 10.0);
            Assert.AreEqual(edge.Label, "knows");
            Assert.AreEqual(edge.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(Count(graph.GetVertices()), 2);
            Assert.AreEqual(Count(graph.GetEdges()), 1);

            try
            {
                graph.AddEdge(null, graph.AddVertex(null), graph.AddVertex(null), "knows", "weight");
                Assert.True(false);
            }
            catch (Exception)
            {
                Assert.False(false);
                Assert.AreEqual(Count(graph.GetVertices()), 4);
                Assert.AreEqual(Count(graph.GetEdges()), 1);
            }
        }

        [Test]
        public void TestAddVertex()
        {
            var graph = new TinkerGraph();
            var vertex = graph.AddVertex(null, "name", "marko", "age", 31);
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex.GetProperty("age"), 31);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(Count(graph.GetVertices()), 1);

            try
            {
                graph.AddVertex(null, "name", "marko", "age");
                Assert.True(false);
            }
            catch (Exception)
            {
                Assert.False(false);
                Assert.AreEqual(Count(graph.GetVertices()), 1);
            }
        }

        [Test]
        public void TestCopyGraph()
        {
            var g = TinkerGraphFactory.CreateTinkerGraph();
            var h = new TinkerGraph();

            g.CopyGraph(h);
            Assert.AreEqual(Count(h.GetVertices()), 7);
            Assert.AreEqual(Count(h.GetEdges()), 6);
            Assert.AreEqual(Count(h.GetVertex("1").GetEdges(Direction.Out)), 3);
            Assert.AreEqual(Count(h.GetVertex("1").GetEdges(Direction.In)), 0);
            var marko = h.GetVertex("1");
            Assert.AreEqual(marko.GetProperty("name"), "marko");
            Assert.AreEqual(marko.GetProperty("age"), 29);
            var counter = 0;
            foreach (var e in h.GetVertex("1").GetEdges(Direction.Out))
            {
                if (e.GetVertex(Direction.In).Id.Equals("2"))
                {
                    Assert.AreEqual(e.GetProperty("weight"), 0.5);
                    Assert.AreEqual(e.Label, "knows");
                    Assert.AreEqual(e.Id, "7");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).Id.Equals("3"))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                    Assert.AreEqual(e.Label, "created");
                    Assert.AreEqual(e.Id, "9");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).Id.Equals("4"))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                    Assert.AreEqual(e.Label, "knows");
                    Assert.AreEqual(e.Id, "8");
                    counter++;
                }
            }

            Assert.AreEqual(Count(h.GetVertex("4").GetEdges(Direction.Out)), 2);
            Assert.AreEqual(Count(h.GetVertex("4").GetEdges(Direction.In)), 1);
            var josh = h.GetVertex("4");
            Assert.AreEqual(josh.GetProperty("name"), "josh");
            Assert.AreEqual(josh.GetProperty("age"), 32);
            foreach (var e in h.GetVertex("4").GetEdges(Direction.Out))
            {
                if (e.GetVertex(Direction.In).Id.Equals("3"))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                    Assert.AreEqual(e.Label, "created");
                    Assert.AreEqual(e.Id, "11");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).Id.Equals("5"))
                {
                    Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                    Assert.AreEqual(e.Label, "created");
                    Assert.AreEqual(e.Id, "10");
                    counter++;
                }
            }

            Assert.AreEqual(counter, 5);
        }
    }
}