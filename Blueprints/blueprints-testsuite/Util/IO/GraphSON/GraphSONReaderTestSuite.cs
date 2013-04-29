using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            IGraph graph = GraphTest.GenerateGraph();
            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                StopWatch();
                using (var stream = typeof(GraphSonReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSonReaderTestSuite), "graph-example-1.json"))
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
                IVertex marko = graph.GetVertex(1);
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                int counter = 0;
                foreach (IEdge e in graph.GetVertex(1).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().ToString().Equals("2"))
                    {
                        Assert.AreEqual(e.GetProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId().ToString(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId().ToString(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().ToString().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId().ToString(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.In)), 1);
                IVertex josh = graph.GetVertex(4);
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (IEdge e in graph.GetVertex(4).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId().ToString(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().ToString().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId().ToString(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 5);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestTinkerGraphEdges()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsEdgeIteration)
            {
                StopWatch();
                using (var stream = typeof(GraphSonReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSonReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeValues = new HashSet<string>();
                int count = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    count++;
                    edgeIds.Add(e.GetId().ToString());
                    foreach (string key in e.GetPropertyKeys())
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
            graph.Shutdown();
        }

        [Test]
        public void TestTinkerGraphVertices()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                using (var stream = typeof(GraphSonReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSonReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var vertexNames = new HashSet<string>();
                int count = 0;
                foreach (IVertex v in graph.GetVertices())
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
            graph.Shutdown();
        }

        [Test]
        public void TestTinkerGraphSoftwareVertices()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                using (var stream = typeof(GraphSonReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSonReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSonReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var softwareVertices = new HashSet<IVertex>();
                int count = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    count++;
                    String name = v.GetProperty("name").ToString();
                    if (name == "lop" || name == "ripple")
                    {
                        softwareVertices.Add(v);
                    }
                }
                Assert.AreEqual(count, 6);
                Assert.AreEqual(softwareVertices.Count(), 2);
                foreach (IVertex v in softwareVertices)
                {
                    Assert.AreEqual(v.GetProperty("lang"), "java");
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestTinkerGraphVertexAndEdges()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                using (var stream = typeof(GraphSonReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSonReaderTestSuite), "graph-example-1.json"))
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
                int c = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    c++;
                    String name = v.GetProperty("name").ToString();
                    if (name == "marko")
                    {
                        marko = v;
                    }
                    else if (name == "peter")
                    {
                        peter = v;
                    }
                    else if (name == "josh")
                    {
                        josh = v;
                    }
                    else if (name == "vadas")
                    {
                        vadas = v;
                    }
                    else if (name == "lop")
                    {
                        lop = v;
                    }
                    else if (name == "ripple")
                    {
                        ripple = v;
                    }
                    else
                    {
                        Assert.True(false);
                    }
                }
                Assert.AreEqual(c, 6);
                Assert.True(null != marko);
                Assert.True(null != peter);
                Assert.True(null != josh);
                Assert.True(null != vadas);
                Assert.True(null != lop);
                Assert.True(null != ripple);

                if (graph.GetFeatures().SupportsEdgeIteration)
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
                foreach (IEdge e in marko.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 3);
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(lop));
// ReSharper restore AssignNullToNotNullAttribute
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(josh));
// ReSharper restore AssignNullToNotNullAttribute
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(vadas));
// ReSharper restore AssignNullToNotNullAttribute
                // test peter
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);
                Assert.AreEqual(peter.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(peter.GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(peter.GetEdges(Direction.In)), 0);
                foreach (IEdge e in peter.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 1);
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(lop));
// ReSharper restore AssignNullToNotNullAttribute
                // test josh
                vertices = new HashSet<IVertex>();
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                Assert.AreEqual(josh.GetPropertyKeys().Count(), 2);
                Assert.AreEqual(Count(josh.GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(josh.GetEdges(Direction.In)), 1);
                foreach (IEdge e in josh.GetEdges(Direction.Out))
                {
                    vertices.Add(e.GetVertex(Direction.In));
                }
                Assert.AreEqual(vertices.Count(), 2);
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(lop));
// ReSharper restore AssignNullToNotNullAttribute
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(ripple));
// ReSharper restore AssignNullToNotNullAttribute
                vertices = new HashSet<IVertex>();
                foreach (IEdge e in josh.GetEdges(Direction.In))
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
                foreach (IEdge e in vadas.GetEdges(Direction.In))
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
                foreach (IEdge e in lop.GetEdges(Direction.In))
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
                foreach (IEdge e in ripple.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(josh));
            }
            graph.Shutdown();
        }
    }
}
