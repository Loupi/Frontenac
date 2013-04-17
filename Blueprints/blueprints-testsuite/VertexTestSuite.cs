using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.Sail;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints
{
    public abstract class VertexTestSuite : TestSuite
    {
        public VertexTestSuite(GraphTest graphTest)
            : base("VertexTestSuite", graphTest)
        {

        }

        [Test]
        public void TestVertexEquality()
        {
            Vertex v;
            Vertex u;
            Graph graph = _GraphTest.GenerateGraph();

            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                v = graph.AddVertex(ConvertId(graph, "1"));
                u = graph.GetVertex(ConvertId(graph, "1"));
                Assert.AreEqual(v, u);
            }

            this.StopWatch();
            v = graph.AddVertex(null);
            Assert.False(v == null);
            u = graph.GetVertex(v.GetId());
            Assert.AreEqual(v, u);
            PrintPerformance(graph.ToString(), 1, "vertex added and retrieved", this.StopWatch());

            Assert.AreEqual(graph.GetVertex(u.GetId()), graph.GetVertex(u.GetId()));
            Assert.AreEqual(graph.GetVertex(v.GetId()), graph.GetVertex(u.GetId()));
            Assert.AreEqual(graph.GetVertex(v.GetId()), graph.GetVertex(v.GetId()));

            graph.Shutdown();
        }

        [Test]
        public void TestVertexEqualityForSuppliedIdsAndHashCode()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
            {
                Vertex v = graph.AddVertex(ConvertId(graph, "1"));
                Vertex u = graph.GetVertex(ConvertId(graph, "1"));
                HashSet<Vertex> set = new HashSet<Vertex>();
                set.Add(v);
                set.Add(v);
                set.Add(u);
                set.Add(u);
                set.Add(graph.GetVertex(ConvertId(graph, "1")));
                set.Add(graph.GetVertex(ConvertId(graph, "1")));
                if (graph.GetFeatures().SupportsVertexIndex.Value)
                    set.Add(graph.GetVertices().First());
                Assert.AreEqual(1, set.Count());
                Assert.AreEqual(v.GetHashCode(), u.GetHashCode());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestBasicAddVertex()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                graph.AddVertex(ConvertId(graph, "1"));
                graph.AddVertex(ConvertId(graph, "2"));
                Assert.AreEqual(2, Count(graph.GetVertices()));
                graph.AddVertex(ConvertId(graph, "3"));
                Assert.AreEqual(3, Count(graph.GetVertices()));
            }

            if (graph.GetFeatures().IsRDFModel.Value)
            {
                Vertex v1 = graph.AddVertex("http://tinkerpop.com#marko");
                Assert.AreEqual("http://tinkerpop.com#marko", v1.GetId());
                Vertex v2 = graph.AddVertex("\"1\"^^<datatype:int>");
                Assert.AreEqual("\"1\"^^<datatype:int>", v2.GetId());
                Vertex v3 = graph.AddVertex("_:ABLANKNODE");
                Assert.AreEqual(v3.GetId(), "_:ABLANKNODE");
                Vertex v4 = graph.AddVertex("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>");
                Assert.AreEqual("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>", v4.GetId());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGetVertexWithNull()
        {
            Graph graph = _GraphTest.GenerateGraph();
            try
            {
                graph.GetVertex(null);
                Assert.False(true);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertex()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
                Assert.AreEqual(graph.GetVertex(ConvertId(graph, "1")), v1);

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            HashSet<Vertex> vertices = new HashSet<Vertex>();
            for (int i = 0; i < 100; i++)
                vertices.Add(graph.AddVertex(null));

            Assert.AreEqual(vertices.Count(), 100);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(100, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            foreach (Vertex v in vertices)
                graph.RemoveVertex(v);

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexWithEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(2, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(1, Count(graph.GetEdges()));

            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.RemoveVertex(v2);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(0, Count(graph.GetEdges()));
            graph.Shutdown();
        }

        [Test]
        public void TestGetNonExistantVertices()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Assert.Null(graph.GetVertex("asbv"));
            Assert.Null(graph.GetVertex(12.0d));
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexNullId()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v1 = graph.AddVertex(null);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));

            HashSet<Vertex> vertices = new HashSet<Vertex>();

            this.StopWatch();
            int vertexCount = 100;
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.AddVertex(null));

            PrintPerformance(graph.ToString(), vertexCount, "vertices added", this.StopWatch());
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));

            this.StopWatch();
            foreach (Vertex v in vertices)
                graph.RemoveVertex(v);

            PrintPerformance(graph.ToString(), vertexCount, "vertices removed", this.StopWatch());
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            graph.Shutdown();
        }

        [Test]
        public void TestVertexIterator()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                this.StopWatch();
                int vertexCount = 1000;
                HashSet<object> ids = new HashSet<object>();
                for (int i = 0; i < vertexCount; i++)
                    ids.Add(graph.AddVertex(null).GetId());

                PrintPerformance(graph.ToString(), vertexCount, "vertices added", this.StopWatch());
                this.StopWatch();
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), vertexCount, "vertices Counted", this.StopWatch());
                // must create unique ids
                Assert.AreEqual(vertexCount, ids.Count());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestLegalVertexEdgeIterables()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex v1 = graph.AddVertex(null);
            for (int i = 0; i < 10; i++)
                graph.AddEdge(null, v1, graph.AddVertex(null), ConvertId(graph, "knows"));

            IEnumerable<Edge> edges = v1.GetEdges(Direction.OUT, ConvertId(graph, "knows"));
            Assert.AreEqual(Count(edges), 10);
            Assert.AreEqual(Count(edges), 10);
            Assert.AreEqual(Count(edges), 10);
            graph.Shutdown();
        }

        [Test]
        public void TestAddVertexProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex v1 = graph.AddVertex(ConvertId(graph, "1"));
                Vertex v2 = graph.AddVertex(ConvertId(graph, "2"));

                if (graph.GetFeatures().SupportsStringProperty.Value)
                {
                    v1.SetProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.GetProperty("key1"));
                }

                if (graph.GetFeatures().SupportsIntegerProperty.Value)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);

                    Assert.AreEqual(10, v1.GetProperty("key2"));
                    Assert.AreEqual(20, v2.GetProperty("key2"));
                }

            }
            else if (graph.GetFeatures().IsRDFModel.Value)
            {
                Vertex v1 = graph.AddVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.GetProperty(SailTokens.DATATYPE));
                Assert.AreEqual(1, v1.GetProperty(SailTokens.VALUE));
                Assert.Null(v1.GetProperty(SailTokens.LANGUAGE));
                Assert.Null(v1.GetProperty("random something"));

                Vertex v2 = graph.AddVertex("\"hello\"@en");
                Assert.AreEqual("en", v2.GetProperty(SailTokens.LANGUAGE));
                Assert.AreEqual("hello", v2.GetProperty(SailTokens.VALUE));
                Assert.Null(v2.GetProperty(SailTokens.DATATYPE));
                Assert.Null(v2.GetProperty("random something"));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddManyVertexProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value && graph.GetFeatures().SupportsStringProperty.Value)
            {
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                this.StopWatch();
                for (int i = 0; i < 50; i++)
                {
                    Vertex vertex = graph.AddVertex(null);
                    for (int j = 0; j < 15; j++)
                        vertex.SetProperty(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

                    vertices.Add(vertex);
                }
                PrintPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", this.StopWatch());

                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(50, Count(graph.GetVertices()));
                Assert.AreEqual(50, vertices.Count());
                foreach (Vertex vertex in vertices)
                    Assert.AreEqual(15, vertex.GetPropertyKeys().Count());

            }
            else if (graph.GetFeatures().IsRDFModel.Value)
            {
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                this.StopWatch();
                for (int i = 0; i < 50; i++)
                {
                    Vertex vertex = graph.AddVertex(string.Concat("\"", Guid.NewGuid().ToString(), "\""));
                    for (int j = 0; j < 15; j++)
                        vertex.SetProperty(SailTokens.DATATYPE, "http://www.w3.org/2001/XMLSchema#anyURI");

                    vertices.Add(vertex);
                }
                PrintPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", this.StopWatch());
                if (graph.GetFeatures().SupportsVertexIteration.Value)
                    Assert.AreEqual(Count(graph.GetVertices()), 50);
                Assert.AreEqual(vertices.Count(), 50);
                foreach (Vertex vertex in vertices)
                {
                    Assert.AreEqual(3, vertex.GetPropertyKeys().Count());
                    Assert.True(vertex.GetPropertyKeys().Contains(SailTokens.DATATYPE));
                    Assert.AreEqual("http://www.w3.org/2001/XMLSchema#anyURI", vertex.GetProperty(SailTokens.DATATYPE));
                    Assert.True(vertex.GetPropertyKeys().Contains(SailTokens.VALUE));
                    Assert.AreEqual("literal", vertex.GetProperty(SailTokens.KIND));

                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {

                Vertex v1 = graph.AddVertex("1");
                Vertex v2 = graph.AddVertex("2");

                Assert.Null(v1.RemoveProperty("key1"));
                Assert.Null(v1.RemoveProperty("key2"));
                Assert.Null(v2.RemoveProperty("key2"));

                if (graph.GetFeatures().SupportsStringProperty.Value)
                {
                    v1.SetProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.RemoveProperty("key1"));
                }

                if (graph.GetFeatures().SupportsIntegerProperty.Value)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);

                    Assert.AreEqual(10, v1.RemoveProperty("key2"));
                    Assert.AreEqual(20, v2.RemoveProperty("key2"));
                }

                Assert.Null(v1.RemoveProperty("key1"));
                Assert.Null(v1.RemoveProperty("key2"));
                Assert.Null(v2.RemoveProperty("key2"));

                if (graph.GetFeatures().SupportsStringProperty.Value)
                    v1.SetProperty("key1", "value1");

                if (graph.GetFeatures().SupportsIntegerProperty.Value)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);
                }

                if (!graph.GetFeatures().IgnoresSuppliedIds.Value)
                {
                    v1 = graph.GetVertex("1");
                    v2 = graph.GetVertex("2");

                    if (graph.GetFeatures().SupportsStringProperty.Value)
                        Assert.AreEqual("value1", v1.RemoveProperty("key1"));

                    if (graph.GetFeatures().SupportsIntegerProperty.Value)
                    {
                        Assert.AreEqual(10, v1.RemoveProperty("key2"));
                        Assert.AreEqual(20, v2.RemoveProperty("key2"));
                    }

                    Assert.Null(v1.RemoveProperty("key1"));
                    Assert.Null(v1.RemoveProperty("key2"));
                    Assert.Null(v2.RemoveProperty("key2"));

                    v1 = graph.GetVertex("1");
                    v2 = graph.GetVertex("2");

                    if (graph.GetFeatures().SupportsStringProperty.Value)
                    {
                        v1.SetProperty("key1", "value2");
                        Assert.AreEqual("value2", v1.RemoveProperty("key1"));
                    }

                    if (graph.GetFeatures().SupportsIntegerProperty.Value)
                    {
                        v1.SetProperty("key2", 20);
                        v2.SetProperty("key2", 30);

                        Assert.AreEqual(20, v1.RemoveProperty("key2"));
                        Assert.AreEqual(30, v2.RemoveProperty("key2"));
                    }

                    Assert.Null(v1.RemoveProperty("key1"));
                    Assert.Null(v1.RemoveProperty("key2"));
                    Assert.Null(v2.RemoveProperty("key2"));
                }
            }
            else if (graph.GetFeatures().IsRDFModel.Value)
            {
                Vertex v1 = graph.AddVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.RemoveProperty("type"));
                Assert.AreEqual("1", v1.GetProperty("value"));
                Assert.Null(v1.GetProperty("lang"));
                Assert.Null(v1.GetProperty("random something"));

                Vertex v2 = graph.AddVertex("\"hello\"@en");
                Assert.AreEqual("en", v2.RemoveProperty("lang"));
                Assert.AreEqual("hello", v2.GetProperty("value"));
                Assert.Null(v2.GetProperty("type"));
                Assert.Null(v2.GetProperty("random something"));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddingIdProperty()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex vertex = graph.AddVertex(null);
                try
                {
                    vertex.SetProperty("id", "123");
                    Assert.True(false);
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestNoConcurrentModificationException()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration.Value)
            {
                for (int i = 0; i < 25; i++)
                    graph.AddVertex(null);

                Assert.AreEqual(Count(graph.GetVertices()), 25);
                foreach (Vertex vertex in graph.GetVertices())
                    graph.RemoveVertex(vertex);

                Assert.AreEqual(Count(graph.GetVertices()), 0);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGettingEdgesAndVertices()
        {
            Graph graph = _GraphTest.GenerateGraph();
            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);
            Vertex c = graph.AddVertex(null);
            Edge w = graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
            Edge x = graph.AddEdge(null, b, c, ConvertId(graph, "knows"));
            Edge y = graph.AddEdge(null, a, c, ConvertId(graph, "hates"));
            Edge z = graph.AddEdge(null, a, b, ConvertId(graph, "hates"));
            Edge zz = graph.AddEdge(null, c, c, ConvertId(graph, "hates"));

            Assert.AreEqual(Count(a.GetEdges(Direction.OUT)), 3);
            Assert.AreEqual(Count(a.GetEdges(Direction.OUT, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetEdges(Direction.OUT, ConvertId(graph, "knows"))), 1);
            Assert.AreEqual(Count(a.GetVertices(Direction.OUT)), 3);
            Assert.AreEqual(Count(a.GetVertices(Direction.OUT, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetVertices(Direction.OUT, ConvertId(graph, "knows"))), 1);
            Assert.AreEqual(Count(a.GetVertices(Direction.BOTH)), 3);
            Assert.AreEqual(Count(a.GetVertices(Direction.BOTH, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetVertices(Direction.BOTH, ConvertId(graph, "knows"))), 1);

            Assert.True(a.GetEdges(Direction.OUT).Contains(w));
            Assert.True(a.GetEdges(Direction.OUT).Contains(y));
            Assert.True(a.GetEdges(Direction.OUT).Contains(z));
            Assert.True(a.GetVertices(Direction.OUT).Contains(b));
            Assert.True(a.GetVertices(Direction.OUT).Contains(c));

            Assert.True(a.GetEdges(Direction.OUT, ConvertId(graph, "knows")).Contains(w));
            Assert.False(a.GetEdges(Direction.OUT, ConvertId(graph, "knows")).Contains(y));
            Assert.False(a.GetEdges(Direction.OUT, ConvertId(graph, "knows")).Contains(z));
            Assert.True(a.GetVertices(Direction.OUT, ConvertId(graph, "knows")).Contains(b));
            Assert.False(a.GetVertices(Direction.OUT, ConvertId(graph, "knows")).Contains(c));

            Assert.False(a.GetEdges(Direction.OUT, ConvertId(graph, "hates")).Contains(w));
            Assert.True(a.GetEdges(Direction.OUT, ConvertId(graph, "hates")).Contains(y));
            Assert.True(a.GetEdges(Direction.OUT, ConvertId(graph, "hates")).Contains(z));
            Assert.True(a.GetVertices(Direction.OUT, ConvertId(graph, "hates")).Contains(b));
            Assert.True(a.GetVertices(Direction.OUT, ConvertId(graph, "hates")).Contains(c));

            Assert.AreEqual(Count(a.GetVertices(Direction.IN)), 0);
            Assert.AreEqual(Count(a.GetVertices(Direction.IN, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(a.GetVertices(Direction.IN, ConvertId(graph, "hates"))), 0);
            Assert.True(a.GetEdges(Direction.OUT).Contains(w));
            Assert.True(a.GetEdges(Direction.OUT).Contains(y));
            Assert.True(a.GetEdges(Direction.OUT).Contains(z));

            Assert.AreEqual(Count(b.GetEdges(Direction.BOTH)), 3);
            Assert.AreEqual(Count(b.GetEdges(Direction.BOTH, ConvertId(graph, "knows"))), 2);
            Assert.True(b.GetEdges(Direction.BOTH, ConvertId(graph, "knows")).Contains(x));
            Assert.True(b.GetEdges(Direction.BOTH, ConvertId(graph, "knows")).Contains(w));
            Assert.True(b.GetVertices(Direction.BOTH, ConvertId(graph, "knows")).Contains(a));
            Assert.True(b.GetVertices(Direction.BOTH, ConvertId(graph, "knows")).Contains(c));

            Assert.AreEqual(Count(c.GetEdges(Direction.BOTH, ConvertId(graph, "hates"))), 3);
            Assert.AreEqual(Count(c.GetVertices(Direction.BOTH, ConvertId(graph, "hates"))), 3);
            Assert.AreEqual(Count(c.GetEdges(Direction.BOTH, ConvertId(graph, "knows"))), 1);
            Assert.True(c.GetEdges(Direction.BOTH, ConvertId(graph, "hates")).Contains(y));
            Assert.True(c.GetEdges(Direction.BOTH, ConvertId(graph, "hates")).Contains(zz));
            Assert.True(c.GetVertices(Direction.BOTH, ConvertId(graph, "hates")).Contains(a));
            Assert.True(c.GetVertices(Direction.BOTH, ConvertId(graph, "hates")).Contains(c));
            Assert.AreEqual(Count(c.GetEdges(Direction.IN, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(c.GetEdges(Direction.OUT, ConvertId(graph, "hates"))), 1);

            try
            {
                x.GetVertex(Direction.BOTH);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }

            graph.Shutdown();
        }

        [Test]
        public void TestEmptyKeyProperty()
        {
            Graph graph = _GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex v = graph.AddVertex(null);
                try
                {
                    v.SetProperty("", "value");
                    Assert.Fail();
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestVertexCentricLinking()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex v = graph.AddVertex(null);
            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);

            v.AddEdge(ConvertId(graph, "knows"), a);
            v.AddEdge(ConvertId(graph, "knows"), b);

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), 3);
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), 2);

            Assert.AreEqual(Count(v.GetEdges(Direction.OUT, ConvertId(graph, "knows"))), 2);
            Assert.AreEqual(Count(a.GetEdges(Direction.OUT, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(a.GetEdges(Direction.IN, ConvertId(graph, "knows"))), 1);

            Assert.AreEqual(Count(b.GetEdges(Direction.OUT, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(b.GetEdges(Direction.IN, ConvertId(graph, "knows"))), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestVertexCentricRemoving()
        {
            Graph graph = _GraphTest.GenerateGraph();

            Vertex a = graph.AddVertex(null);
            Vertex b = graph.AddVertex(null);
            Vertex c = graph.AddVertex(null);

            object cId = c.GetId();

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), 3);

            a.Remove();
            b.Remove();

            Assert.NotNull(graph.GetVertex(cId));

            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestConcurrentModificationOnProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex a = graph.AddVertex(null);
                a.SetProperty("test1", 1);
                a.SetProperty("test2", 2);
                a.SetProperty("test3", 3);
                a.SetProperty("test4", 4);
                foreach (string key in a.GetPropertyKeys())
                    a.RemoveProperty(key);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestSettingBadVertexProperties()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex v = graph.AddVertex(null);
                try
                {
                    v.SetProperty(null, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.SetProperty("", -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.SetProperty(StringFactory.ID, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.SetProperty(ConvertId(graph, "good"), null);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }
            }
            graph.Shutdown();
        }
    }
}
