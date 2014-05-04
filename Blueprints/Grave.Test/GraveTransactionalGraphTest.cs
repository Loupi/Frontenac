using System;
using System.IO;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using NUnit.Framework;

namespace Frontenac.Grave.Tests
{
    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphGraphTestSuite : GraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphGraphTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphVertexTestSuite : VertexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphVertexTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphVertexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphEdgeTestSuite : EdgeTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphEdgeTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphEdgeTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphKeyIndexableGraphTestSuite : KeyIndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphKeyIndexableGraphTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphKeyIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphIndexableGraphTestSuite : IndexableGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphIndexableGraphTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphIndexableGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphIndexTestSuite : IndexTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphIndexTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphIndexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }


    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphGmlReaderTestSuite : GmlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphGmlReaderTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphGmlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphGraphMlReaderTestSuite : GraphMlReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphGraphMlReaderTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphGraphMlReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphGraphSonReaderTestSuite : GraphSonReaderTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphGraphSonReaderTestSuite()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphGraphSonReaderTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphTestImpl : GraphTest
    {
        public override IGraph GenerateGraph()
        {
            return GenerateGraph("graph");
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return GraveFactory.CreateTransactionalGraph();
        }

        public static string GetGraveGraphDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "GraveGraph");
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphTest : TransactionalGraphTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphTest()
            : base(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphTest(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "GraveTransactionalGraphGraphTestSuite")]
    public class GraveTransactionalGraphTestGeneral : TestSuite
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveTransactionalGraphTestImpl.GetGraveGraphDirectory());
        }

        public GraveTransactionalGraphTestGeneral()
            : this(new GraveTransactionalGraphTestImpl())
        {
        }

        public GraveTransactionalGraphTestGeneral(GraphTest graphTest) :
            base("GraveTransactionalGraphTestGeneral", graphTest)
        {
        }

        private void TestGraphFileType(string directory, FileType fileType)
        {
            var path = GraveTransactionalGraphTestImpl.GetGraveGraphDirectory() + "/" + directory;
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