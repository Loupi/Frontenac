using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGraphTestSuite : GraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphGraphTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphVertexTestSuite : VertexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphVertexTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphVertexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphEdgeTestSuite : EdgeTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphEdgeTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphEdgeTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphKeyIndexableGraphTestSuite : KeyIndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphKeyIndexableGraphTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphKeyIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphIndexableGraphTestSuite : IndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphIndexableGraphTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphIndexTestSuite : IndexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphIndexTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphIndexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGmlReaderTestSuite : GmlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphGmlReaderTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphGmlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGraphMlReaderTestSuite : GraphMlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphGraphMlReaderTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphGraphMlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphGraphSonReaderTestSuite : GraphSonReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphGraphSonReaderTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphGraphSonReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    public class TinkerGraphTestImpl : GraphTest
    {
        public override IGraph GenerateGraph()
        {
            return GenerateGraph("graph");
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return new TinkerGraph(GetThinkerGraphDirectory());
        }

        public static string GetThinkerGraphDirectory()
        {
            string directory = Environment.GetEnvironmentVariable("tinkerGraphDirectory") ?? GetWorkingDirectory();

            return string.Concat(directory, "/graph");
        }

        static string GetWorkingDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphTestGeneral : TestSuite
    {
        public TinkerGraphTestGeneral()
            : this(new TinkerGraphTestImpl())
        {
        }

        public TinkerGraphTestGeneral(GraphTest graphTest) :
            base("TinkerGraphTestGeneral", graphTest)
        {

        }

        [Test]
        public void TestClear()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
            var graph = (TinkerGraph)GraphTest.GenerateGraph();
            StopWatch();
            for (int i = 0; i < 25; i++)
            {
                IVertex a = graph.AddVertex(null);
                IVertex b = graph.AddVertex(null);
                graph.AddEdge(null, a, b, "knows");
            }
            PrintPerformance(graph.ToString(), 75, "elements added", StopWatch());

            Assert.AreEqual(50, Count(graph.GetVertices()));
            Assert.AreEqual(25, Count(graph.GetEdges()));

            StopWatch();
            graph.Clear();
            PrintPerformance(graph.ToString(), 75, "elements deleted", StopWatch());

            Assert.AreEqual(0, Count(graph.GetVertices()));
            Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.Shutdown();
        }

        [Test]
        public void TestShutdownStartManyTimes()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
            var graph = (TinkerGraph)GraphTest.GenerateGraph();
            for (int i = 0; i < 25; i++)
            {
                IVertex a = graph.AddVertex(null);
                a.SetProperty("name", string.Concat("a", Guid.NewGuid()));
                IVertex b = graph.AddVertex(null);
                b.SetProperty("name", string.Concat("b", Guid.NewGuid()));
                graph.AddEdge(null, a, b, "knows").SetProperty("weight", 1);
            }
            graph.Shutdown();
            StopWatch();
            const int iterations = 150;
            for (int i = 0; i < iterations; i++)
            {
                graph = (TinkerGraph)GraphTest.GenerateGraph();
                Assert.AreEqual(50, Count(graph.GetVertices()));
                foreach (IVertex v in graph.GetVertices())
                {
                    Assert.True(v.GetProperty("name").ToString().StartsWith("a") || v.GetProperty("name").ToString().StartsWith("b"));
                }
                Assert.AreEqual(25, Count(graph.GetEdges()));
                foreach (IEdge e in graph.GetEdges())
                {
                    Assert.AreEqual(e.GetProperty("weight"), 1);
                }

                graph.Shutdown();
            }
            PrintPerformance(graph.ToString(), iterations, "iterations of shutdown and restart", StopWatch());
        }


        [Test]
        public void TestGraphFileTypeJava()
        {
            TestGraphFileType("graph-test-java", TinkerGraph.FileType.Java);
        }

        [Test]
        public void TestGraphFileTypeGml()
        {
            TestGraphFileType("graph-test-gml", TinkerGraph.FileType.Gml);
        }

        [Test]
        public void TestGraphFileTypeGraphMl()
        {
            TestGraphFileType("graph-test-graphml", TinkerGraph.FileType.Graphml);
        }

        [Test]
        public void TestGraphFileTypeGraphSon()
        {
            TestGraphFileType("graph-test-graphson", TinkerGraph.FileType.Graphson);
        }

        void TestGraphFileType(string directory, TinkerGraph.FileType fileType)
        {
            string path = TinkerGraphTestImpl.GetThinkerGraphDirectory() + "/" + directory;
            DeleteDirectory(path);

            TinkerGraph sourceGraph = TinkerGraphFactory.CreateTinkerGraph();
            var targetGraph = new TinkerGraph(path, fileType);
            CreateKeyIndices(targetGraph);

            CopyGraphs(sourceGraph, targetGraph);

            CreateManualIndices(targetGraph);

            StopWatch();
            targetGraph.Shutdown();
            PrintTestPerformance("save graph: " + fileType.ToString(), StopWatch());

            StopWatch();
            var compareGraph = new TinkerGraph(path, fileType);
            PrintTestPerformance("load graph: " + fileType.ToString(), StopWatch());

            CompareGraphs(targetGraph, compareGraph, fileType);
        }

        static void CreateKeyIndices(TinkerGraph g)
        {
            g.CreateKeyIndex("name", typeof(IVertex));
            g.CreateKeyIndex("weight", typeof(IEdge));
        }

        static void CreateManualIndices(TinkerGraph g)
        {
            IIndex ageIndex = g.CreateIndex("age", typeof(IVertex));
            IVertex v1 = g.GetVertex(1);
            IVertex v2 = g.GetVertex(2);
            ageIndex.Put("age", v1.GetProperty("age"), v1);
            ageIndex.Put("age", v2.GetProperty("age"), v2);

            IIndex weightIndex = g.CreateIndex("weight", typeof(IEdge));
            IEdge e7 = g.GetEdge(7);
            IEdge e12 = g.GetEdge(12);
            weightIndex.Put("weight", e7.GetProperty("weight"), e7);
            weightIndex.Put("weight", e12.GetProperty("weight"), e12);
        }

        static void CopyGraphs(TinkerGraph src, TinkerGraph dst)
        {
            foreach (IVertex v in src.GetVertices())
            {
                ElementHelper.CopyProperties(v, dst.AddVertex(v.GetId()));
            }

            foreach (IEdge e in src.GetEdges())
            {
                ElementHelper.CopyProperties(e,
                        dst.AddEdge(e.GetId(),
                                    dst.GetVertex(e.GetVertex(Direction.Out).GetId()),
                                    dst.GetVertex(e.GetVertex(Direction.In).GetId()),
                                    e.GetLabel()));
            }
        }

        void CompareGraphs(TinkerGraph g1, TinkerGraph g2, TinkerGraph.FileType fileType)
        {
            foreach (IVertex v1 in g1.GetVertices())
            {
                IVertex v2 = g2.GetVertex(v1.GetId());

                CompareEdgeCounts(v1, v2, Direction.In);
                CompareEdgeCounts(v1, v2, Direction.Out);
                CompareEdgeCounts(v1, v2, Direction.Both);

                Assert.True(ElementHelper.HaveEqualProperties(v1, v2));
                Assert.True(ElementHelper.AreEqual(v1, v2));
            }

            foreach (IEdge e1 in g1.GetEdges())
            {
                IEdge e2 = g2.GetEdge(e1.GetId());

                CompareVertices(e1, e2, Direction.In);
                CompareVertices(e2, e2, Direction.Out);

                if (fileType == TinkerGraph.FileType.Gml)
                {
                    // For GML we need to iterate the properties manually to catch the
                    // case where the property returned from GML is an integer
                    // while the target graph property is a float.
                    foreach (String p in e1.GetPropertyKeys())
                    {
                        Object v1 = e1.GetProperty(p);
                        Object v2 = e2.GetProperty(p);

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
                    Assert.True(ElementHelper.HaveEqualProperties(e1, e2));
                }

                Assert.True(ElementHelper.AreEqual(e1, e2));
            }

            IIndex idxAge = g2.GetIndex("age", typeof(IVertex));
            Assert.AreEqual(g2.GetVertex(1), idxAge.Get("age", 29).First());
            Assert.AreEqual(g2.GetVertex(2), idxAge.Get("age", 27).First());

            IIndex idxWeight = g2.GetIndex("weight", typeof(IEdge));
            Assert.AreEqual(g2.GetEdge(7), idxWeight.Get("weight", 0.5).First());
            Assert.AreEqual(g2.GetEdge(12), idxWeight.Get("weight", 0.2).First());

            IEnumerator<IVertex> namesItty = g2.GetVertices("name", "marko").GetEnumerator();
            namesItty.MoveNext();
            Assert.AreEqual(g2.GetVertex(1), namesItty.Current);
            Assert.False(namesItty.MoveNext());

            IEnumerator<IEdge> weightItty = g2.GetEdges("weight", 0.5).GetEnumerator();
            weightItty.MoveNext();
            Assert.AreEqual(g2.GetEdge(7), weightItty.Current);
            Assert.False(weightItty.MoveNext());
        }

        static void CompareEdgeCounts(IVertex v1, IVertex v2, Direction direction)
        {
            int c1 = v1.GetEdges(direction).Count();
            int c2 = v2.GetEdges(direction).Count();

            Assert.AreEqual(c1, c2);
        }

        static void CompareVertices(IEdge e1, IEdge e2, Direction direction)
        {
            IVertex v1 = e1.GetVertex(direction);
            IVertex v2 = e2.GetVertex(direction);

            Assert.AreEqual(v1.GetId(), v2.GetId());
        }
    }
}
