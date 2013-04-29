using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.Sail;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints
{
    public abstract class VertexTestSuite : TestSuite
    {
        protected VertexTestSuite(GraphTest graphTest)
            : base("VertexTestSuite", graphTest)
        {

        }

        [Test]
        public void TestVertexEquality()
        {
            IVertex v;
            IVertex u;
            IGraph graph = GraphTest.GenerateGraph();

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                v = graph.AddVertex(ConvertId(graph, "1"));
                u = graph.GetVertex(ConvertId(graph, "1"));
                Assert.AreEqual(v, u);
            }

            StopWatch();
            v = graph.AddVertex(null);
            Assert.False(v == null);
            u = graph.GetVertex(v.GetId());
            Assert.AreEqual(v, u);
            PrintPerformance(graph.ToString(), 1, "vertex added and retrieved", StopWatch());

            Assert.AreEqual(graph.GetVertex(u.GetId()), graph.GetVertex(u.GetId()));
            Assert.AreEqual(graph.GetVertex(v.GetId()), graph.GetVertex(u.GetId()));
            Assert.AreEqual(graph.GetVertex(v.GetId()), graph.GetVertex(v.GetId()));

            graph.Shutdown();
        }

        [Test]
        public void TestVertexEqualityForSuppliedIdsAndHashCode()
        {
            var graph = GraphTest.GenerateGraph();

            if (!graph.GetFeatures().IgnoresSuppliedIds)
            {
                var v = graph.AddVertex(ConvertId(graph, "1"));
                var u = graph.GetVertex(ConvertId(graph, "1"));
                var set = new HashSet<IVertex>
                    {
                        v,
                        v,
                        u,
                        u,
                        graph.GetVertex(ConvertId(graph, "1")),
                        graph.GetVertex(ConvertId(graph, "1"))
                    };

                if (graph.GetFeatures().SupportsVertexIndex)
                    set.Add(graph.GetVertices().First());
                Assert.AreEqual(1, set.Count());
                Assert.AreEqual(v.GetHashCode(), u.GetHashCode());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestBasicAddVertex()
        {
            var graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration)
            {
                graph.AddVertex(ConvertId(graph, "1"));
                graph.AddVertex(ConvertId(graph, "2"));
                Assert.AreEqual(2, Count(graph.GetVertices()));
                graph.AddVertex(ConvertId(graph, "3"));
                Assert.AreEqual(3, Count(graph.GetVertices()));
            }

            if (graph.GetFeatures().IsRdfModel)
            {
                var v1 = graph.AddVertex("http://tinkerpop.com#marko");
                Assert.AreEqual("http://tinkerpop.com#marko", v1.GetId());
                var v2 = graph.AddVertex("\"1\"^^<datatype:int>");
                Assert.AreEqual("\"1\"^^<datatype:int>", v2.GetId());
                var v3 = graph.AddVertex("_:ABLANKNODE");
                Assert.AreEqual(v3.GetId(), "_:ABLANKNODE");
                var v4 = graph.AddVertex("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>");
                Assert.AreEqual("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>", v4.GetId());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGetVertexWithNull()
        {
            var graph = GraphTest.GenerateGraph();
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
            var graph = GraphTest.GenerateGraph();

            var v1 = graph.AddVertex(ConvertId(graph, "1"));
            if (!graph.GetFeatures().IgnoresSuppliedIds)
                Assert.AreEqual(graph.GetVertex(ConvertId(graph, "1")), v1);

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            var vertices = new HashSet<IVertex>();
            for (int i = 0; i < 100; i++)
                vertices.Add(graph.AddVertex(null));

            Assert.AreEqual(vertices.Count(), 100);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(100, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            foreach (IVertex v in vertices)
                graph.RemoveVertex(v);

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexWithEdges()
        {
            IGraph graph = GraphTest.GenerateGraph();
            IVertex v1 = graph.AddVertex(ConvertId(graph, "1"));
            IVertex v2 = graph.AddVertex(ConvertId(graph, "2"));
            graph.AddEdge(null, v1, v2, ConvertId(graph, "knows"));
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(2, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(1, Count(graph.GetEdges()));

            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));

            graph.RemoveVertex(v2);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(0, Count(graph.GetEdges()));
            graph.Shutdown();
        }

        [Test]
        public void TestGetNonExistantVertices()
        {
            IGraph graph = GraphTest.GenerateGraph();
            Assert.Null(graph.GetVertex("asbv"));
            Assert.Null(graph.GetVertex(12.0d));
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexNullId()
        {
            IGraph graph = GraphTest.GenerateGraph();

            IVertex v1 = graph.AddVertex(null);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(1, Count(graph.GetVertices()));
            graph.RemoveVertex(v1);
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(0, Count(graph.GetVertices()));

            var vertices = new HashSet<IVertex>();

            StopWatch();
            const int vertexCount = 100;
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.AddVertex(null));

            PrintPerformance(graph.ToString(), vertexCount, "vertices added", StopWatch());
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));

            StopWatch();
            foreach (IVertex v in vertices)
                graph.RemoveVertex(v);

            PrintPerformance(graph.ToString(), vertexCount, "vertices removed", StopWatch());
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(0, Count(graph.GetVertices()));
            graph.Shutdown();
        }

        [Test]
        public void TestVertexIterator()
        {
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration)
            {
                StopWatch();
                const int vertexCount = 1000;
                var ids = new HashSet<object>();
                for (int i = 0; i < vertexCount; i++)
                    ids.Add(graph.AddVertex(null).GetId());

                PrintPerformance(graph.ToString(), vertexCount, "vertices added", StopWatch());
                StopWatch();
                Assert.AreEqual(vertexCount, Count(graph.GetVertices()));
                PrintPerformance(graph.ToString(), vertexCount, "vertices Counted", StopWatch());
                // must create unique ids
                Assert.AreEqual(vertexCount, ids.Count());
            }
            graph.Shutdown();
        }

        [Test]
        public void TestLegalVertexEdgeIterables()
        {
            IGraph graph = GraphTest.GenerateGraph();
            IVertex v1 = graph.AddVertex(null);
            for (int i = 0; i < 10; i++)
                graph.AddEdge(null, v1, graph.AddVertex(null), ConvertId(graph, "knows"));

            IEnumerable<IEdge> edges = v1.GetEdges(Direction.Out, ConvertId(graph, "knows"));
            Assert.AreEqual(Count(edges), 10);
            Assert.AreEqual(Count(edges), 10);
            Assert.AreEqual(Count(edges), 10);
            graph.Shutdown();
        }

        [Test]
        public void TestAddVertexProperties()
        {
            var graph = GraphTest.GenerateGraph();

            if (graph.GetFeatures().SupportsVertexProperties)
            {
                var v1 = graph.AddVertex(ConvertId(graph, "1"));
                var v2 = graph.AddVertex(ConvertId(graph, "2"));

                if (graph.GetFeatures().SupportsStringProperty)
                {
                    v1.SetProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.GetProperty("key1"));
                }

                if (graph.GetFeatures().SupportsIntegerProperty)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);

                    Assert.AreEqual(10, v1.GetProperty("key2"));
                    Assert.AreEqual(20, v2.GetProperty("key2"));
                }

            }
            else if (graph.GetFeatures().IsRdfModel)
            {
                var v1 = graph.AddVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.GetProperty(SailTokens.Datatype));
                Assert.AreEqual(1, v1.GetProperty(SailTokens.Value));
                Assert.Null(v1.GetProperty(SailTokens.Language));
                Assert.Null(v1.GetProperty("random something"));

                var v2 = graph.AddVertex("\"hello\"@en");
                Assert.AreEqual("en", v2.GetProperty(SailTokens.Language));
                Assert.AreEqual("hello", v2.GetProperty(SailTokens.Value));
                Assert.Null(v2.GetProperty(SailTokens.Datatype));
                Assert.Null(v2.GetProperty("random something"));
            }
            graph.Shutdown();
        }

        [Test]
        public void TestAddManyVertexProperties()
        {
            var graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties && graph.GetFeatures().SupportsStringProperty)
            {
                var vertices = new HashSet<IVertex>();
                StopWatch();
                for (int i = 0; i < 50; i++)
                {
                    IVertex vertex = graph.AddVertex(null);
                    for (int j = 0; j < 15; j++)
                        vertex.SetProperty(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

                    vertices.Add(vertex);
                }
                PrintPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", StopWatch());

                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(50, Count(graph.GetVertices()));
                Assert.AreEqual(50, vertices.Count());
                foreach (IVertex vertex in vertices)
                    Assert.AreEqual(15, vertex.GetPropertyKeys().Count());

            }
            else if (graph.GetFeatures().IsRdfModel)
            {
                var vertices = new HashSet<IVertex>();
                StopWatch();
                for (int i = 0; i < 50; i++)
                {
                    IVertex vertex = graph.AddVertex(string.Concat("\"", Guid.NewGuid().ToString(), "\""));
                    for (int j = 0; j < 15; j++)
                        vertex.SetProperty(SailTokens.Datatype, "http://www.w3.org/2001/XMLSchema#anyURI");

                    vertices.Add(vertex);
                }
                PrintPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", StopWatch());
                if (graph.GetFeatures().SupportsVertexIteration)
                    Assert.AreEqual(Count(graph.GetVertices()), 50);
                Assert.AreEqual(vertices.Count(), 50);
                foreach (var vertex in vertices)
                {
                    Assert.AreEqual(3, vertex.GetPropertyKeys().Count());
                    Assert.True(vertex.GetPropertyKeys().Contains(SailTokens.Datatype));
                    Assert.AreEqual("http://www.w3.org/2001/XMLSchema#anyURI", vertex.GetProperty(SailTokens.Datatype));
                    Assert.True(vertex.GetPropertyKeys().Contains(SailTokens.Value));
                    Assert.AreEqual("literal", vertex.GetProperty(SailTokens.Kind));

                }
            }
            graph.Shutdown();
        }

        [Test]
        public void TestRemoveVertexProperties()
        {
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties)
            {

                IVertex v1 = graph.AddVertex("1");
                IVertex v2 = graph.AddVertex("2");

                Assert.Null(v1.RemoveProperty("key1"));
                Assert.Null(v1.RemoveProperty("key2"));
                Assert.Null(v2.RemoveProperty("key2"));

                if (graph.GetFeatures().SupportsStringProperty)
                {
                    v1.SetProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.RemoveProperty("key1"));
                }

                if (graph.GetFeatures().SupportsIntegerProperty)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);

                    Assert.AreEqual(10, v1.RemoveProperty("key2"));
                    Assert.AreEqual(20, v2.RemoveProperty("key2"));
                }

                Assert.Null(v1.RemoveProperty("key1"));
                Assert.Null(v1.RemoveProperty("key2"));
                Assert.Null(v2.RemoveProperty("key2"));

                if (graph.GetFeatures().SupportsStringProperty)
                    v1.SetProperty("key1", "value1");

                if (graph.GetFeatures().SupportsIntegerProperty)
                {
                    v1.SetProperty("key2", 10);
                    v2.SetProperty("key2", 20);
                }

                if (!graph.GetFeatures().IgnoresSuppliedIds)
                {
                    v1 = graph.GetVertex("1");
                    v2 = graph.GetVertex("2");

                    if (graph.GetFeatures().SupportsStringProperty)
                        Assert.AreEqual("value1", v1.RemoveProperty("key1"));

                    if (graph.GetFeatures().SupportsIntegerProperty)
                    {
                        Assert.AreEqual(10, v1.RemoveProperty("key2"));
                        Assert.AreEqual(20, v2.RemoveProperty("key2"));
                    }

                    Assert.Null(v1.RemoveProperty("key1"));
                    Assert.Null(v1.RemoveProperty("key2"));
                    Assert.Null(v2.RemoveProperty("key2"));

                    v1 = graph.GetVertex("1");
                    v2 = graph.GetVertex("2");

                    if (graph.GetFeatures().SupportsStringProperty)
                    {
                        v1.SetProperty("key1", "value2");
                        Assert.AreEqual("value2", v1.RemoveProperty("key1"));
                    }

                    if (graph.GetFeatures().SupportsIntegerProperty)
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
            else if (graph.GetFeatures().IsRdfModel)
            {
                IVertex v1 = graph.AddVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.RemoveProperty("type"));
                Assert.AreEqual("1", v1.GetProperty("value"));
                Assert.Null(v1.GetProperty("lang"));
                Assert.Null(v1.GetProperty("random something"));

                IVertex v2 = graph.AddVertex("\"hello\"@en");
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
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties)
            {
                IVertex vertex = graph.AddVertex(null);
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
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIteration)
            {
                for (int i = 0; i < 25; i++)
                    graph.AddVertex(null);

                Assert.AreEqual(Count(graph.GetVertices()), 25);
                foreach (IVertex vertex in graph.GetVertices())
                    graph.RemoveVertex(vertex);

                Assert.AreEqual(Count(graph.GetVertices()), 0);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestGettingEdgesAndVertices()
        {
            IGraph graph = GraphTest.GenerateGraph();
            IVertex a = graph.AddVertex(null);
            IVertex b = graph.AddVertex(null);
            IVertex c = graph.AddVertex(null);
            IEdge w = graph.AddEdge(null, a, b, ConvertId(graph, "knows"));
            IEdge x = graph.AddEdge(null, b, c, ConvertId(graph, "knows"));
            IEdge y = graph.AddEdge(null, a, c, ConvertId(graph, "hates"));
            IEdge z = graph.AddEdge(null, a, b, ConvertId(graph, "hates"));
            IEdge zz = graph.AddEdge(null, c, c, ConvertId(graph, "hates"));

            Assert.AreEqual(Count(a.GetEdges(Direction.Out)), 3);
            Assert.AreEqual(Count(a.GetEdges(Direction.Out, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetEdges(Direction.Out, ConvertId(graph, "knows"))), 1);
            Assert.AreEqual(Count(a.GetVertices(Direction.Out)), 3);
            Assert.AreEqual(Count(a.GetVertices(Direction.Out, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetVertices(Direction.Out, ConvertId(graph, "knows"))), 1);
            Assert.AreEqual(Count(a.GetVertices(Direction.Both)), 3);
            Assert.AreEqual(Count(a.GetVertices(Direction.Both, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(a.GetVertices(Direction.Both, ConvertId(graph, "knows"))), 1);

            Assert.True(a.GetEdges(Direction.Out).Contains(w));
            Assert.True(a.GetEdges(Direction.Out).Contains(y));
            Assert.True(a.GetEdges(Direction.Out).Contains(z));
            Assert.True(a.GetVertices(Direction.Out).Contains(b));
            Assert.True(a.GetVertices(Direction.Out).Contains(c));

            Assert.True(a.GetEdges(Direction.Out, ConvertId(graph, "knows")).Contains(w));
            Assert.False(a.GetEdges(Direction.Out, ConvertId(graph, "knows")).Contains(y));
            Assert.False(a.GetEdges(Direction.Out, ConvertId(graph, "knows")).Contains(z));
            Assert.True(a.GetVertices(Direction.Out, ConvertId(graph, "knows")).Contains(b));
            Assert.False(a.GetVertices(Direction.Out, ConvertId(graph, "knows")).Contains(c));

            Assert.False(a.GetEdges(Direction.Out, ConvertId(graph, "hates")).Contains(w));
            Assert.True(a.GetEdges(Direction.Out, ConvertId(graph, "hates")).Contains(y));
            Assert.True(a.GetEdges(Direction.Out, ConvertId(graph, "hates")).Contains(z));
            Assert.True(a.GetVertices(Direction.Out, ConvertId(graph, "hates")).Contains(b));
            Assert.True(a.GetVertices(Direction.Out, ConvertId(graph, "hates")).Contains(c));

            Assert.AreEqual(Count(a.GetVertices(Direction.In)), 0);
            Assert.AreEqual(Count(a.GetVertices(Direction.In, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(a.GetVertices(Direction.In, ConvertId(graph, "hates"))), 0);
            Assert.True(a.GetEdges(Direction.Out).Contains(w));
            Assert.True(a.GetEdges(Direction.Out).Contains(y));
            Assert.True(a.GetEdges(Direction.Out).Contains(z));

            Assert.AreEqual(Count(b.GetEdges(Direction.Both)), 3);
            Assert.AreEqual(Count(b.GetEdges(Direction.Both, ConvertId(graph, "knows"))), 2);
            Assert.True(b.GetEdges(Direction.Both, ConvertId(graph, "knows")).Contains(x));
            Assert.True(b.GetEdges(Direction.Both, ConvertId(graph, "knows")).Contains(w));
            Assert.True(b.GetVertices(Direction.Both, ConvertId(graph, "knows")).Contains(a));
            Assert.True(b.GetVertices(Direction.Both, ConvertId(graph, "knows")).Contains(c));

            Assert.AreEqual(Count(c.GetEdges(Direction.Both, ConvertId(graph, "hates"))), 3);
            Assert.AreEqual(Count(c.GetVertices(Direction.Both, ConvertId(graph, "hates"))), 3);
            Assert.AreEqual(Count(c.GetEdges(Direction.Both, ConvertId(graph, "knows"))), 1);
            Assert.True(c.GetEdges(Direction.Both, ConvertId(graph, "hates")).Contains(y));
            Assert.True(c.GetEdges(Direction.Both, ConvertId(graph, "hates")).Contains(zz));
            Assert.True(c.GetVertices(Direction.Both, ConvertId(graph, "hates")).Contains(a));
            Assert.True(c.GetVertices(Direction.Both, ConvertId(graph, "hates")).Contains(c));
            Assert.AreEqual(Count(c.GetEdges(Direction.In, ConvertId(graph, "hates"))), 2);
            Assert.AreEqual(Count(c.GetEdges(Direction.Out, ConvertId(graph, "hates"))), 1);

            try
            {
                x.GetVertex(Direction.Both);
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
            IGraph graph = GraphTest.GenerateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.GetFeatures().SupportsVertexProperties)
            {
                IVertex v = graph.AddVertex(null);
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
            IGraph graph = GraphTest.GenerateGraph();

            IVertex v = graph.AddVertex(null);
            IVertex a = graph.AddVertex(null);
            IVertex b = graph.AddVertex(null);

            v.AddEdge(ConvertId(graph, "knows"), a);
            v.AddEdge(ConvertId(graph, "knows"), b);

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 3);
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), 2);

            Assert.AreEqual(Count(v.GetEdges(Direction.Out, ConvertId(graph, "knows"))), 2);
            Assert.AreEqual(Count(a.GetEdges(Direction.Out, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(a.GetEdges(Direction.In, ConvertId(graph, "knows"))), 1);

            Assert.AreEqual(Count(b.GetEdges(Direction.Out, ConvertId(graph, "knows"))), 0);
            Assert.AreEqual(Count(b.GetEdges(Direction.In, ConvertId(graph, "knows"))), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestVertexCentricRemoving()
        {
            IGraph graph = GraphTest.GenerateGraph();

            IVertex a = graph.AddVertex(null);
            IVertex b = graph.AddVertex(null);
            IVertex c = graph.AddVertex(null);

            object cId = c.GetId();

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 3);

            a.Remove();
            b.Remove();

            Assert.NotNull(graph.GetVertex(cId));

            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), 1);

            graph.Shutdown();
        }

        [Test]
        public void TestConcurrentModificationOnProperties()
        {
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties)
            {
                IVertex a = graph.AddVertex(null);
                a.SetProperty("test1", 1);
                a.SetProperty("test2", 2);
                a.SetProperty("test3", 3);
                a.SetProperty("test4", 4);
                foreach (var key in a.GetPropertyKeys())
                    a.RemoveProperty(key);
            }
            graph.Shutdown();
        }

        [Test]
        public void TestSettingBadVertexProperties()
        {
            IGraph graph = GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexProperties)
            {
                IVertex v = graph.AddVertex(null);
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
                    v.SetProperty(StringFactory.Id, -1);
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
