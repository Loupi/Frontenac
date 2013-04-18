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
        public void testVertexEquality()
        {
            Vertex v;
            Vertex u;
            Graph graph = graphTest.generateGraph();

            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                v = graph.addVertex(convertId(graph, "1"));
                u = graph.getVertex(convertId(graph, "1"));
                Assert.AreEqual(v, u);
            }

            this.stopWatch();
            v = graph.addVertex(null);
            Assert.False(v == null);
            u = graph.getVertex(v.getId());
            Assert.AreEqual(v, u);
            printPerformance(graph.ToString(), 1, "vertex added and retrieved", this.stopWatch());

            Assert.AreEqual(graph.getVertex(u.getId()), graph.getVertex(u.getId()));
            Assert.AreEqual(graph.getVertex(v.getId()), graph.getVertex(u.getId()));
            Assert.AreEqual(graph.getVertex(v.getId()), graph.getVertex(v.getId()));

            graph.shutdown();
        }

        [Test]
        public void testVertexEqualityForSuppliedIdsAndHashCode()
        {
            Graph graph = graphTest.generateGraph();
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
            {
                Vertex v = graph.addVertex(convertId(graph, "1"));
                Vertex u = graph.getVertex(convertId(graph, "1"));
                HashSet<Vertex> set = new HashSet<Vertex>();
                set.Add(v);
                set.Add(v);
                set.Add(u);
                set.Add(u);
                set.Add(graph.getVertex(convertId(graph, "1")));
                set.Add(graph.getVertex(convertId(graph, "1")));
                if (graph.getFeatures().supportsVertexIndex.Value)
                    set.Add(graph.getVertices().First());
                Assert.AreEqual(1, set.Count());
                Assert.AreEqual(v.GetHashCode(), u.GetHashCode());
            }
            graph.shutdown();
        }

        [Test]
        public void testBasicAddVertex()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                graph.addVertex(convertId(graph, "1"));
                graph.addVertex(convertId(graph, "2"));
                Assert.AreEqual(2, count(graph.getVertices()));
                graph.addVertex(convertId(graph, "3"));
                Assert.AreEqual(3, count(graph.getVertices()));
            }

            if (graph.getFeatures().isRdfModel.Value)
            {
                Vertex v1 = graph.addVertex("http://tinkerpop.com#marko");
                Assert.AreEqual("http://tinkerpop.com#marko", v1.getId());
                Vertex v2 = graph.addVertex("\"1\"^^<datatype:int>");
                Assert.AreEqual("\"1\"^^<datatype:int>", v2.getId());
                Vertex v3 = graph.addVertex("_:ABLANKNODE");
                Assert.AreEqual(v3.getId(), "_:ABLANKNODE");
                Vertex v4 = graph.addVertex("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>");
                Assert.AreEqual("\"2.24\"^^<http://www.w3.org/2001/XMLSchema#double>", v4.getId());
            }
            graph.shutdown();
        }

        [Test]
        public void testGetVertexWithNull()
        {
            Graph graph = graphTest.generateGraph();
            try
            {
                graph.getVertex(null);
                Assert.False(true);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
            graph.shutdown();
        }

        [Test]
        public void testRemoveVertex()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            if (!graph.getFeatures().ignoresSuppliedIds.Value)
                Assert.AreEqual(graph.getVertex(convertId(graph, "1")), v1);

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(1, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            graph.removeVertex(v1);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            HashSet<Vertex> vertices = new HashSet<Vertex>();
            for (int i = 0; i < 100; i++)
                vertices.Add(graph.addVertex(null));

            Assert.AreEqual(vertices.Count(), 100);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(100, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            foreach (Vertex v in vertices)
                graph.removeVertex(v);

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            graph.shutdown();
        }

        [Test]
        public void testRemoveVertexWithEdges()
        {
            Graph graph = graphTest.generateGraph();
            Vertex v1 = graph.addVertex(convertId(graph, "1"));
            Vertex v2 = graph.addVertex(convertId(graph, "2"));
            graph.addEdge(null, v1, v2, convertId(graph, "knows"));
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(2, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(1, count(graph.getEdges()));

            graph.removeVertex(v1);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(1, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));

            graph.removeVertex(v2);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(0, count(graph.getEdges()));
            graph.shutdown();
        }

        [Test]
        public void testGetNonExistantVertices()
        {
            Graph graph = graphTest.generateGraph();
            Assert.Null(graph.getVertex("asbv"));
            Assert.Null(graph.getVertex(12.0d));
            graph.shutdown();
        }

        [Test]
        public void testRemoveVertexNullId()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v1 = graph.addVertex(null);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(1, count(graph.getVertices()));
            graph.removeVertex(v1);
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));

            HashSet<Vertex> vertices = new HashSet<Vertex>();

            this.stopWatch();
            int vertexCount = 100;
            for (int i = 0; i < vertexCount; i++)
                vertices.Add(graph.addVertex(null));

            printPerformance(graph.ToString(), vertexCount, "vertices added", this.stopWatch());
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(vertexCount, count(graph.getVertices()));

            this.stopWatch();
            foreach (Vertex v in vertices)
                graph.removeVertex(v);

            printPerformance(graph.ToString(), vertexCount, "vertices removed", this.stopWatch());
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(0, count(graph.getVertices()));
            graph.shutdown();
        }

        [Test]
        public void testVertexIterator()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                this.stopWatch();
                int vertexCount = 1000;
                HashSet<object> ids = new HashSet<object>();
                for (int i = 0; i < vertexCount; i++)
                    ids.Add(graph.addVertex(null).getId());

                printPerformance(graph.ToString(), vertexCount, "vertices added", this.stopWatch());
                this.stopWatch();
                Assert.AreEqual(vertexCount, count(graph.getVertices()));
                printPerformance(graph.ToString(), vertexCount, "vertices Counted", this.stopWatch());
                // must create unique ids
                Assert.AreEqual(vertexCount, ids.Count());
            }
            graph.shutdown();
        }

        [Test]
        public void testLegalVertexEdgeIterables()
        {
            Graph graph = graphTest.generateGraph();
            Vertex v1 = graph.addVertex(null);
            for (int i = 0; i < 10; i++)
                graph.addEdge(null, v1, graph.addVertex(null), convertId(graph, "knows"));

            IEnumerable<Edge> edges = v1.getEdges(Direction.OUT, convertId(graph, "knows"));
            Assert.AreEqual(count(edges), 10);
            Assert.AreEqual(count(edges), 10);
            Assert.AreEqual(count(edges), 10);
            graph.shutdown();
        }

        [Test]
        public void testAddVertexProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v1 = graph.addVertex(convertId(graph, "1"));
                Vertex v2 = graph.addVertex(convertId(graph, "2"));

                if (graph.getFeatures().supportsStringProperty.Value)
                {
                    v1.setProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.getProperty("key1"));
                }

                if (graph.getFeatures().supportsIntegerProperty.Value)
                {
                    v1.setProperty("key2", 10);
                    v2.setProperty("key2", 20);

                    Assert.AreEqual(10, v1.getProperty("key2"));
                    Assert.AreEqual(20, v2.getProperty("key2"));
                }

            }
            else if (graph.getFeatures().isRdfModel.Value)
            {
                Vertex v1 = graph.addVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.getProperty(SailTokens.DATATYPE));
                Assert.AreEqual(1, v1.getProperty(SailTokens.VALUE));
                Assert.Null(v1.getProperty(SailTokens.LANGUAGE));
                Assert.Null(v1.getProperty("random something"));

                Vertex v2 = graph.addVertex("\"hello\"@en");
                Assert.AreEqual("en", v2.getProperty(SailTokens.LANGUAGE));
                Assert.AreEqual("hello", v2.getProperty(SailTokens.VALUE));
                Assert.Null(v2.getProperty(SailTokens.DATATYPE));
                Assert.Null(v2.getProperty("random something"));
            }
            graph.shutdown();
        }

        [Test]
        public void testAddManyVertexProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value && graph.getFeatures().supportsStringProperty.Value)
            {
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                this.stopWatch();
                for (int i = 0; i < 50; i++)
                {
                    Vertex vertex = graph.addVertex(null);
                    for (int j = 0; j < 15; j++)
                        vertex.setProperty(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

                    vertices.Add(vertex);
                }
                printPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", this.stopWatch());

                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(50, count(graph.getVertices()));
                Assert.AreEqual(50, vertices.Count());
                foreach (Vertex vertex in vertices)
                    Assert.AreEqual(15, vertex.getPropertyKeys().Count());

            }
            else if (graph.getFeatures().isRdfModel.Value)
            {
                HashSet<Vertex> vertices = new HashSet<Vertex>();
                this.stopWatch();
                for (int i = 0; i < 50; i++)
                {
                    Vertex vertex = graph.addVertex(string.Concat("\"", Guid.NewGuid().ToString(), "\""));
                    for (int j = 0; j < 15; j++)
                        vertex.setProperty(SailTokens.DATATYPE, "http://www.w3.org/2001/XMLSchema#anyURI");

                    vertices.Add(vertex);
                }
                printPerformance(graph.ToString(), 15 * 50, "vertex properties added (with vertices being added too)", this.stopWatch());
                if (graph.getFeatures().supportsVertexIteration.Value)
                    Assert.AreEqual(count(graph.getVertices()), 50);
                Assert.AreEqual(vertices.Count(), 50);
                foreach (Vertex vertex in vertices)
                {
                    Assert.AreEqual(3, vertex.getPropertyKeys().Count());
                    Assert.True(vertex.getPropertyKeys().Contains(SailTokens.DATATYPE));
                    Assert.AreEqual("http://www.w3.org/2001/XMLSchema#anyURI", vertex.getProperty(SailTokens.DATATYPE));
                    Assert.True(vertex.getPropertyKeys().Contains(SailTokens.VALUE));
                    Assert.AreEqual("literal", vertex.getProperty(SailTokens.KIND));

                }
            }
            graph.shutdown();
        }

        [Test]
        public void testRemoveVertexProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {

                Vertex v1 = graph.addVertex("1");
                Vertex v2 = graph.addVertex("2");

                Assert.Null(v1.removeProperty("key1"));
                Assert.Null(v1.removeProperty("key2"));
                Assert.Null(v2.removeProperty("key2"));

                if (graph.getFeatures().supportsStringProperty.Value)
                {
                    v1.setProperty("key1", "value1");
                    Assert.AreEqual("value1", v1.removeProperty("key1"));
                }

                if (graph.getFeatures().supportsIntegerProperty.Value)
                {
                    v1.setProperty("key2", 10);
                    v2.setProperty("key2", 20);

                    Assert.AreEqual(10, v1.removeProperty("key2"));
                    Assert.AreEqual(20, v2.removeProperty("key2"));
                }

                Assert.Null(v1.removeProperty("key1"));
                Assert.Null(v1.removeProperty("key2"));
                Assert.Null(v2.removeProperty("key2"));

                if (graph.getFeatures().supportsStringProperty.Value)
                    v1.setProperty("key1", "value1");

                if (graph.getFeatures().supportsIntegerProperty.Value)
                {
                    v1.setProperty("key2", 10);
                    v2.setProperty("key2", 20);
                }

                if (!graph.getFeatures().ignoresSuppliedIds.Value)
                {
                    v1 = graph.getVertex("1");
                    v2 = graph.getVertex("2");

                    if (graph.getFeatures().supportsStringProperty.Value)
                        Assert.AreEqual("value1", v1.removeProperty("key1"));

                    if (graph.getFeatures().supportsIntegerProperty.Value)
                    {
                        Assert.AreEqual(10, v1.removeProperty("key2"));
                        Assert.AreEqual(20, v2.removeProperty("key2"));
                    }

                    Assert.Null(v1.removeProperty("key1"));
                    Assert.Null(v1.removeProperty("key2"));
                    Assert.Null(v2.removeProperty("key2"));

                    v1 = graph.getVertex("1");
                    v2 = graph.getVertex("2");

                    if (graph.getFeatures().supportsStringProperty.Value)
                    {
                        v1.setProperty("key1", "value2");
                        Assert.AreEqual("value2", v1.removeProperty("key1"));
                    }

                    if (graph.getFeatures().supportsIntegerProperty.Value)
                    {
                        v1.setProperty("key2", 20);
                        v2.setProperty("key2", 30);

                        Assert.AreEqual(20, v1.removeProperty("key2"));
                        Assert.AreEqual(30, v2.removeProperty("key2"));
                    }

                    Assert.Null(v1.removeProperty("key1"));
                    Assert.Null(v1.removeProperty("key2"));
                    Assert.Null(v2.removeProperty("key2"));
                }
            }
            else if (graph.getFeatures().isRdfModel.Value)
            {
                Vertex v1 = graph.addVertex("\"1\"^^<http://www.w3.org/2001/XMLSchema#int>");
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#int", v1.removeProperty("type"));
                Assert.AreEqual("1", v1.getProperty("value"));
                Assert.Null(v1.getProperty("lang"));
                Assert.Null(v1.getProperty("random something"));

                Vertex v2 = graph.addVertex("\"hello\"@en");
                Assert.AreEqual("en", v2.removeProperty("lang"));
                Assert.AreEqual("hello", v2.getProperty("value"));
                Assert.Null(v2.getProperty("type"));
                Assert.Null(v2.getProperty("random something"));
            }
            graph.shutdown();
        }

        [Test]
        public void testAddingIdProperty()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex vertex = graph.addVertex(null);
                try
                {
                    vertex.setProperty("id", "123");
                    Assert.True(false);
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testNoConcurrentModificationException()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIteration.Value)
            {
                for (int i = 0; i < 25; i++)
                    graph.addVertex(null);

                Assert.AreEqual(count(graph.getVertices()), 25);
                foreach (Vertex vertex in graph.getVertices())
                    graph.removeVertex(vertex);

                Assert.AreEqual(count(graph.getVertices()), 0);
            }
            graph.shutdown();
        }

        [Test]
        public void testGettingEdgesAndVertices()
        {
            Graph graph = graphTest.generateGraph();
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Vertex c = graph.addVertex(null);
            Edge w = graph.addEdge(null, a, b, convertId(graph, "knows"));
            Edge x = graph.addEdge(null, b, c, convertId(graph, "knows"));
            Edge y = graph.addEdge(null, a, c, convertId(graph, "hates"));
            Edge z = graph.addEdge(null, a, b, convertId(graph, "hates"));
            Edge zz = graph.addEdge(null, c, c, convertId(graph, "hates"));

            Assert.AreEqual(count(a.getEdges(Direction.OUT)), 3);
            Assert.AreEqual(count(a.getEdges(Direction.OUT, convertId(graph, "hates"))), 2);
            Assert.AreEqual(count(a.getEdges(Direction.OUT, convertId(graph, "knows"))), 1);
            Assert.AreEqual(count(a.getVertices(Direction.OUT)), 3);
            Assert.AreEqual(count(a.getVertices(Direction.OUT, convertId(graph, "hates"))), 2);
            Assert.AreEqual(count(a.getVertices(Direction.OUT, convertId(graph, "knows"))), 1);
            Assert.AreEqual(count(a.getVertices(Direction.BOTH)), 3);
            Assert.AreEqual(count(a.getVertices(Direction.BOTH, convertId(graph, "hates"))), 2);
            Assert.AreEqual(count(a.getVertices(Direction.BOTH, convertId(graph, "knows"))), 1);

            Assert.True(a.getEdges(Direction.OUT).Contains(w));
            Assert.True(a.getEdges(Direction.OUT).Contains(y));
            Assert.True(a.getEdges(Direction.OUT).Contains(z));
            Assert.True(a.getVertices(Direction.OUT).Contains(b));
            Assert.True(a.getVertices(Direction.OUT).Contains(c));

            Assert.True(a.getEdges(Direction.OUT, convertId(graph, "knows")).Contains(w));
            Assert.False(a.getEdges(Direction.OUT, convertId(graph, "knows")).Contains(y));
            Assert.False(a.getEdges(Direction.OUT, convertId(graph, "knows")).Contains(z));
            Assert.True(a.getVertices(Direction.OUT, convertId(graph, "knows")).Contains(b));
            Assert.False(a.getVertices(Direction.OUT, convertId(graph, "knows")).Contains(c));

            Assert.False(a.getEdges(Direction.OUT, convertId(graph, "hates")).Contains(w));
            Assert.True(a.getEdges(Direction.OUT, convertId(graph, "hates")).Contains(y));
            Assert.True(a.getEdges(Direction.OUT, convertId(graph, "hates")).Contains(z));
            Assert.True(a.getVertices(Direction.OUT, convertId(graph, "hates")).Contains(b));
            Assert.True(a.getVertices(Direction.OUT, convertId(graph, "hates")).Contains(c));

            Assert.AreEqual(count(a.getVertices(Direction.IN)), 0);
            Assert.AreEqual(count(a.getVertices(Direction.IN, convertId(graph, "knows"))), 0);
            Assert.AreEqual(count(a.getVertices(Direction.IN, convertId(graph, "hates"))), 0);
            Assert.True(a.getEdges(Direction.OUT).Contains(w));
            Assert.True(a.getEdges(Direction.OUT).Contains(y));
            Assert.True(a.getEdges(Direction.OUT).Contains(z));

            Assert.AreEqual(count(b.getEdges(Direction.BOTH)), 3);
            Assert.AreEqual(count(b.getEdges(Direction.BOTH, convertId(graph, "knows"))), 2);
            Assert.True(b.getEdges(Direction.BOTH, convertId(graph, "knows")).Contains(x));
            Assert.True(b.getEdges(Direction.BOTH, convertId(graph, "knows")).Contains(w));
            Assert.True(b.getVertices(Direction.BOTH, convertId(graph, "knows")).Contains(a));
            Assert.True(b.getVertices(Direction.BOTH, convertId(graph, "knows")).Contains(c));

            Assert.AreEqual(count(c.getEdges(Direction.BOTH, convertId(graph, "hates"))), 3);
            Assert.AreEqual(count(c.getVertices(Direction.BOTH, convertId(graph, "hates"))), 3);
            Assert.AreEqual(count(c.getEdges(Direction.BOTH, convertId(graph, "knows"))), 1);
            Assert.True(c.getEdges(Direction.BOTH, convertId(graph, "hates")).Contains(y));
            Assert.True(c.getEdges(Direction.BOTH, convertId(graph, "hates")).Contains(zz));
            Assert.True(c.getVertices(Direction.BOTH, convertId(graph, "hates")).Contains(a));
            Assert.True(c.getVertices(Direction.BOTH, convertId(graph, "hates")).Contains(c));
            Assert.AreEqual(count(c.getEdges(Direction.IN, convertId(graph, "hates"))), 2);
            Assert.AreEqual(count(c.getEdges(Direction.OUT, convertId(graph, "hates"))), 1);

            try
            {
                x.getVertex(Direction.BOTH);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }

            graph.shutdown();
        }

        [Test]
        public void testEmptyKeyProperty()
        {
            Graph graph = graphTest.generateGraph();

            // no point in testing graph features for setting string properties because the intent is for it to
            // fail based on the empty key.
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v = graph.addVertex(null);
                try
                {
                    v.setProperty("", "value");
                    Assert.Fail();
                }
                catch (ArgumentException)
                {
                    Assert.True(true);
                }
            }
            graph.shutdown();
        }

        [Test]
        public void testVertexCentricLinking()
        {
            Graph graph = graphTest.generateGraph();

            Vertex v = graph.addVertex(null);
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);

            v.addEdge(convertId(graph, "knows"), a);
            v.addEdge(convertId(graph, "knows"), b);

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 3);
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), 2);

            Assert.AreEqual(count(v.getEdges(Direction.OUT, convertId(graph, "knows"))), 2);
            Assert.AreEqual(count(a.getEdges(Direction.OUT, convertId(graph, "knows"))), 0);
            Assert.AreEqual(count(a.getEdges(Direction.IN, convertId(graph, "knows"))), 1);

            Assert.AreEqual(count(b.getEdges(Direction.OUT, convertId(graph, "knows"))), 0);
            Assert.AreEqual(count(b.getEdges(Direction.IN, convertId(graph, "knows"))), 1);

            graph.shutdown();
        }

        [Test]
        public void testVertexCentricRemoving()
        {
            Graph graph = graphTest.generateGraph();

            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Vertex c = graph.addVertex(null);

            object cId = c.getId();

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 3);

            a.remove();
            b.remove();

            Assert.NotNull(graph.getVertex(cId));

            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), 1);

            graph.shutdown();
        }

        [Test]
        public void testConcurrentModificationOnProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex a = graph.addVertex(null);
                a.setProperty("test1", 1);
                a.setProperty("test2", 2);
                a.setProperty("test3", 3);
                a.setProperty("test4", 4);
                foreach (string key in a.getPropertyKeys())
                    a.removeProperty(key);
            }
            graph.shutdown();
        }

        [Test]
        public void testSettingBadVertexProperties()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex v = graph.addVertex(null);
                try
                {
                    v.setProperty(null, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.setProperty("", -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.setProperty(StringFactory.ID, -1);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }

                try
                {
                    v.setProperty(convertId(graph, "good"), null);
                    Assert.False(true);
                }
                catch (Exception)
                {
                    Assert.True(true);
                }
            }
            graph.shutdown();
        }
    }
}
