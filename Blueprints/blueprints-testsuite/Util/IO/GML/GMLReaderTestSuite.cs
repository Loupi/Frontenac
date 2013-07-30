using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.IO.GML
{
    public abstract class GmlReaderTestSuite : TestSuite
    {
        protected GmlReaderTestSuite(GraphTest graphTest)
            : base("GMLReaderTestSuite", graphTest)
        {

        }

        [Test]
        public void TestReadingTinkerGraph()
        {
            IGraph graph = GraphTest.GenerateGraph();

            // note that GML does not have the notion of Edge Identifiers built into the specification
            // so that values are not tested here even for graphs that allow edge identifier assignment
            // like tinkergraph.

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                StopWatch();

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-1.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.InputGraph(stream);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());

                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(graph.GetVertex("1").GetEdges(Direction.In)), 0);
                IVertex marko = graph.GetVertex("1");
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                int counter = 0;
                foreach (IEdge e in graph.GetVertex("1").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetLabel(), "knows");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(0, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetLabel(), "created");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("4"))
                    {
                        Assert.AreEqual(1, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetLabel(), "knows");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex("4").GetEdges(Direction.In)), 1);
                IVertex josh = graph.GetVertex("4");
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (IEdge e in graph.GetVertex("4").GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(0, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetLabel(), "created");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("5"))
                    {
                        Assert.AreEqual(1, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetLabel(), "created");
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

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-1.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.InputGraph(stream);
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

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-1.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.InputGraph(stream);
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
                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-1.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.InputGraph(stream);
                }
                PrintPerformance(graph.ToString(), null, "graph-example-1 loaded", StopWatch());
                var softwareVertices = new HashSet<IVertex>();
                int count = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    count++;
                    string name = v.GetProperty("name").ToString();
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
                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-1.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.InputGraph(stream);
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
                    string name = v.GetProperty("name").ToString();
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
                Assert.True(lop != null && vertices.Contains(lop));
                Assert.True(josh != null && vertices.Contains(josh));
                Assert.True(vadas != null && vertices.Contains(vadas));
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
                Assert.True(lop != null && vertices.Contains(lop));
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
                Assert.True(lop != null && vertices.Contains(lop));
                Assert.True(ripple != null && vertices.Contains(ripple));
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

        [Test]
        public void TestReadingTinkerGraphExample3()
        {
            IGraph graph = GraphTest.GenerateGraph();
            
            if (!graph.GetFeatures().IgnoresSuppliedIds && graph.GetFeatures().SupportsEdgeIteration && graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-3.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.SetEdgeIdKey(GmlTokens.Id);
                    gmlReader.InputGraph(stream, 1000);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(Count(graph.GetVertex(1).GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(graph.GetVertex(1).GetEdges(Direction.In)), 0);
                IVertex marko = graph.GetVertex(1);
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                Assert.AreEqual(marko.GetProperty("id2"), 2);
                int counter = 0;
                foreach (IEdge e in graph.GetVertex(1).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetProperty("id2"), 8);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(0, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetProperty("id2"), 10);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("4"))
                    {
                        Assert.AreEqual(1, Math.Round((Convert.ToSingle(e.GetProperty("weight")))));
                        Assert.AreEqual(e.GetProperty("id2"), 9);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex(2).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex(2).GetEdges(Direction.In)), 1);
                IVertex vadas = graph.GetVertex(2);
                Assert.AreEqual(vadas.GetProperty("name"), "vadas");
                Assert.AreEqual(vadas.GetProperty("age"), 27);
                Assert.AreEqual(vadas.GetProperty("id2"), 3);

                Assert.AreEqual(Count(graph.GetVertex(3).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex(3).GetEdges(Direction.In)), 3);
                IVertex lop = graph.GetVertex(3);
                Assert.AreEqual(lop.GetProperty("name"), "lop");
                Assert.AreEqual(lop.GetProperty("lang"), "java");
                Assert.AreEqual(lop.GetProperty("id2"), 4);

                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(graph.GetVertex(4).GetEdges(Direction.In)), 1);
                IVertex josh = graph.GetVertex(4);
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (IEdge e in graph.GetVertex(4).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((Convert.ToSingle(e.GetProperty("weight")))), 0);
                        Assert.AreEqual(e.GetProperty("id2"), 13);
                        Assert.AreEqual(e.GetProperty("label2"), null);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round((Convert.ToSingle(e.GetProperty("weight")))), 1);
                        Assert.AreEqual(e.GetProperty("id2"), 11);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(graph.GetVertex(5).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(graph.GetVertex(5).GetEdges(Direction.In)), 1);
                IVertex ripple = graph.GetVertex(5);
                Assert.AreEqual(ripple.GetProperty("name"), "ripple");
                Assert.AreEqual(ripple.GetProperty("lang"), "java");
                Assert.AreEqual(ripple.GetProperty("id2"), 7);

                Assert.AreEqual(Count(graph.GetVertex(6).GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(graph.GetVertex(6).GetEdges(Direction.In)), 0);
                IVertex peter = graph.GetVertex(6);
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);

                foreach (IEdge e in graph.GetVertex(6).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("id2"), null);
                        Assert.AreEqual(e.GetProperty("label2"), null);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "12");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.GetId().ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                int edgeCount = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.GetId().ToString());
                    foreach (string key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                    }
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count(), 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("id2"), true);
                Assert.AreEqual(vertexKeys.Count(), 4);

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("id2"), true);
                Assert.AreEqual(edgeKeys.Contains("label2"), true);
                Assert.AreEqual(edgeKeys.Count(), 3);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingLabels()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                var r = new GmlReader(graph);
                r.SetEdgeLabelKey("label2");
                r.SetEdgeIdKey("id");

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-3.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.SetEdgeLabelKey("label2");
                    gmlReader.SetEdgeIdKey("id");
                    gmlReader.InputGraph(stream, 1000);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.GetId().ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.GetId().ToString());
                    edgeLabels.Add(e.GetLabel());
                    foreach (string key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                    }
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count(), 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("id2"), true);
                Assert.AreEqual(vertexKeys.Count(), 4);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("id2"), true);
                Assert.AreEqual(edgeKeys.Contains("label2"), false);
                Assert.AreEqual(edgeKeys.Count(), 2);
                Assert.AreEqual(edgeLabels.Count(), 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), true);
                Assert.AreEqual(edgeLabels.Contains("knows"), false);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingIDs()
        {
            IGraph graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();

                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-3.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.SetVertexIdKey("id2");
                    gmlReader.SetEdgeIdKey("id2");
                    gmlReader.InputGraph(stream, 1000);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.GetId().ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.GetId().ToString());
                    edgeLabels.Add(e.GetLabel());
                    foreach (string key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                    }
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count(), 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("id2"), false);
                Assert.AreEqual(vertexKeys.Count(), 3);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("id2"), false);
                Assert.AreEqual(edgeKeys.Contains("label2"), true);
                Assert.AreEqual(edgeKeys.Count(), 2);
                Assert.AreEqual(edgeLabels.Count(), 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), false);
                Assert.AreEqual(edgeLabels.Contains("knows"), true);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestReadingTinkerGraphExample3MappingAll()
        {
            IGraph graph = GraphTest.GenerateGraph();
            
            if (graph.GetFeatures().SupportsEdgeIteration && graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-3.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.SetVertexIdKey("id2");
                    gmlReader.SetEdgeIdKey("id2");
                    gmlReader.SetEdgeLabelKey("label2");
                    gmlReader.InputGraph(stream, 1000);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                var vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (IVertex v in graph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.GetId().ToString());
                    vertexNames.Add(v.GetProperty("name").ToString());
                    foreach (string key in v.GetPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                var edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (IEdge e in graph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.GetId().ToString());
                    edgeLabels.Add(e.GetLabel());
                    foreach (string key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                    }
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count(), 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("id2"), false);
                Assert.AreEqual(vertexKeys.Count(), 3);
                Assert.True(vertexNames.Contains("marko"));
                Assert.True(vertexNames.Contains("josh"));
                Assert.True(vertexNames.Contains("peter"));
                Assert.True(vertexNames.Contains("vadas"));
                Assert.True(vertexNames.Contains("ripple"));
                Assert.True(vertexNames.Contains("lop"));

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("id2"), false);
                Assert.AreEqual(edgeKeys.Contains("label2"), false);
                Assert.AreEqual(edgeKeys.Count(), 1);
                Assert.AreEqual(edgeLabels.Count(), 2);
                Assert.AreEqual(edgeLabels.Contains("has high fived"), true);
                Assert.AreEqual(edgeLabels.Contains("knows"), false);
                Assert.AreEqual(edgeLabels.Contains("created"), true);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestMigratingTinkerGraphExample3()
        {
            IGraph graph = GraphTest.GenerateGraph();
            
            if (!graph.GetFeatures().IgnoresSuppliedIds && graph.GetFeatures().SupportsEdgeIteration && graph.GetFeatures().SupportsVertexIteration)
            {

                StopWatch();
                using (var stream = typeof(GmlReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GmlReaderTestSuite), "graph-example-3.gml"))
                {
                    var gmlReader = new GmlReader(graph);
                    gmlReader.SetEdgeIdKey("id");
                    gmlReader.InputGraph(stream, 1000);
                }

                PrintPerformance(graph.ToString(), null, "graph-example-3 loaded", StopWatch());

                StopWatch();
                // FIXME Should not explicitly define the Graph type (TinkerGraph)
                // here. Need to accept 2 graphs as input params?
                IGraph toGraph = new TinkerGraph();
                GraphMigrator.MigrateGraph(graph, toGraph);
                PrintPerformance(toGraph.ToString(), null, "graph-example-3 migrated", StopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(Count(toGraph.GetVertex(1).GetEdges(Direction.Out)), 3);
                Assert.AreEqual(Count(toGraph.GetVertex(1).GetEdges(Direction.In)), 0);
                IVertex marko = toGraph.GetVertex(1);
                Assert.AreEqual(marko.GetProperty("name"), "marko");
                Assert.AreEqual(marko.GetProperty("age"), 29);
                Assert.AreEqual(marko.GetProperty("id2"), 2);
                int counter = 0;
                foreach (IEdge e in toGraph.GetVertex(1).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.GetProperty("id2"), 8);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId(), "7");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("id2"), 10);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "9");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("id2"), 9);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "knows");
                        Assert.AreEqual(e.GetId(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(toGraph.GetVertex(2).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex(2).GetEdges(Direction.In)), 1);
                IVertex vadas = toGraph.GetVertex(2);
                Assert.AreEqual(vadas.GetProperty("name"), "vadas");
                Assert.AreEqual(vadas.GetProperty("age"), 27);
                Assert.AreEqual(vadas.GetProperty("id2"), 3);

                Assert.AreEqual(Count(toGraph.GetVertex(3).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex(3).GetEdges(Direction.In)), 3);
                IVertex lop = toGraph.GetVertex(3);
                Assert.AreEqual(lop.GetProperty("name"), "lop");
                Assert.AreEqual(lop.GetProperty("lang"), "java");
                Assert.AreEqual(lop.GetProperty("id2"), 4);

                Assert.AreEqual(Count(toGraph.GetVertex(4).GetEdges(Direction.Out)), 2);
                Assert.AreEqual(Count(toGraph.GetVertex(4).GetEdges(Direction.In)), 1);
                IVertex josh = toGraph.GetVertex(4);
                Assert.AreEqual(josh.GetProperty("name"), "josh");
                Assert.AreEqual(josh.GetProperty("age"), 32);
                foreach (IEdge e in toGraph.GetVertex(4).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("id2"), 13);
                        Assert.AreEqual(e.GetProperty("label2"), null);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "11");
                        counter++;
                    }
                    else if (e.GetVertex(Direction.In).GetId().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 1);
                        Assert.AreEqual(e.GetProperty("id2"), 11);
                        Assert.AreEqual(e.GetProperty("label2"), "has high fived");
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(Count(toGraph.GetVertex(5).GetEdges(Direction.Out)), 0);
                Assert.AreEqual(Count(toGraph.GetVertex(5).GetEdges(Direction.In)), 1);
                IVertex ripple = toGraph.GetVertex(5);
                Assert.AreEqual(ripple.GetProperty("name"), "ripple");
                Assert.AreEqual(ripple.GetProperty("lang"), "java");
                Assert.AreEqual(ripple.GetProperty("id2"), 7);

                Assert.AreEqual(Count(toGraph.GetVertex(6).GetEdges(Direction.Out)), 1);
                Assert.AreEqual(Count(toGraph.GetVertex(6).GetEdges(Direction.In)), 0);
                IVertex peter = toGraph.GetVertex(6);
                Assert.AreEqual(peter.GetProperty("name"), "peter");
                Assert.AreEqual(peter.GetProperty("age"), 35);

                foreach (IEdge e in toGraph.GetVertex(6).GetEdges(Direction.Out))
                {
                    if (e.GetVertex(Direction.In).GetId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.GetProperty("weight"))), 0);
                        Assert.AreEqual(e.GetProperty("id2"), null);
                        Assert.AreEqual(e.GetProperty("label2"), null);
                        Assert.AreEqual(e.GetLabel(), "created");
                        Assert.AreEqual(e.GetId(), "12");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                var vertexIds = new HashSet<string>();
                var vertexKeys = new HashSet<string>();
                int vertexCount = 0;
                foreach (IVertex v in toGraph.GetVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.GetId().ToString());
                    foreach (string key in v.GetPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                var edgeIds = new HashSet<string>();
                var edgeKeys = new HashSet<string>();
                int edgeCount = 0;
                foreach (IEdge e in toGraph.GetEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.GetId().ToString());
                    foreach (string key in e.GetPropertyKeys())
                    {
                        edgeKeys.Add(key);
                    }
                }

                Assert.AreEqual(vertexCount, 6);
                Assert.AreEqual(vertexIds.Count(), 6);
                Assert.AreEqual(vertexKeys.Contains("name"), true);
                Assert.AreEqual(vertexKeys.Contains("age"), true);
                Assert.AreEqual(vertexKeys.Contains("lang"), true);
                Assert.AreEqual(vertexKeys.Contains("id2"), true);
                Assert.AreEqual(vertexKeys.Count(), 4);

                Assert.AreEqual(edgeCount, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Contains("weight"), true);
                Assert.AreEqual(edgeKeys.Contains("id2"), true);
                Assert.AreEqual(edgeKeys.Contains("label2"), true);
                Assert.AreEqual(edgeKeys.Count(), 3);
            }
            graph.Shutdown();
        }
    }
}
