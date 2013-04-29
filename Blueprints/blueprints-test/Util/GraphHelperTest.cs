﻿using NUnit.Framework;
using System;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "GraphHelperTest")]
    public class GraphHelperTest : BaseTest
    {
        [Test]
        public void TestAddVertex()
        {
            IGraph graph = new TinkerGraph();
            IVertex vertex = GraphHelper.AddVertex(graph, null, "name", "marko", "age", 31);
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex.GetProperty("age"), 31);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(Count(graph.GetVertices()), 1);

            try
            {
                GraphHelper.AddVertex(graph, null, "name", "marko", "age");
                Assert.True(false);
            }
            catch (Exception)
            {
                Assert.False(false);
                Assert.AreEqual(Count(graph.GetVertices()), 1);
            }
        }

        [Test]
        public void TestAddEdge()
        {
            IGraph graph = new TinkerGraph();
            IEdge edge = GraphHelper.AddEdge(graph, null, graph.AddVertex(null), graph.AddVertex(null), "knows", "weight", 10.0f);
            Assert.AreEqual(edge.GetProperty("weight"), 10.0f);
            Assert.AreEqual(edge.GetLabel(), "knows");
            Assert.AreEqual(edge.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(Count(graph.GetVertices()), 2);
            Assert.AreEqual(Count(graph.GetEdges()), 1);

            try
            {
                GraphHelper.AddEdge(graph, null, graph.AddVertex(null), graph.AddVertex(null), "knows", "weight");
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
        public void TestCopyGraph()
        {
            IGraph g = TinkerGraphFactory.CreateTinkerGraph();
            IGraph h = new TinkerGraph();

            GraphHelper.CopyGraph(g, h);
            Assert.AreEqual(Count(h.GetVertices()), 7);
            Assert.AreEqual(Count(h.GetEdges()), 6);
            Assert.AreEqual(Count(h.GetVertex("1").GetEdges(Direction.Out)), 3);
            Assert.AreEqual(Count(h.GetVertex("1").GetEdges(Direction.In)), 0);
            IVertex marko = h.GetVertex("1");
            Assert.AreEqual(marko.GetProperty("name"), "marko");
            Assert.AreEqual(marko.GetProperty("age"), 29);
            int counter = 0;
            foreach (IEdge e in h.GetVertex("1").GetEdges(Direction.Out))
            {
                if (e.GetVertex(Direction.In).GetId().Equals("2"))
                {
                    Assert.AreEqual(e.GetProperty("weight"), 0.5f);
                    Assert.AreEqual(e.GetLabel(), "knows");
                    Assert.AreEqual(e.GetId(), "7");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).GetId().Equals("3"))
                {
                    Assert.AreEqual(Math.Round((float) e.GetProperty("weight")), 0);
                    Assert.AreEqual(e.GetLabel(), "created");
                    Assert.AreEqual(e.GetId(), "9");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).GetId().Equals("4"))
                {
                    Assert.AreEqual(Math.Round((float) e.GetProperty("weight")), 1);
                    Assert.AreEqual(e.GetLabel(), "knows");
                    Assert.AreEqual(e.GetId(), "8");
                    counter++;
                }
            }

            Assert.AreEqual(Count(h.GetVertex("4").GetEdges(Direction.Out)), 2);
            Assert.AreEqual(Count(h.GetVertex("4").GetEdges(Direction.In)), 1);
            IVertex josh = h.GetVertex("4");
            Assert.AreEqual(josh.GetProperty("name"), "josh");
            Assert.AreEqual(josh.GetProperty("age"), 32);
            foreach (IEdge e in h.GetVertex("4").GetEdges(Direction.Out))
            {
                if (e.GetVertex(Direction.In).GetId().Equals("3"))
                {
                    Assert.AreEqual(Math.Round((float) e.GetProperty("weight")), 0);
                    Assert.AreEqual(e.GetLabel(), "created");
                    Assert.AreEqual(e.GetId(), "11");
                    counter++;
                }
                else if (e.GetVertex(Direction.In).GetId().Equals("5"))
                {
                    Assert.AreEqual(Math.Round((float) e.GetProperty("weight")), 1);
                    Assert.AreEqual(e.GetLabel(), "created");
                    Assert.AreEqual(e.GetId(), "10");
                    counter++;
                }
            }

            Assert.AreEqual(counter, 5);
        }
    }
}
