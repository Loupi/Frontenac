using System;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using NUnit.Framework;

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
            return new TinkerGrapĥ();
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return new TinkerGrapĥ(GetThinkerGraphDirectory());
        }

        public override ITransactionalGraph GenerateTransactionalGraph()
        {
            throw new NotImplementedException();
        }

        public override ITransactionalGraph GenerateTransactionalGraph(string graphDirectoryName)
        {
            throw new NotImplementedException();
        }

        public static string GetThinkerGraphDirectory()
        {
            string directory = Environment.GetEnvironmentVariable("tinkerGraphDirectory") ?? GetWorkingDirectory();

            return string.Concat(directory, "/Graph");
        }

        private static string GetWorkingDirectory()
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

        private void TestGraphFileType(string directory, TinkerGrapĥ.FileType fileType)
        {
            var path = TinkerGraphTestImpl.GetThinkerGraphDirectory() + "/" + directory;
            DeleteDirectory(path);

            var sourceGraph = TinkerGraphFactory.CreateTinkerGraph();
            try
            {
                var targetGraph = new TinkerGrapĥ(path, fileType);
                try
                {
                    CreateKeyIndices(targetGraph);

                    CopyGraphs(sourceGraph, targetGraph);

                    CreateManualIndices(targetGraph);

                    StopWatch();
                    PrintTestPerformance("save Graph: " + fileType, StopWatch());
                    StopWatch();
                }
                finally
                {
                    targetGraph.Shutdown();
                }

                var compareGraph = new TinkerGrapĥ(path, fileType);
                try
                {
                    PrintTestPerformance("load Graph: " + fileType, StopWatch());

                    CompareGraphs(targetGraph, compareGraph, fileType);
                }
                finally
                {
                    compareGraph.Shutdown();
                }
            }
            finally
            {
                sourceGraph.Shutdown();
            }
        }

        private static void CreateKeyIndices(IKeyIndexableGraph g)
        {
            g.CreateKeyIndex("name", typeof (IVertex));
            g.CreateKeyIndex("weight", typeof (IEdge));
        }

        private static void CreateManualIndices(IIndexableGraph g)
        {
            var ageIndex = g.CreateIndex("age", typeof (IVertex));
            var v1 = g.GetVertex(1);
            var v2 = g.GetVertex(2);
            ageIndex.Put("age", v1.GetProperty("age"), v1);
            ageIndex.Put("age", v2.GetProperty("age"), v2);

            var weightIndex = g.CreateIndex("weight", typeof (IEdge));
            var e7 = g.GetEdge(7);
            var e12 = g.GetEdge(12);
            weightIndex.Put("weight", e7.GetProperty("weight"), e7);
            weightIndex.Put("weight", e12.GetProperty("weight"), e12);
        }

        private static void CopyGraphs(IGraph src, IGraph dst)
        {
            foreach (var v in src.GetVertices())
            {
                v.CopyProperties(dst.AddVertex(v.Id));
            }

            foreach (var e in src.GetEdges())
            {
                e.CopyProperties(dst.AddEdge(e.Id,
                                   dst.GetVertex(e.GetVertex(Direction.Out).Id),
                                   dst.GetVertex(e.GetVertex(Direction.In).Id),
                                   e.Label));
            }
        }

        private static void CompareGraphs(IGraph g1, IIndexableGraph g2, TinkerGrapĥ.FileType fileType)
        {
            foreach (var v1 in g1.GetVertices())
            {
                var v2 = g2.GetVertex(v1.Id);

                CompareEdgeCounts(v1, v2, Direction.In);
                CompareEdgeCounts(v1, v2, Direction.Out);
                CompareEdgeCounts(v1, v2, Direction.Both);

                Assert.True(v1.HaveEqualProperties(v2));
                Assert.True(v1.AreEqual(v2));
            }

            foreach (var e1 in g1.GetEdges())
            {
                var e2 = g2.GetEdge(e1.Id);

                CompareVertices(e1, e2, Direction.In);
                CompareVertices(e2, e2, Direction.Out);

                if (fileType == TinkerGrapĥ.FileType.Gml)
                {
                    // For GML we need to iterate the properties manually to catch the
                    // case where the property returned from GML is an integer
                    // while the target Graph property is a float.
                    foreach (var p in e1.GetPropertyKeys())
                    {
                        var v1 = e1.GetProperty(p);
                        var v2 = e2.GetProperty(p);

                        if (v1.GetType() != v2.GetType())
                        {
                            if ((v1 is float) && (v2 is int))
                            {
                                Assert.AreEqual(v1, (float) ((int) v2));
                            }
                            else if ((v1 is int) && (v2 is float))
                            {
                                Assert.AreEqual((float) ((int) v1), v2);
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
                    Assert.True(e1.HaveEqualProperties(e2));
                }

                Assert.True(e1.AreEqual(e2));
            }

            var idxAge = g2.GetIndex("age", typeof (IVertex));
            Assert.AreEqual(g2.GetVertex(1), idxAge.Get("age", 29).First());
            Assert.AreEqual(g2.GetVertex(2), idxAge.Get("age", 27).First());

            var idxWeight = g2.GetIndex("weight", typeof (IEdge));
            Assert.AreEqual(g2.GetEdge(7), idxWeight.Get("weight", 0.5).First());
            Assert.AreEqual(g2.GetEdge(12), idxWeight.Get("weight", 0.2).First());

            var namesItty = g2.GetVertices("name", "marko").GetEnumerator();
            namesItty.MoveNext();
            Assert.AreEqual(g2.GetVertex(1), namesItty.Current);
            Assert.False(namesItty.MoveNext());

            var weightItty = g2.GetEdges("weight", 0.5).GetEnumerator();
            weightItty.MoveNext();
            Assert.AreEqual(g2.GetEdge(7), weightItty.Current);
            Assert.False(weightItty.MoveNext());
        }

        private static void CompareEdgeCounts(IVertex v1, IVertex v2, Direction direction)
        {
            var c1 = v1.GetEdges(direction).Count();
            var c2 = v2.GetEdges(direction).Count();

            Assert.AreEqual(c1, c2);
        }

        private static void CompareVertices(IEdge e1, IEdge e2, Direction direction)
        {
            var v1 = e1.GetVertex(direction);
            var v2 = e2.GetVertex(direction);

            Assert.AreEqual(v1.Id, v2.Id);
        }

        [Test]
        public void TestClear()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
            var graph = (TinkerGrapĥ) GraphTest.GenerateGraph();
            try
            {
                StopWatch();
                for (var i = 0; i < 25; i++)
                {
                    var a = graph.AddVertex(null);
                    var b = graph.AddVertex(null);
                    graph.AddEdge(null, a, b, "knows");
                }
                PrintPerformance(graph.ToString(), 75, "elements added", StopWatch());

                Assert.AreEqual(Count(graph.GetVertices()), 50);
                Assert.AreEqual(Count(graph.GetEdges()), 25);

                StopWatch();
                graph.Clear();
                PrintPerformance(graph.ToString(), 75, "elements deleted", StopWatch());

                Assert.AreEqual(Count(graph.GetVertices()), 0);
                Assert.AreEqual(Count(graph.GetEdges()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGraphFileTypeGml()
        {
            TestGraphFileType("Graph-test-gml", TinkerGrapĥ.FileType.Gml);
        }

        [Test]
        public void TestGraphFileTypeGraphMl()
        {
            TestGraphFileType("Graph-test-graphml", TinkerGrapĥ.FileType.Graphml);
        }

        [Test]
        public void TestGraphFileTypeGraphSon()
        {
            TestGraphFileType("InnerGraph-test-graphson", TinkerGrapĥ.FileType.Graphson);
        }

        [Test]
        public void TestGraphFileTypeJava()
        {
            TestGraphFileType("Graph-test-java", TinkerGrapĥ.FileType.DotNet);
        }

        [Test]
        public void TestShutdownStartManyTimes()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
            var graph = (TinkerGrapĥ) GraphTest.GenerateGraph("Graph");
            try
            {
                for (var i = 0; i < 25; i++)
                {
                    var a = graph.AddVertex(null);
                    a.SetProperty("name", string.Concat("a", Guid.NewGuid()));
                    var b = graph.AddVertex(null);
                    b.SetProperty("name", string.Concat("b", Guid.NewGuid()));
                    graph.AddEdge(null, a, b, "knows").SetProperty("weight", 1);
                }
            }
            finally
            {
                graph.Shutdown();
            }
            StopWatch();
            const int iterations = 150;
            for (var i = 0; i < iterations; i++)
            {
                graph = (TinkerGrapĥ) GraphTest.GenerateGraph("graph");
                try
                {
                    Assert.AreEqual(50, Count(graph.GetVertices()));
                    foreach (var v in graph.GetVertices())
                    {
                        Assert.True(v.GetProperty("name").ToString().StartsWith("a") ||
                                    v.GetProperty("name").ToString().StartsWith("b"));
                    }
                    Assert.AreEqual(Count(graph.GetEdges()), 25);
                    foreach (var e in graph.GetEdges())
                    {
                        Assert.AreEqual(1, e.GetProperty("weight"));
                    }
                    PrintPerformance(graph.ToString(), iterations, "iterations of shutdown and restart", StopWatch());
                }
                finally
                {
                    graph.Shutdown();
                }
            }
        }
    }
}