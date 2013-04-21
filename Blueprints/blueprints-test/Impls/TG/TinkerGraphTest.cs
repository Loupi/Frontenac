using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGraphTestSuite : GraphTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphGraphTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphVertexTestSuite : VertexTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphVertexTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphEdgeTestSuite : EdgeTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphEdgeTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphIndexableGraphTestSuite : IndexableGraphTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphIndexableGraphTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphIndexTestSuite : IndexTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphIndexTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGMLReaderTestSuite : GMLReaderTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphGMLReaderTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGraphMLReaderTestSuite : GraphMLReaderTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphGraphMLReaderTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    public class TinkerGraphTestImpl : GraphTest
    {
        public override Graph generateGraph()
        {
            return generateGraph("graph");
        }

        public override Graph generateGraph(string graphDirectoryName)
        {
            return new TinkerGraph(getThinkerGraphDirectory());
        }

        public static string getThinkerGraphDirectory()
        {
            string directory = System.Environment.GetEnvironmentVariable("tinkerGraphDirectory");
            if (directory == null)
                directory = getWorkingDirectory();

            return string.Concat(directory, "/graph");
        }

        static string getWorkingDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphTestGeneral : TinkerGraphTestImpl
    {
        [Test]
        public void testClear()
        {
            deleteDirectory(getThinkerGraphDirectory());
            TinkerGraph graph = (TinkerGraph)this.generateGraph();
            this.stopWatch();
            for (int i = 0; i < 25; i++)
            {
                Vertex a = graph.addVertex(null);
                Vertex b = graph.addVertex(null);
                graph.addEdge(null, a, b, "knows");
            }
            printPerformance(graph.ToString(), 75, "elements added", this.stopWatch());

            Assert.AreEqual(50, count(graph.getVertices()));
            Assert.AreEqual(25, count(graph.getEdges()));

            this.stopWatch();
            graph.clear();
            printPerformance(graph.ToString(), 75, "elements deleted", this.stopWatch());

            Assert.AreEqual(0, count(graph.getVertices()));
            Assert.AreEqual(0, count(graph.getEdges()));

            graph.shutdown();
        }

        [Test]
        public void testShutdownStartManyTimes()
        {
            deleteDirectory(getThinkerGraphDirectory());
            TinkerGraph graph = (TinkerGraph)this.generateGraph();
            for (int i = 0; i < 25; i++)
            {
                Vertex a = graph.addVertex(null);
                a.setProperty("name", string.Concat("a", Guid.NewGuid()));
                Vertex b = graph.addVertex(null);
                b.setProperty("name", string.Concat("b", Guid.NewGuid()));
                graph.addEdge(null, a, b, "knows").setProperty("weight", 1);
            }
            graph.shutdown();
            this.stopWatch();
            int iterations = 150;
            for (int i = 0; i < iterations; i++)
            {
                graph = (TinkerGraph)this.generateGraph();
                Assert.AreEqual(50, count(graph.getVertices()));
                foreach (Vertex v in graph.getVertices())
                {
                    Assert.True(v.getProperty("name").ToString().StartsWith("a") || v.getProperty("name").ToString().StartsWith("b"));
                }
                Assert.AreEqual(25, count(graph.getEdges()));
                foreach (Edge e in graph.getEdges())
                {
                    Assert.AreEqual(e.getProperty("weight"), 1);
                }

                graph.shutdown();
            }
            printPerformance(graph.ToString(), iterations, "iterations of shutdown and restart", this.stopWatch());
        }


        [Test]
        public void testGraphFileTypeJava()
        {
            testGraphFileType("graph-test-java", TinkerGraph.FileType.JAVA);
        }

        [Test]
        public void testGraphFileTypeGML()
        {
            testGraphFileType("graph-test-gml", TinkerGraph.FileType.GML);
        }

        [Test]
        public void testGraphFileTypeGraphML()
        {
            testGraphFileType("graph-test-graphml", TinkerGraph.FileType.GRAPHML);
        }

        [Test]
        public void testGraphFileTypeGraphSON()
        {
            testGraphFileType("graph-test-graphson", TinkerGraph.FileType.GRAPHSON);
        }

        void testGraphFileType(string directory, TinkerGraph.FileType fileType)
        {
            string path = TinkerGraphTestImpl.getThinkerGraphDirectory() + "/" + directory;
            deleteDirectory(path);

            TinkerGraph sourceGraph = TinkerGraphFactory.createTinkerGraph();
            TinkerGraph targetGraph = new TinkerGraph(path, fileType);
            createKeyIndices(targetGraph);

            copyGraphs(sourceGraph, targetGraph);

            createManualIndices(targetGraph);

            this.stopWatch();
            targetGraph.shutdown();
            printTestPerformance("save graph: " + fileType.ToString(), this.stopWatch());

            this.stopWatch();
            TinkerGraph compareGraph = new TinkerGraph(path, fileType);
            printTestPerformance("load graph: " + fileType.ToString(), this.stopWatch());

            compareGraphs(targetGraph, compareGraph, fileType);
        }

        void createKeyIndices(TinkerGraph g)
        {
            g.createKeyIndex("name", typeof(Vertex));
            g.createKeyIndex("weight", typeof(Edge));
        }

        void createManualIndices(TinkerGraph g)
        {
            Index ageIndex = g.createIndex("age", typeof(Vertex));
            Vertex v1 = g.getVertex(1);
            Vertex v2 = g.getVertex(2);
            ageIndex.put("age", v1.getProperty("age"), v1);
            ageIndex.put("age", v2.getProperty("age"), v2);

            Index weightIndex = g.createIndex("weight", typeof(Edge));
            Edge e7 = g.getEdge(7);
            Edge e12 = g.getEdge(12);
            weightIndex.put("weight", e7.getProperty("weight"), e7);
            weightIndex.put("weight", e12.getProperty("weight"), e12);
        }

        void copyGraphs(TinkerGraph src, TinkerGraph dst)
        {
            foreach (Vertex v in src.getVertices())
            {
                ElementHelper.copyProperties(v, dst.addVertex(v.getId()));
            }

            foreach (Edge e in src.getEdges())
            {
                ElementHelper.copyProperties(e,
                        dst.addEdge(e.getId(),
                                    dst.getVertex(e.getVertex(Direction.OUT).getId()),
                                    dst.getVertex(e.getVertex(Direction.IN).getId()),
                                    e.getLabel()));
            }
        }

        void compareGraphs(TinkerGraph g1, TinkerGraph g2, TinkerGraph.FileType fileType)
        {
            foreach (Vertex v1 in g1.getVertices())
            {
                Vertex v2 = g2.getVertex(v1.getId());

                compareEdgeCounts(v1, v2, Direction.IN);
                compareEdgeCounts(v1, v2, Direction.OUT);
                compareEdgeCounts(v1, v2, Direction.BOTH);

                Assert.True(ElementHelper.haveEqualProperties(v1, v2));
                Assert.True(ElementHelper.areEqual(v1, v2));
            }

            foreach (Edge e1 in g1.getEdges())
            {
                Edge e2 = g2.getEdge(e1.getId());

                compareVertices(e1, e2, Direction.IN);
                compareVertices(e2, e2, Direction.OUT);

                if (fileType == TinkerGraph.FileType.GML)
                {
                    // For GML we need to iterate the properties manually to catch the
                    // case where the property returned from GML is an integer
                    // while the target graph property is a float.
                    foreach (String p in e1.getPropertyKeys())
                    {
                        Object v1 = e1.getProperty(p);
                        Object v2 = e2.getProperty(p);

                        if (v1.GetType() != v2.GetType())
                        {
                            if ((v1 is float) && (v2 is int))
                            {
                                Assert.AreEqual(v1, (float)((int)v2));
                            }
                            else if ((v1 is int) && (v2 is float))
                            {
                                Assert.AreEqual((float)((int)v1), v2);
                            }
                        }
                        else
                        {
                            Assert.AreEqual(v1, v2);
                        }
                    }
                }
                else
                {
                    Assert.True(ElementHelper.haveEqualProperties(e1, e2));
                }

                Assert.True(ElementHelper.areEqual(e1, e2));
            }

            Index idxAge = g2.getIndex("age", typeof(Vertex));
            Assert.AreEqual(g2.getVertex(1), idxAge.get("age", 29).First());
            Assert.AreEqual(g2.getVertex(2), idxAge.get("age", 27).First());

            Index idxWeight = g2.getIndex("weight", typeof(Edge));
            Assert.AreEqual(g2.getEdge(7), idxWeight.get("weight", 0.5f).First());
            Assert.AreEqual(g2.getEdge(12), idxWeight.get("weight", 0.2f).First());

            IEnumerator<Vertex> namesItty = g2.getVertices("name", "marko").GetEnumerator();
            namesItty.MoveNext();
            Assert.AreEqual(g2.getVertex(1), namesItty.Current);
            Assert.False(namesItty.MoveNext());

            IEnumerator<Edge> weightItty = g2.getEdges("weight", 0.5f).GetEnumerator();
            weightItty.MoveNext();
            Assert.AreEqual(g2.getEdge(7), weightItty.Current);
            Assert.False(weightItty.MoveNext());
        }

        void compareEdgeCounts(Vertex v1, Vertex v2, Direction direction)
        {
            int c1 = v1.getEdges(direction).Count();
            int c2 = v2.getEdges(direction).Count();

            Assert.AreEqual(c1, c2);
        }

        void compareVertices(Edge e1, Edge e2, Direction direction)
        {
            Vertex v1 = e1.getVertex(direction);
            Vertex v2 = e2.getVertex(direction);

            Assert.AreEqual(v1.getId(), v2.getId());
        }
    }
}
