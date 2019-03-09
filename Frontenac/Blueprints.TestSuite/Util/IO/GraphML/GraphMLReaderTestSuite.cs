using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    public abstract class GraphMlReaderTestSuite : TestSuite
    {
        protected GraphMlReaderTestSuite(GraphTest graphTest)
            : base("GraphMLReaderTestSuite", graphTest)
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

                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-1.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());

                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.In)), 0);
                var marko = graph.GetVertex("1");
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                var counter = 0;
                foreach (var e in graph.GetVertex("1").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.In)), 1);
                var josh = graph.GetVertex("4");
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (var e in graph.GetVertex("4").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
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
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-1.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
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
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Count, 1);
                Assert.AreEqual(edgeValues.Count, 4);
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
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-1.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
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
                Assert.AreEqual(vertexNames.Count, 6);
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
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-1.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
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
                Assert.AreEqual(softwareVertices.Count, 2);
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
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-1.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
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
                Assert.AreEqual(vertices.Count, 3);
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
                Assert.AreEqual(vertices.Count, 1);
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
                Assert.AreEqual(vertices.Count, 2);
// ReSharper disable AssignNullToNotNullAttribute
                Assert.True(vertices.Contains(lop));
                Assert.True(vertices.Contains(ripple));
// ReSharper restore AssignNullToNotNullAttribute
                vertices = new HashSet<IVertex>();
                foreach (var e in josh.GetEdges(Direction.In))
                {
                    vertices.Add(e.GetVertex(Direction.Out));
                }
                Assert.AreEqual(vertices.Count, 1);
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
                Assert.AreEqual(vertices.Count, 1);
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
                Assert.AreEqual(vertices.Count, 3);
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
                Assert.AreEqual(vertices.Count, 1);
                Assert.True(vertices.Contains(josh));
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestReadingTinkerGraphExample3()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IgnoresSuppliedIds || !graph.Features.SupportsEdgeIteration ||
                    !graph.Features.SupportsVertexIteration)
                    return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-3.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.In)), 0);
                var marko = graph.GetVertex("1");
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                Assert.AreEqual(marko.GetProperty("_id"), 2);
                var counter = 0;
                foreach (var e in graph.GetVertex("1").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetProperty("_id"), 8);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("_id"), 10);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("_id"), 9);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id.ToString(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex("2").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex("2").GetEdges(Direction.In)), 1);
                var vadas = graph.GetVertex("2");
                Assert.AreEqual(vadas.GetProperty("name"), "vadas");
                Assert.AreEqual(vadas.GetProperty("age"), 27);
                Assert.AreEqual(vadas.GetProperty("_id"), 3);

                Assert.AreEqual(Count(graph.GetVertex("3").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex("3").GetEdges(Direction.In)), 3);
                var lop = graph.GetVertex("3");
                Assert.AreEqual(lop.GetProperty("name"), "lop");
                Assert.AreEqual(lop.GetProperty("lang"), "java");
                Assert.AreEqual(lop.GetProperty("_id"), 4);

                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.In)), 1);
                var josh = graph.GetVertex("4");
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (var e in graph.GetVertex("4").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("_id"), 13);
                        Assert.AreEqual(e.GetProperty("_label"), null);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.ToString().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("_id"), 11);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id.ToString(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex("5").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex("5").GetEdges(Direction.In)), 1);
                var ripple = graph.GetVertex("5");
                Assert.AreEqual(ripple.GetProperty("name"), "ripple");
                Assert.AreEqual(ripple.GetProperty("lang"), "java");
                Assert.AreEqual(ripple.GetProperty("_id"), 7);

                Assert.AreEqual(Count(graph.GetVertex("6").GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(graph.GetVertex("6").GetEdges(Direction.In)), 0);
                var peter = graph.GetVertex("6");
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);

                foreach (
                    var e in
                        graph.GetVertex("6")
                             .GetEdges(Direction.Out)
                             .Where(e => e.GetVertex(Direction.In).Id.ToString().Equals("3")))
                {
                    Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                    Assert.AreEqual(e.GetProperty("_id"), null);
                    Assert.AreEqual(e.GetProperty("_label"), null);
                    Assert.AreEqual(e.Label, "created");
                    Assert.AreEqual(e.Id.ToString(), "12");
                    counter++;
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                var vertexCount = 0;
                foreach (var v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.Id.ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                        vertexKeys.Add(key);
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeCount = 0;
                foreach (var e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.Id.ToString());
                    foreach (var key in e.GetPropertyKeys())
                        edgeKeys.Add(key);
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count, 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("_id"), true);
                Assert.AreEqual(vertexKeys.Count, 4);

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("_id"), true);
                Assert.AreEqual(edgeKeys.Contains("_label"), true);
                Assert.AreEqual(edgeKeys.Count, 3);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingLabels()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIteration || !graph.Features.SupportsVertexIteration) return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-3.xml"))
                {
                    var r = new GraphMlReader(graph) {EdgeLabelKey = "_label"};
                    r.InputGraph(stream, 1000);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                var vertexCount = 0;
                foreach (var v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.Id.ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (var key in v.GetPropertyKeys())
                        vertexKeys.Add(key);
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                var edgeCount = 0;
                foreach (var e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.Id.ToString());
                    edgeLabels.Add(e.Label);
                    foreach (var key in e.GetPropertyKeys())
                        edgeKeys.Add(key);
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count, 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("_id"), true);
                Assert.AreEqual(vertexKeys.Count, 4);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("_id"), true);
                Assert.AreEqual(edgeKeys.Contains("_label"), false);
                Assert.AreEqual(edgeKeys.Count, 2);
                Assert.AreEqual(edgeLabels.Count, 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), true);
                Assert.AreEqual(edgeLabels.Contains("knows"), false);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingIDs()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIteration || !graph.Features.SupportsVertexIteration) return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-3.xml"))
                {
                    var r = new GraphMlReader(graph) {VertexIdKey = "_id", EdgeIdKey = "_id"};
                    r.InputGraph(stream, 1000);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                var vertexCount = 0;
                foreach (var v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.Id.ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                        vertexKeys.Add(key);
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                var edgeCount = 0;
                foreach (var e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.Id.ToString());
                    edgeLabels.Add(e.Label);
                    foreach (var key in e.GetPropertyKeys())
                        edgeKeys.Add(key);
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count, 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("_id"), false);
                Assert.AreEqual(vertexKeys.Count, 3);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("_id"), false);
                Assert.AreEqual(edgeKeys.Contains("_label"), true);
                Assert.AreEqual(edgeKeys.Count, 2);
                Assert.AreEqual(edgeLabels.Count, 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), false);
                Assert.AreEqual(edgeLabels.Contains("knows"), true);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingAll()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeIteration || !graph.Features.SupportsVertexIteration) return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-3.xml"))
                {
                    var r = new GraphMlReader(graph)
                        {
                            VertexIdKey = "_id",
                            EdgeIdKey = "_id",
                            EdgeLabelKey = "_label"
                        };
                    r.InputGraph(stream, 1000);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                var vertexCount = 0;
                foreach (var v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.Id.ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (var key in v.GetPropertyKeys())
                        vertexKeys.Add(key);
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                var edgeCount = 0;
                foreach (var e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.Id.ToString());
                    edgeLabels.Add(e.Label);
                    foreach (var key in e.GetPropertyKeys())
                        edgeKeys.Add(key);
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count, 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("_id"), false);
                Assert.AreEqual(vertexKeys.Count, 3);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("_id"), false);
                Assert.AreEqual(edgeKeys.Contains("_label"), false);
                Assert.AreEqual(edgeKeys.Count, 1);
                Assert.AreEqual(edgeLabels.Count, 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), true);
                Assert.AreEqual(edgeLabels.Contains("knows"), false);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestMigratingTinkerGraphExample3()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IgnoresSuppliedIds || !graph.Features.SupportsEdgeIteration ||
                    !graph.Features.SupportsVertexIteration)
                    return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-3.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream, 1000);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                StopWatch();
                // FIXME Should not explicitly define the Graph type (TinkerGraph)
                // here. Need to accept 2 graphs as input params?
                var toGraph = new TinkerGraph();
                GraphMigrator.MigrateGraph(graph, toGraph);
                PrintPerformance(toGraph.ToString(), null, "graph-example-3 migrated", StopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(Count(toGraph.GetVertex("1").GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(toGraph.GetVertex("1").GetEdges(Direction.In)), 0);
                var marko = toGraph.GetVertex("1");
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                Assert.AreEqual(marko.GetProperty("_id"), 2);
                var counter = 0;
                foreach (var e in toGraph.GetVertex("1").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetProperty("_id"), 8);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id, "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("_id"), 10);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id, "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("_id"), 9);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "knows");
                        Assert.AreEqual(e.Id, "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(toGraph.GetVertex("2").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex("2").GetEdges(Direction.In)), 1);
                var vadas = toGraph.GetVertex("2");
                Assert.AreEqual(vadas.GetProperty("name"), "vadas");
                Assert.AreEqual(vadas.GetProperty("age"), 27);
                Assert.AreEqual(vadas.GetProperty("_id"), 3);

                Assert.AreEqual(Count(toGraph.GetVertex("3").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex("3").GetEdges(Direction.In)), 3);
                var lop = toGraph.GetVertex("3");
                Assert.AreEqual(lop.GetProperty("name"), "lop");
                Assert.AreEqual(lop.GetProperty("lang"), "java");
                Assert.AreEqual(lop.GetProperty("_id"), 4);

                Assert.AreEqual(Count(toGraph.GetVertex("4").GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(toGraph.GetVertex("4").GetEdges(Direction.In)), 1);
                var josh = toGraph.GetVertex("4");
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (var e in toGraph.GetVertex("4").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).Id.Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("_id"), 13);
                        Assert.AreEqual(e.GetProperty("_label"), null);
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id, "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).Id.Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("_id"), 11);
                        Assert.AreEqual(e.GetProperty("_label"), "has high fived");
                        Assert.AreEqual(e.Label, "created");
                        Assert.AreEqual(e.Id, "10");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(toGraph.GetVertex("5").GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex("5").GetEdges(Direction.In)), 1);
                var ripple = toGraph.GetVertex("5");
                Assert.AreEqual(ripple.GetProperty("name"), "ripple");
                Assert.AreEqual(ripple.GetProperty("lang"), "java");
                Assert.AreEqual(ripple.GetProperty("_id"), 7);

                Assert.AreEqual(Count(toGraph.GetVertex("6").GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(toGraph.GetVertex("6").GetEdges(Direction.In)), 0);
                var peter = toGraph.GetVertex("6");
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);

                foreach (var e in toGraph.GetVertex("6")
                                         .GetEdges(Direction.Out)
                                         .Where(e => e.GetVertex(Direction.In).Id.Equals("3")))
                {
                    Assert.AreEqual(Math.Round(Convert.ToSingle(e.GetProperty("weight"))), 0);
                    Assert.AreEqual(e.GetProperty("_id"), null);
                    Assert.AreEqual(e.GetProperty("_label"), null);
                    Assert.AreEqual(e.Label, "created");
                    Assert.AreEqual(e.Id, "12");
                    counter++;
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexCount = 0;
                foreach (var v in toGraph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.Id.ToString());
                    foreach (string key in v.GetPropertyKeys())
                        vertexKeys.Add(key);
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeCount = 0;
                foreach (var e in toGraph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.Id.ToString());
                    foreach (var key in e.GetPropertyKeys())
                        edgeKeys.Add(key);
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count, 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("_id"), true);
                Assert.AreEqual(vertexKeys.Count, 4);

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count, 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("_id"), true);
                Assert.AreEqual(edgeKeys.Contains("_label"), true);
                Assert.AreEqual(edgeKeys.Count, 3);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestAllGraphMlTypeCastsAndDataMappings()
        {
            // the "key" in the <data> element should map back to the "id" in the "key" element
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.IgnoresSuppliedIds) return;

                StopWatch();
                using (var stream = GetResource<GraphMlReaderTestSuite>("graph-example-4.xml"))
                {
                    new GraphMlReader(graph).InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-4 loaded", StopWatch());

                var onlyOne = graph.GetVertex("1");
                Assert.NotNull(onlyOne);
                Assert.AreEqual(123.45d, onlyOne.GetProperty("d"));
                Assert.AreEqual("some-string", onlyOne.GetProperty("s"));
                Assert.AreEqual(29, onlyOne.GetProperty("i"));
                Assert.AreEqual(true, onlyOne.GetProperty("b"));
                Assert.AreEqual(10000000, onlyOne.GetProperty("l"));
                Assert.AreEqual(123.54f, Convert.ToSingle(onlyOne.GetProperty("f")));
                Assert.AreEqual("junk", onlyOne.GetProperty("n"));
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}