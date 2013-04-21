using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GraphML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GML
{
    public abstract class GMLReaderTestSuite : TestSuite
    {
        public GMLReaderTestSuite(GraphTest graphTest)
            : base("GMLReaderTestSuite", graphTest)
        {

        }

        [Test]
        public void testReadingTinkerGraph()
        {
            Graph graph = graphTest.generateGraph();

            // note that GML does not have the notion of Edge Identifiers built into the specification
            // so that values are not tested here even for graphs that allow edge identifier assignment
            // like tinkergraph.
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                this.stopWatch();

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-1.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.inputGraph(stream);
                }

                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());

                Assert.AreEqual(count(graph.getVertex("1").getEdges(Direction.OUT)), 3);
                Assert.AreEqual(count(graph.getVertex("1").getEdges(Direction.IN)), 0);
                Vertex marko = graph.getVertex("1");
                Assert.AreEqual(marko.getProperty("name"), "marko");
                Assert.AreEqual(marko.getProperty("age"), 29);
                int counter = 0;
                foreach (Edge e in graph.getVertex("1").getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.getLabel(), "knows");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(0, Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getLabel(), "created");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("4"))
                    {
                        Assert.AreEqual(1, Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getLabel(), "knows");
                        counter++;
                    }
                }

                Assert.AreEqual(count(graph.getVertex("4").getEdges(Direction.OUT)), 2);
                Assert.AreEqual(count(graph.getVertex("4").getEdges(Direction.IN)), 1);
                Vertex josh = graph.getVertex("4");
                Assert.AreEqual(josh.getProperty("name"), "josh");
                Assert.AreEqual(josh.getProperty("age"), 32);
                foreach (Edge e in graph.getVertex("4").getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(0, (float)Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getLabel(), "created");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("5"))
                    {
                        Assert.AreEqual(1, (float)Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getLabel(), "created");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 5);
            }
            graph.shutdown();
        }

        [Test]
        public void testTinkerGraphEdges()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                this.stopWatch();

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-1.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.inputGraph(stream);
                }

                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                HashSet<string> edgeValues = new HashSet<string>();
                int count = 0;

                foreach (Edge e in graph.getEdges())
                {
                    count++;
                    edgeIds.Add(e.getId().ToString());
                    foreach (string key in e.getPropertyKeys())
                    {
                        edgeKeys.Add(key);
                        edgeValues.Add(e.getProperty(key).ToString());
                    }
                }

                Assert.AreEqual(count, 6);
                Assert.AreEqual(edgeIds.Count(), 6);
                Assert.AreEqual(edgeKeys.Count(), 1);
                Assert.AreEqual(edgeValues.Count(), 4);
            }
            graph.shutdown();
        }

        [Test]
        public void testTinkerGraphVertices()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-1.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.inputGraph(stream);
                }

                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());
                HashSet<string> vertexNames = new HashSet<string>();
                int count = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    count++;
                    vertexNames.Add(v.getProperty("name").ToString());
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
            graph.shutdown();
        }

        [Test]
        public void testTinkerGraphSoftwareVertices()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-1.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.inputGraph(stream);
                }
                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());
                HashSet<Vertex> softwareVertices = new HashSet<Vertex>();
                int count = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    count++;
                    string name = v.getProperty("name").ToString();
                    if (name == "lop" || name == "ripple")
                    {
                        softwareVertices.Add(v);
                    }
                }
                Assert.AreEqual(count, 6);
                Assert.AreEqual(softwareVertices.Count(), 2);
                foreach (Vertex v in softwareVertices)
                {
                    Assert.AreEqual(v.getProperty("lang"), "java");
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testTinkerGraphVertexAndEdges()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-1.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.inputGraph(stream);
                }
                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());
                Vertex marko = null;
                Vertex peter = null;
                Vertex josh = null;
                Vertex vadas = null;
                Vertex lop = null;
                Vertex ripple = null;
                int c = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    c++;
                    string name = v.getProperty("name").ToString();
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

                if (graph.getFeatures().supportsEdgeIteration.Value)
                {
                    Assert.AreEqual(count(graph.getEdges()), 6);
                }

                // test marko
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                Assert.AreEqual(marko.getProperty("name"), "marko");
                Assert.AreEqual(marko.getProperty("age"), 29);
                Assert.AreEqual(marko.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(marko.getEdges(Direction.OUT)), 3);
                Assert.AreEqual(count(marko.getEdges(Direction.IN)), 0);
                foreach (Edge e in marko.getEdges(Direction.OUT))
                {
                    vertices.Add(e.getVertex(Direction.IN));
                }
                Assert.AreEqual(vertices.Count(), 3);
                Assert.True(vertices.Contains(lop));
                Assert.True(vertices.Contains(josh));
                Assert.True(vertices.Contains(vadas));
                // test peter
                vertices = new HashSet<Vertex>();
                Assert.AreEqual(peter.getProperty("name"), "peter");
                Assert.AreEqual(peter.getProperty("age"), 35);
                Assert.AreEqual(peter.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(peter.getEdges(Direction.OUT)), 1);
                Assert.AreEqual(count(peter.getEdges(Direction.IN)), 0);
                foreach (Edge e in peter.getEdges(Direction.OUT))
                {
                    vertices.Add(e.getVertex(Direction.IN));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(lop));
                // test josh
                vertices = new HashSet<Vertex>();
                Assert.AreEqual(josh.getProperty("name"), "josh");
                Assert.AreEqual(josh.getProperty("age"), 32);
                Assert.AreEqual(josh.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(josh.getEdges(Direction.OUT)), 2);
                Assert.AreEqual(count(josh.getEdges(Direction.IN)), 1);
                foreach (Edge e in josh.getEdges(Direction.OUT))
                {
                    vertices.Add(e.getVertex(Direction.IN));
                }
                Assert.AreEqual(vertices.Count(), 2);
                Assert.True(vertices.Contains(lop));
                Assert.True(vertices.Contains(ripple));
                vertices = new HashSet<Vertex>();
                foreach (Edge e in josh.getEdges(Direction.IN))
                {
                    vertices.Add(e.getVertex(Direction.OUT));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(marko));
                // test vadas
                vertices = new HashSet<Vertex>();
                Assert.AreEqual(vadas.getProperty("name"), "vadas");
                Assert.AreEqual(vadas.getProperty("age"), 27);
                Assert.AreEqual(vadas.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(vadas.getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(vadas.getEdges(Direction.IN)), 1);
                foreach (Edge e in vadas.getEdges(Direction.IN))
                {
                    vertices.Add(e.getVertex(Direction.OUT));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(marko));
                // test lop
                vertices = new HashSet<Vertex>();
                Assert.AreEqual(lop.getProperty("name"), "lop");
                Assert.AreEqual(lop.getProperty("lang"), "java");
                Assert.AreEqual(lop.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(lop.getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(lop.getEdges(Direction.IN)), 3);
                foreach (Edge e in lop.getEdges(Direction.IN))
                {
                    vertices.Add(e.getVertex(Direction.OUT));
                }
                Assert.AreEqual(vertices.Count(), 3);
                Assert.True(vertices.Contains(marko));
                Assert.True(vertices.Contains(josh));
                Assert.True(vertices.Contains(peter));
                // test ripple
                vertices = new HashSet<Vertex>();
                Assert.AreEqual(ripple.getProperty("name"), "ripple");
                Assert.AreEqual(ripple.getProperty("lang"), "java");
                Assert.AreEqual(ripple.getPropertyKeys().Count(), 2);
                Assert.AreEqual(count(ripple.getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(ripple.getEdges(Direction.IN)), 1);
                foreach (Edge e in ripple.getEdges(Direction.IN))
                {
                    vertices.Add(e.getVertex(Direction.OUT));
                }
                Assert.AreEqual(vertices.Count(), 1);
                Assert.True(vertices.Contains(josh));
            }
            graph.shutdown();
        }

        [Test]
        public void testReadingTinkerGraphExample3()
        {
            Graph graph = graphTest.generateGraph();
            if (!graph.getFeatures().ignoresSuppliedIds.Value &&
                graph.getFeatures().supportsEdgeIteration.Value &&
                graph.getFeatures().supportsVertexIteration.Value)
            {

                this.stopWatch();

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-3.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.setEdgeIdKey(GMLTokens.ID);
                    gmlReader.inputGraph(stream, 1000);
                }

                printPerformance(graph.ToString(), null, "graph-example-3 loaded", this.stopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(count(graph.getVertex(1).getEdges(Direction.OUT)), 3);
                Assert.AreEqual(count(graph.getVertex(1).getEdges(Direction.IN)), 0);
                Vertex marko = graph.getVertex(1);
                Assert.AreEqual(marko.getProperty("name"), "marko");
                Assert.AreEqual(marko.getProperty("age"), 29);
                Assert.AreEqual(marko.getProperty("id2"), 2);
                int counter = 0;
                foreach (Edge e in graph.getVertex(1).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.getProperty("id2"), 8);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId(), "7");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(0, Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getProperty("id2"), 10);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "9");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("4"))
                    {
                        Assert.AreEqual(1, Math.Round((Convert.ToSingle(e.getProperty("weight")))));
                        Assert.AreEqual(e.getProperty("id2"), 9);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(count(graph.getVertex(2).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(graph.getVertex(2).getEdges(Direction.IN)), 1);
                Vertex vadas = graph.getVertex(2);
                Assert.AreEqual(vadas.getProperty("name"), "vadas");
                Assert.AreEqual(vadas.getProperty("age"), 27);
                Assert.AreEqual(vadas.getProperty("id2"), 3);

                Assert.AreEqual(count(graph.getVertex(3).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(graph.getVertex(3).getEdges(Direction.IN)), 3);
                Vertex lop = graph.getVertex(3);
                Assert.AreEqual(lop.getProperty("name"), "lop");
                Assert.AreEqual(lop.getProperty("lang"), "java");
                Assert.AreEqual(lop.getProperty("id2"), 4);

                Assert.AreEqual(count(graph.getVertex(4).getEdges(Direction.OUT)), 2);
                Assert.AreEqual(count(graph.getVertex(4).getEdges(Direction.IN)), 1);
                Vertex josh = graph.getVertex(4);
                Assert.AreEqual(josh.getProperty("name"), "josh");
                Assert.AreEqual(josh.getProperty("age"), 32);
                foreach (Edge e in graph.getVertex(4).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((Convert.ToSingle(e.getProperty("weight")))), 0);
                        Assert.AreEqual(e.getProperty("id2"), 13);
                        Assert.AreEqual(e.getProperty("label2"), null);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "11");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round((Convert.ToSingle(e.getProperty("weight")))), 1);
                        Assert.AreEqual(e.getProperty("id2"), 11);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(count(graph.getVertex(5).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(graph.getVertex(5).getEdges(Direction.IN)), 1);
                Vertex ripple = graph.getVertex(5);
                Assert.AreEqual(ripple.getProperty("name"), "ripple");
                Assert.AreEqual(ripple.getProperty("lang"), "java");
                Assert.AreEqual(ripple.getProperty("id2"), 7);

                Assert.AreEqual(count(graph.getVertex(6).getEdges(Direction.OUT)), 1);
                Assert.AreEqual(count(graph.getVertex(6).getEdges(Direction.IN)), 0);
                Vertex peter = graph.getVertex(6);
                Assert.AreEqual(peter.getProperty("name"), "peter");
                Assert.AreEqual(peter.getProperty("age"), 35);

                foreach (Edge e in graph.getVertex(6).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 0);
                        Assert.AreEqual(e.getProperty("id2"), null);
                        Assert.AreEqual(e.getProperty("label2"), null);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "12");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                HashSet<string> vertexIds = new HashSet<string>();
                HashSet<string> vertexKeys = new HashSet<string>();
                HashSet<string> vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.getId().ToString());
                    vertexNames.Add(v.getProperty("name").ToString());
                    foreach (string key in v.getPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                int edgeCount = 0;
                foreach (Edge e in graph.getEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.getId().ToString());
                    foreach (string key in e.getPropertyKeys())
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
            graph.shutdown();
        }

        [Test]
        public void testReadingTinkerGraphExample3MappingLabels()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value && graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                GMLReader r = new GMLReader(graph);
                r.setEdgeLabelKey("label2");
                r.setEdgeIdKey("id");

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-3.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.setEdgeLabelKey("label2");
                    gmlReader.setEdgeIdKey("id");
                    gmlReader.inputGraph(stream, 1000);
                }

                printPerformance(graph.ToString(), null, "graph-example-3 loaded", this.stopWatch());

                HashSet<string> vertexIds = new HashSet<string>();
                HashSet<string> vertexKeys = new HashSet<string>();
                HashSet<string> vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.getId().ToString());
                    vertexNames.Add(v.getProperty("name").ToString());
                    foreach (string key in v.getPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                HashSet<string> edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (Edge e in graph.getEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.getId().ToString());
                    edgeLabels.Add(e.getLabel());
                    foreach (string key in e.getPropertyKeys())
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
            graph.shutdown();
        }

        [Test]
        public void testReadingTinkerGraphExample3MappingIDs()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value && graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();

                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-3.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.setVertexIdKey("id2");
                    gmlReader.setEdgeIdKey("id2");
                    gmlReader.inputGraph(stream, 1000);
                }

                printPerformance(graph.ToString(), null, "graph-example-3 loaded", this.stopWatch());

                HashSet<string> vertexIds = new HashSet<string>();
                HashSet<string> vertexKeys = new HashSet<string>();
                HashSet<string> vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.getId().ToString());
                    vertexNames.Add(v.getProperty("name").ToString());
                    foreach (string key in v.getPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                HashSet<string> edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (Edge e in graph.getEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.getId().ToString());
                    edgeLabels.Add(e.getLabel());
                    foreach (string key in e.getPropertyKeys())
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
            graph.shutdown();
        }

        [Test]
        public void testReadingTinkerGraphExample3MappingAll()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value && graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-3.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.setVertexIdKey("id2");
                    gmlReader.setEdgeIdKey("id2");
                    gmlReader.setEdgeLabelKey("label2");
                    gmlReader.inputGraph(stream, 1000);
                }

                printPerformance(graph.ToString(), null, "graph-example-3 loaded", this.stopWatch());

                HashSet<string> vertexIds = new HashSet<string>();
                HashSet<string> vertexKeys = new HashSet<string>();
                HashSet<string> vertexNames = new HashSet<string>();
                int vertexCount = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.getId().ToString());
                    vertexNames.Add(v.getProperty("name").ToString());
                    foreach (string key in v.getPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                HashSet<string> edgeLabels = new HashSet<string>();
                int edgeCount = 0;
                foreach (Edge e in graph.getEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.getId().ToString());
                    edgeLabels.Add(e.getLabel());
                    foreach (string key in e.getPropertyKeys())
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
            graph.shutdown();
        }

        [Test]
        public void testMigratingTinkerGraphExample3()
        {
            Graph graph = graphTest.generateGraph();
            if (!graph.getFeatures().ignoresSuppliedIds.Value &&
                graph.getFeatures().supportsEdgeIteration.Value &&
                graph.getFeatures().supportsVertexIteration.Value)
            {

                this.stopWatch();
                using (var stream = typeof(GMLReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GMLReaderTestSuite), "graph-example-3.gml"))
                {
                    GMLReader gmlReader = new GMLReader(graph);
                    gmlReader.setEdgeIdKey("id");
                    gmlReader.inputGraph(stream, 1000);
                }

                printPerformance(graph.ToString(), null, "graph-example-3 loaded", this.stopWatch());

                this.stopWatch();
                // FIXME Should not explicitly define the Graph type (TinkerGraph)
                // here. Need to accept 2 graphs as input params?
                Graph toGraph = new TinkerGraph();
                GraphMigrator.migrateGraph(graph, toGraph);
                printPerformance(toGraph.ToString(), null, "graph-example-3 migrated", this.stopWatch());

                // Specific Graph Characteristics

                Assert.AreEqual(count(toGraph.getVertex(1).getEdges(Direction.OUT)), 3);
                Assert.AreEqual(count(toGraph.getVertex(1).getEdges(Direction.IN)), 0);
                Vertex marko = toGraph.getVertex(1);
                Assert.AreEqual(marko.getProperty("name"), "marko");
                Assert.AreEqual(marko.getProperty("age"), 29);
                Assert.AreEqual(marko.getProperty("id2"), 2);
                int counter = 0;
                foreach (Edge e in toGraph.getVertex(1).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("2"))
                    {
                        // Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.getProperty("id2"), 8);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId(), "7");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 0);
                        Assert.AreEqual(e.getProperty("id2"), 10);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "9");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 1);
                        Assert.AreEqual(e.getProperty("id2"), 9);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(count(toGraph.getVertex(2).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(toGraph.getVertex(2).getEdges(Direction.IN)), 1);
                Vertex vadas = toGraph.getVertex(2);
                Assert.AreEqual(vadas.getProperty("name"), "vadas");
                Assert.AreEqual(vadas.getProperty("age"), 27);
                Assert.AreEqual(vadas.getProperty("id2"), 3);

                Assert.AreEqual(count(toGraph.getVertex(3).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(toGraph.getVertex(3).getEdges(Direction.IN)), 3);
                Vertex lop = toGraph.getVertex(3);
                Assert.AreEqual(lop.getProperty("name"), "lop");
                Assert.AreEqual(lop.getProperty("lang"), "java");
                Assert.AreEqual(lop.getProperty("id2"), 4);

                Assert.AreEqual(count(toGraph.getVertex(4).getEdges(Direction.OUT)), 2);
                Assert.AreEqual(count(toGraph.getVertex(4).getEdges(Direction.IN)), 1);
                Vertex josh = toGraph.getVertex(4);
                Assert.AreEqual(josh.getProperty("name"), "josh");
                Assert.AreEqual(josh.getProperty("age"), 32);
                foreach (Edge e in toGraph.getVertex(4).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 0);
                        Assert.AreEqual(e.getProperty("id2"), 13);
                        Assert.AreEqual(e.getProperty("label2"), null);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "11");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 1);
                        Assert.AreEqual(e.getProperty("id2"), 11);
                        Assert.AreEqual(e.getProperty("label2"), "has high fived");
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "10");
                        counter++;
                    }
                }

                Assert.AreEqual(count(toGraph.getVertex(5).getEdges(Direction.OUT)), 0);
                Assert.AreEqual(count(toGraph.getVertex(5).getEdges(Direction.IN)), 1);
                Vertex ripple = toGraph.getVertex(5);
                Assert.AreEqual(ripple.getProperty("name"), "ripple");
                Assert.AreEqual(ripple.getProperty("lang"), "java");
                Assert.AreEqual(ripple.getProperty("id2"), 7);

                Assert.AreEqual(count(toGraph.getVertex(6).getEdges(Direction.OUT)), 1);
                Assert.AreEqual(count(toGraph.getVertex(6).getEdges(Direction.IN)), 0);
                Vertex peter = toGraph.getVertex(6);
                Assert.AreEqual(peter.getProperty("name"), "peter");
                Assert.AreEqual(peter.getProperty("age"), 35);

                foreach (Edge e in toGraph.getVertex(6).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round((float)e.getProperty("weight")), 0);
                        Assert.AreEqual(e.getProperty("id2"), null);
                        Assert.AreEqual(e.getProperty("label2"), null);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId(), "12");
                        counter++;
                    }
                }

                Assert.AreEqual(counter, 6);

                // General Graph Characteristics

                HashSet<string> vertexIds = new HashSet<string>();
                HashSet<string> vertexKeys = new HashSet<string>();
                int vertexCount = 0;
                foreach (Vertex v in toGraph.getVertices())
                {
                    vertexCount++;
                    vertexIds.Add(v.getId().ToString());
                    foreach (string key in v.getPropertyKeys())
                    {
                        vertexKeys.Add(key);
                    }
                }

                HashSet<string> edgeIds = new HashSet<string>();
                HashSet<string> edgeKeys = new HashSet<string>();
                int edgeCount = 0;
                foreach (Edge e in toGraph.getEdges())
                {
                    edgeCount++;
                    edgeIds.Add(e.getId().ToString());
                    foreach (string key in e.getPropertyKeys())
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
            graph.shutdown();
        }
    }
}
