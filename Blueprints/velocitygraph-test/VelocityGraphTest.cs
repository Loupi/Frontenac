using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Util;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using VelocityGraph;
using VelocityDb.Session;

namespace Frontenac.Blueprints.Impls.VG
{
  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphGraphTestSuite : GraphTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    [TearDown]
    public void Dispose()
    {
      FixtureTearDown();
    }

    public VelocityGraphGraphTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphGraphTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphVertexTestSuite : VertexTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    public VelocityGraphVertexTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphVertexTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphEdgeTestSuite : EdgeTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    public VelocityGraphEdgeTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphEdgeTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphGmlReaderTestSuite : GmlReaderTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    public VelocityGraphGmlReaderTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphGmlReaderTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphGraphMlReaderTestSuite : GraphMlReaderTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    public VelocityGraphGraphMlReaderTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphGraphMlReaderTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphGraphSonReaderTestSuite : GraphSonReaderTestSuite
  {
    [SetUp]
    public void SetUp()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
    }

    public VelocityGraphGraphSonReaderTestSuite()
      : base(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphGraphSonReaderTestSuite(GraphTest graphTest)
      : base(graphTest)
    {
    }
  }

  public class  VelocityGraphTestImpl : GraphTest
  {
    public override IGraph GenerateGraph()
    {
      return GenerateGraph("graph");
    }

    public override IGraph GenerateGraph(string graphDirectoryName)
    {
      string dir = GetVelocityGraphDirectory(graphDirectoryName);
      SessionBase session = new SessionNoServer(dir);
      session.BeginUpdate();
      Graph graph = Graph.Open(session);
      if (graph != null)
      {
        return graph;
      }
      // You need to put a VelocityDB license file in c: root
      File.Copy(Path.Combine("c:\\", "4.odb"), Path.Combine(dir, "4.odb"));
      graph = new Graph(session);
      session.Persist(graph);
      return graph;
    }

    public static string GetVelocityGraphDirectory(string subdir = "graph")
    {
      string directory = Environment.GetEnvironmentVariable("VelocityGraphDirectory") ?? GetWorkingDirectory();

      return System.IO.Path.Combine(directory,subdir);
    }

    static string GetWorkingDirectory()
    {
      return Directory.GetCurrentDirectory();
    }
  }

  [TestFixture(Category = "VelocityGraphGraphTestSuite")]
  public class VelocityGraphTestGeneral : TestSuite
  {
    public VelocityGraphTestGeneral()
      : this(new VelocityGraphTestImpl())
    {
    }

    public VelocityGraphTestGeneral(GraphTest graphTest) :
      base("VelocityGraphTestGeneral", graphTest)
    {

    }

    [Test]
    public void TestClear()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
      var graph = (Graph)GraphTest.GenerateGraph();

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
      graph.Session.Commit();
    }

    [Test]
    public void TestShutdownStartManyTimes()
    {
      DeleteDirectory(VelocityGraphTestImpl.GetVelocityGraphDirectory());
      var graph = (Graph)GraphTest.GenerateGraph();
      for (int i = 0; i < 25; i++)
      {
        IVertex a = graph.AddVertex(null);
        a.SetProperty("name", string.Concat("a", Guid.NewGuid()));
        IVertex b = graph.AddVertex(null);
        b.SetProperty("name", string.Concat("b", Guid.NewGuid()));
        graph.AddEdge(null, a, b, "knows").SetProperty("weight", 1);
      }
      graph.Session.Commit();
      StopWatch();
      const int iterations = 150;
      for (int i = 0; i < iterations; i++)
      {        
        graph = (Graph)GraphTest.GenerateGraph();
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
        graph.Session.Commit();
      }
      PrintPerformance(graph.ToString(), iterations, "iterations of shutdown and restart", StopWatch());
    }


    [Test]
    public void TestGraphFileTypeJava()
    {
      TestGraphFileType("graph-test-java", FileType.Java);
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

    void TestGraphFileType(string directory, FileType fileType)
    {
      string path = VelocityGraphTestImpl.GetVelocityGraphDirectory() + "/" + directory;
      DeleteDirectory(path);
      using (SessionBase session = new SessionNoServer(directory))
      {
        Graph sourceGraph = VelocityGraphFactory.CreateVelocityGraph(session);
        var targetGraph = new Graph(session);

        CopyGraphs(sourceGraph, targetGraph);

        StopWatch();
        PrintTestPerformance("save graph: " + fileType.ToString(), StopWatch());

        StopWatch();
        var compareGraph = VelocityGraphFactory.CreateVelocityGraph(session);
        PrintTestPerformance("load graph: " + fileType.ToString(), StopWatch());

        CompareGraphs(targetGraph, compareGraph, fileType);
      }
    }

    static void CopyGraphs(Graph src, Graph dst)
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

    void CompareGraphs(Graph g1, Graph g2, FileType fileType)
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

        if (fileType == FileType.Gml)
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

      IEnumerator<IEdge> weightItty = g2.GetEdges("weight", 0.5).GetEnumerator();
      weightItty.MoveNext();
      Assert.AreEqual(g2.GetEdge(weightItty.Current.GetId()), weightItty.Current);
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
