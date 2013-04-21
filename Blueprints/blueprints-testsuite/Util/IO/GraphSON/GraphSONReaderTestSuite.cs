using Frontenac.Blueprints.Impls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    public abstract class GraphSONReaderTestSuite : TestSuite
    {
        protected GraphSONReaderTestSuite(GraphTest graph) :
            base("GraphSONReaderTestSuite", graph)
        {
        }

        [Test]
        public void testReadingTinkerGraph()
        {
            Graph graph = graphTest.generateGraph();
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GraphSONReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSONReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSONReader(graph).inputGraph(stream);
                }
                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());

                // note that TinkerGraph converts ids to string internally, but the various WrapperGraphs
                // might like the original data type of the ID. so...this tests getVertex with the original
                // type (integer) but then compares on getId() are ToString() to deal with scenarios
                // where those ids are dealt with differently per graph implementation.

                Assert.AreEqual(count(graph.getVertex(1).getEdges(Direction.OUT)), 3);
                Assert.AreEqual(count(graph.getVertex(1).getEdges(Direction.IN)), 0);
                Vertex marko = graph.getVertex(1);
                Assert.AreEqual(marko.getProperty("name"), "marko");
                Assert.AreEqual(marko.getProperty("age"), 29);
                int counter = 0;
                foreach (Edge e in graph.getVertex(1).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().ToString().Equals("2"))
                    {
                        Assert.AreEqual(e.getProperty("weight"), 0.5);
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId().ToString(), "7");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.getProperty("weight"))), 0);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId().ToString(), "9");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().ToString().Equals("4"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.getProperty("weight"))), 1);
                        Assert.AreEqual(e.getLabel(), "knows");
                        Assert.AreEqual(e.getId().ToString(), "8");
                        counter++;
                    }
                }

                Assert.AreEqual(count(graph.getVertex(4).getEdges(Direction.OUT)), 2);
                Assert.AreEqual(count(graph.getVertex(4).getEdges(Direction.IN)), 1);
                Vertex josh = graph.getVertex(4);
                Assert.AreEqual(josh.getProperty("name"), "josh");
                Assert.AreEqual(josh.getProperty("age"), 32);
                foreach (Edge e in graph.getVertex(4).getEdges(Direction.OUT))
                {
                    if (e.getVertex(Direction.IN).getId().ToString().Equals("3"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.getProperty("weight"))), 0);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId().ToString(), "11");
                        counter++;
                    }
                    else if (e.getVertex(Direction.IN).getId().ToString().Equals("5"))
                    {
                        Assert.AreEqual(Math.Round(Convert.ToDouble(e.getProperty("weight"))), 1);
                        Assert.AreEqual(e.getLabel(), "created");
                        Assert.AreEqual(e.getId().ToString(), "10");
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
            Graph graph = this.graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GraphSONReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSONReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSONReader(graph).inputGraph(stream);
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
            Graph graph = this.graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GraphSONReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSONReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSONReader(graph).inputGraph(stream);
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
            Graph graph = this.graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GraphSONReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSONReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSONReader(graph).inputGraph(stream);
                }
                printPerformance(graph.ToString(), null, "graph-example-1 loaded", this.stopWatch());
                HashSet<Vertex> softwareVertices = new HashSet<Vertex>();
                int count = 0;
                foreach (Vertex v in graph.getVertices())
                {
                    count++;
                    String name = v.getProperty("name").ToString();
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
            Graph graph = this.graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                using (var stream = typeof(GraphSONReaderTestSuite).Assembly.GetManifestResourceStream(typeof(GraphSONReaderTestSuite), "graph-example-1.json"))
                {
                    new GraphSONReader(graph).inputGraph(stream);
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
                    String name = v.getProperty("name").ToString();
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
    }
}
