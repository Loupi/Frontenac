using System;
using System.IO;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using Grave;
using NUnit.Framework;

namespace Grave_test
{
    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphGraphTestSuite : GraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphGraphTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphVertexTestSuite : VertexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphVertexTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphVertexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphEdgeTestSuite : EdgeTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphEdgeTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphEdgeTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphKeyIndexableGraphTestSuite : KeyIndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphKeyIndexableGraphTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphKeyIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphIndexableGraphTestSuite : IndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphIndexableGraphTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphIndexTestSuite : IndexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphIndexTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphIndexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }


    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphGmlReaderTestSuite : GmlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphGmlReaderTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphGmlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphGraphMlReaderTestSuite : GraphMlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphGraphMlReaderTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphGraphMlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphGraphSonReaderTestSuite : GraphSonReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphGraphSonReaderTestSuite()
            : base(new GraveGraphTestImpl())
        {
        }

        public GraveGraphGraphSonReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    public class GraveGraphTestImpl : GraphTest
    {
        public override IGraph GenerateGraph()
        {
            return GenerateGraph("graph");
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return GraveFactory.CreateGraph();
        }

        public static string GetGraveGraphDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Data");
        }
    }

    [TestFixture(Category = "GraveGraphGraphTestSuite")]
    public class GraveGraphTestGeneral : TestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveGraphTestGeneral()
            : this(new GraveGraphTestImpl())
        {
        }

        public GraveGraphTestGeneral(GraphTest graphTest) :
            base("GraveGraphTestGeneral", graphTest)
        {
        }

        /*[Test]
        public void TestClear()
        {
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
            using (var graph = (GraveIndexedGraph)GraphTest.GenerateGraph())
            {
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
            }
        }*/

        private void TestGraphFileType(string directory, FileType fileType)
        {
            var path = GraveGraphTestImpl.GetGraveGraphDirectory() + "/" + directory;
            DeleteDirectory(path);

            /*using (var sourceGraph = GraveFactory.CreateTinkerGraph())
            {
                using (var targetGraph = new GraveIndexedGraph(path, fileType))
                {
                    CreateKeyIndices(targetGraph);

                    CopyGraphs(sourceGraph, targetGraph);

                    CreateManualIndices(targetGraph);

                    StopWatch();
                    PrintTestPerformance("save graph: " + fileType.ToString(), StopWatch());
                    StopWatch();

                    //targetGraph.Dispose();

                    using (var compareGraph = new GraveIndexedGraph(path, fileType))
                    {
                        PrintTestPerformance("load graph: " + fileType.ToString(), StopWatch());

                        CompareGraphs(targetGraph, compareGraph, fileType);
                    }
                }
            }*/
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
                ElementHelper.CopyProperties(v, dst.AddVertex(v.Id));
            }

            foreach (var e in src.GetEdges())
            {
                ElementHelper.CopyProperties(e,
                                             dst.AddEdge(e.Id,
                                                         dst.GetVertex(e.GetVertex(Direction.Out).Id),
                                                         dst.GetVertex(e.GetVertex(Direction.In).Id),
                                                         e.Label));
            }
        }

        private void CompareGraphs(IGraph g1, IIndexableGraph g2, FileType fileType)
        {
            foreach (var v1 in g1.GetVertices())
            {
                var v2 = g2.GetVertex(v1.Id);

                CompareEdgeCounts(v1, v2, Direction.In);
                CompareEdgeCounts(v1, v2, Direction.Out);
                CompareEdgeCounts(v1, v2, Direction.Both);

                Assert.True(ElementHelper.HaveEqualProperties(v1, v2));
                Assert.True(ElementHelper.AreEqual(v1, v2));
            }

            foreach (var e1 in g1.GetEdges())
            {
                var e2 = g2.GetEdge(e1.Id);

                CompareVertices(e1, e2, Direction.In);
                CompareVertices(e2, e2, Direction.Out);

                if (fileType == FileType.Gml)
                {
                    // For GML we need to iterate the properties manually to catch the
                    // case where the property returned from GML is an integer
                    // while the target graph property is a float.
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
                    Assert.True(ElementHelper.HaveEqualProperties(e1, e2));
                }

                Assert.True(ElementHelper.AreEqual(e1, e2));
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
        public void TestGraphFileTypeDotNet()
        {
            TestGraphFileType("graph-test-dotnet", FileType.DotNet);
        }

        [Test]
        public void TestGraphFileTypeGml()
        {
            TestGraphFileType("graph-test-gml", FileType.Gml);
        }

        [Test]
        public void TestGraphFileTypeGraphMl()
        {
            TestGraphFileType("graph-test-graphml", FileType.Graphml);
        }

        [Test]
        public void TestGraphFileTypeGraphSon()
        {
            TestGraphFileType("graph-test-graphson", FileType.Graphson);
        }

        [Test]
        public void TestShutdownStartManyTimes()
        {
            var graph = (GraveGraph) GraphTest.GenerateGraph();
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
                graph = (GraveGraph) GraphTest.GenerateGraph();
                try
                {
                    Assert.AreEqual(50, Count(graph.GetVertices()));
                    foreach (var v in graph.GetVertices())
                    {
                        Assert.True(v.GetProperty("name").ToString().StartsWith("a") ||
                                    v.GetProperty("name").ToString().StartsWith("b"));
                    }
                    Assert.AreEqual(25, Count(graph.GetEdges()));
                    foreach (var e in graph.GetEdges())
                    {
                        Assert.AreEqual(e.GetProperty("weight"), 1);
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