using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    public abstract class GraphSonReaderTestSuite : TestSuite
    {
        protected GraphSonReaderTestSuite(GraphTest graph) :
            base("GraphSONReaderTestSuite", graph)
        {
        }

        [Test]
        public void TestReadingTinkerGraph()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IgnoresSuppliedIds) return;

                StopWatch();
                using (var stream = GetResource<GraphSonReaderTestSuite>("graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());

                // note that TinkerGraph converts ids to string internally, but the various WrapperGraphs
                // might like the original data type of the ID. so...this tests getVertex with the original
                // type (integer) but then compares on getId() are ToString() to deal with scenarios
                // where those ids are dealt with differently per graph implementation.

                Assert.AreEqual(Count(graph.GetVertex(1).GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(graph.GetVertex(1).GetEdges(Direction.In)), 0);
                var marko = graph.GetVertex(1);
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                var counter = 0;
                foreach (var e in graph.GetVertex(1).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("2"))
                    {
                        Assert.AreEqual(e.GetProperty("weight"), 0.5);
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.In)), 1);
                var josh = graph.GetVertex(4);
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (var e in graph.GetVertex(4).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 5);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestTinkerGraphEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIteration) return;

                StopWatch();

                using (var stream = GetResource<GraphSonReaderTestSuite>("graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeValues = new HashSet<string>();
                var count = 0;
                foreach (var e in graph.GetEdges())
                {
                    count++;
                    edgeIds.Add(e.Id.ToString());
                    foreach (var key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                        edgeValues.Add(e.GetProperty(key).ToString());
                    }
                }
                Assert.AreEqual(count, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Count(), 1);
                Assert.AreEqual(edgeValues.Count(), 4);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestTinkerGraphVertices()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIteration) return;

                StopWatch();

                using (var stream = GetResource<GraphSonReaderTestSuite>("graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var vertexNames = new HashSet<string>();
                var count = 0;
                foreach (var v in graph.GetVertices())
                {
                    count++;
                    vertexNames.Add(v.GetProperty("name").ToString());
                    // System.out.println(v);
                }
                Assert.AreEqual(count, 6);
                Assert.AreEqual(vertexNames.Count(), 6);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestTinkerGraphSoftwareVertices()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIteration) return;

                StopWatch();
                using (var stream = GetResource<GraphSonReaderTestSuite>("graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var softwareVertices = new HashSet<IVertex>();
                var count = 0;
                foreach (var v in graph.GetVertices())
                {
                    count++;
                    var name = v.GetProperty("name").ToString();
                    if (name == "lop" || name == "ripple")
                    {
                        softwareVertices.Add(v);
                    }
                }
                Assert.AreEqual(count, 6);
                Assert.AreEqual(softwareVertices.Count(), 2);
                foreach (var v in softwareVertices)
                {
                    Assert.AreEqual(v.GetProperty("lang"), "java");
                }
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestTinkerGraphVertexAndEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsVertexIteration) return;

                StopWatch();
                using (var stream = GetResource<GraphSonReaderTestSuite>("graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                IVertex marko = null;
                IVertex peter = null;
                IVertex josh = null;
                IVertex vadas = null;
                IVertex lop = null;
                IVertex ripple = null;
                var c = 0;
                foreach (var v in graph.GetVertices())
                {
                    c++;
                    var name = v.GetProperty("name").ToString();
                    switch (name)
                    {
                        case "marko":
                            marko = v;
                            break;
                        case "peter":
                            peter = v;
                            break;
                        case "josh":
                            josh = v;
                            break;
                        case "vadas":
                            vadas = v;
                            break;
                        case "lop":
                            lop = v;
                            break;
                        case "ripple":
                            ripple = v;
                            break;
                        default:
                            Assert.True(false);
                            break;
                    }
                }
                Assert.AreEqual(c, 6);
                Assert.NotNull(marko);
                Assert.NotNull(peter);
                Assert.NotNull(josh);
                Assert.NotNull(vadas);
                Assert.NotNull(lop);
                Assert.NotNull(ripple);

                if (graph.Features.SupportsEdgeIteration)
                {
                    Assert.AreEqual(Count(graph.GetEdges()), 6);
                }

                // test marko
                var vertices = new HashSet<IVertex>();
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                Assert.AreEqual(marko.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(marko.GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(marko.GetEdges(Direction.In)), 0);
                foreach (var e in marko.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 3);
                // ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(lop));
                Assert.True(vertices.Contains(josh));
                Assert.True(vertices.Contains(vadas));

                // test peter
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);
                Assert.AreEqual(peter.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(peter.GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(peter.GetEdges(Direction.In)), 0);
                foreach (var e in peter.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(lop));

                // test josh
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                Assert.AreEqual(josh.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(josh.GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(josh.GetEdges(Direction.In)), 1);
                foreach (var e in josh.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 2);
                Assert.True(vertices.Contains(lop));
                Assert.True(vertices.Contains(ripple));
                // ReSharper restore AssignNullToNotNullAttribute
                vertices = new HashSet<IVertex>();
                foreach (var e in josh.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(marko));

                // test vadas
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(vadas.GetProperty("name"), "vadas");
                Assert.AreEqual(vadas.GetProperty("age"), 27);
                Assert.AreEqual(vadas.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(vadas.GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(vadas.GetEdges(Direction.In)), 1);
                foreach (var e in vadas.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(marko));

                // test lop
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(lop.GetProperty("name"), "lop");
                Assert.AreEqual(lop.GetProperty("lang"), "java");
                Assert.AreEqual(lop.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(lop.GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(lop.GetEdges(Direction.In)), 3);
                foreach (var e in lop.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count(), 3);
                Assert.True(vertices.Contains(marko));
                Assert.True(vertices.Contains(josh));
                Assert.True(vertices.Contains(peter));

                // test ripple
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(ripple.GetProperty("name"), "ripple");
                Assert.AreEqual(ripple.GetProperty("lang"), "java");
                Assert.AreEqual(ripple.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(ripple.GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(ripple.GetEdges(Direction.In)), 1);
                foreach (var e in ripple.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(josh));
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}