using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "GraphHelperTest")]
    public class GraphHelperTest : BaseTest
    {
        [Test]
        public void testAddVertex()
        {
            Graph graph = new TinkerGraph();
            Vertex vertex = GraphHelper.addVertex(graph, null, "name", "marko", "age", 31);
            Assert.AreEqual(vertex.getProperty("name"), "marko");
            Assert.AreEqual(vertex.getProperty("age"), 31);
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 2);
            Assert.AreEqual(count(graph.getVertices()), 1);

            try
            {
                vertex = GraphHelper.addVertex(graph, null, "name", "marko", "age");
                Assert.True(false);
            }
            catch (Exception)
            {
                Assert.False(false);
                Assert.AreEqual(count(graph.getVertices()), 1);
            }
        }

        [Test]
        public void testAddEdge()
        {
            Graph graph = new TinkerGraph();
            Edge edge = GraphHelper.addEdge(graph, null, graph.addVertex(null), graph.addVertex(null), "knows", "weight", 10.0f);
            Assert.AreEqual(edge.getProperty("weight"), 10.0f);
            Assert.AreEqual(edge.getLabel(), "knows");
            Assert.AreEqual(edge.getPropertyKeys().Count(), 1);
            Assert.AreEqual(count(graph.getVertices()), 2);
            Assert.AreEqual(count(graph.getEdges()), 1);

            try
            {
                edge = GraphHelper.addEdge(graph, null, graph.addVertex(null), graph.addVertex(null), "knows", "weight");
                Assert.True(false);
            }
            catch (Exception)
            {
                Assert.False(false);
                Assert.AreEqual(count(graph.getVertices()), 4);
                Assert.AreEqual(count(graph.getEdges()), 1);
            }
        }

        [Test]
        public void testCopyGraph()
        {
            Graph g = TinkerGraphFactory.createTinkerGraph();
            Graph h = new TinkerGraph();

            GraphHelper.copyGraph(g, h);
            Assert.AreEqual(count(h.getVertices()), 7);
            Assert.AreEqual(count(h.getEdges()), 6);
            Assert.AreEqual(count(h.getVertex("1").getEdges(Direction.OUT)), 3);
            Assert.AreEqual(count(h.getVertex("1").getEdges(Direction.IN)), 0);
            Vertex marko = h.getVertex("1");
            Assert.AreEqual(marko.getProperty("name"), "marko");
            Assert.AreEqual(marko.getProperty("age"), 29);
            int counter = 0;
            foreach (Edge e in h.getVertex("1").getEdges(Direction.OUT))
            {
                if (e.getVertex(Direction.IN).getId().Equals("2"))
                {
                    Assert.AreEqual(e.getProperty("weight"), 0.5f);
                    Assert.AreEqual(e.getLabel(), "knows");
                    Assert.AreEqual(e.getId(), "7");
                    counter++;
                }
                else if (e.getVertex(Direction.IN).getId().Equals("3"))
                {
                    Assert.AreEqual(Math.Round((float) e.getProperty("weight")), 0);
                    Assert.AreEqual(e.getLabel(), "created");
                    Assert.AreEqual(e.getId(), "9");
                    counter++;
                }
                else if (e.getVertex(Direction.IN).getId().Equals("4"))
                {
                    Assert.AreEqual(Math.Round((float) e.getProperty("weight")), 1);
                    Assert.AreEqual(e.getLabel(), "knows");
                    Assert.AreEqual(e.getId(), "8");
                    counter++;
                }
            }

            Assert.AreEqual(count(h.getVertex("4").getEdges(Direction.OUT)), 2);
            Assert.AreEqual(count(h.getVertex("4").getEdges(Direction.IN)), 1);
            Vertex josh = h.getVertex("4");
            Assert.AreEqual(josh.getProperty("name"), "josh");
            Assert.AreEqual(josh.getProperty("age"), 32);
            foreach (Edge e in h.getVertex("4").getEdges(Direction.OUT))
            {
                if (e.getVertex(Direction.IN).getId().Equals("3"))
                {
                    Assert.AreEqual(Math.Round((float) e.getProperty("weight")), 0);
                    Assert.AreEqual(e.getLabel(), "created");
                    Assert.AreEqual(e.getId(), "11");
                    counter++;
                }
                else if (e.getVertex(Direction.IN).getId().Equals("5"))
                {
                    Assert.AreEqual(Math.Round((float) e.getProperty("weight")), 1);
                    Assert.AreEqual(e.getLabel(), "created");
                    Assert.AreEqual(e.getId(), "10");
                    counter++;
                }
            }

            Assert.AreEqual(counter, 5);
        }
    }
}
