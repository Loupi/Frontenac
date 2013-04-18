using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    [TestFixture(Category = "BatchGraphTest")]
    public class BatchGraphTest
    {
        const string _UID = "uid";

        const string vertexIdKey = "vid";
        const string edgeIdKey = "eid";
        bool _assignKeys = false;
        bool _ignoreIDs = false;

        [Test]
        public void testNumberIdLoading()
        {
            loadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            loadingTest(200000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());

            _assignKeys = true;
            loadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            loadingTest(50000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());
            _assignKeys = false;

            _ignoreIDs = true;
            loadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            loadingTest(50000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());
            _ignoreIDs = false;
        }

        [Test]
        public void testObjectIdLoading()
        {
            loadingTest(5000, 100, VertexIDType.OBJECT, new StringLoadingFactory());
            loadingTest(200000, 10000, VertexIDType.OBJECT, new StringLoadingFactory());
        }

        [Test]
        public void testStringIdLoading()
        {
            loadingTest(5000, 100, VertexIDType.STRING, new StringLoadingFactory());
            loadingTest(200000, 10000, VertexIDType.STRING, new StringLoadingFactory());
        }

        [Test]
        public void testUrlIdLoading()
        {
            loadingTest(5000, 100, VertexIDType.URL, new URLLoadingFactory());
            loadingTest(200000, 10000, VertexIDType.URL, new URLLoadingFactory());
        }

        [Test]
        public void testQuadLoading()
        {
            int numEdges = 10000;
            string[][] quads = generateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph graph = new TinkerGraph();
            BatchGraph bgraph = new BatchGraph(new WritethroughGraph(graph), VertexIDType.STRING, 1000);
            foreach (string[] quad in quads)
            {
                Vertex[] vertices = new Vertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = bgraph.getVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = bgraph.addVertex(quad[i]);
                }
                Edge edge = bgraph.addEdge(null, vertices[0], vertices[1], quad[2]);
                edge.setProperty("annotation", quad[3]);
            }
            Assert.AreEqual(numEdges, BaseTest.count(graph.getEdges()));

            bgraph.shutdown();
        }

        [Test]
        public void testLoadingWithExisting1()
        {
            int numEdges = 1000;
            string[][] quads = generateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph tg = new TinkerGraph();
            BatchGraph bg = new BatchGraph(new WritethroughGraph(tg), VertexIDType.STRING, 100);
            bg.setLoadingFromScratch(false);
            Graph graph = null;
            int counter = 0;
            foreach (string[] quad in quads)
            {
                if (counter < numEdges / 2) graph = tg;
                else graph = bg;

                Vertex[] vertices = new Vertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = graph.getVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.addVertex(quad[i]);
                }
                Edge edge = graph.addEdge(null, vertices[0], vertices[1], quad[2]);
                edge.setProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.count(tg.getEdges()));

            bg.shutdown();
        }

        [Test]
        public void testLoadingWithExisting2()
        {
            int numEdges = 1000;
            string[][] quads = generateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph tg = new IgnoreIdTinkerGraph();
            BatchGraph bg = new BatchGraph(new WritethroughGraph(tg), VertexIDType.STRING, 100);
            try
            {
                bg.setLoadingFromScratch(false);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
            bg.setVertexIdKey("uid");
            bg.setLoadingFromScratch(false);
            try
            {
                bg.setVertexIdKey(null);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            Graph graph = null;
            int counter = 0;
            foreach (string[] quad in quads)
            {
                if (counter < numEdges / 2) graph = tg;
                else graph = bg;

                Vertex[] vertices = new Vertex[2];
                for (int i = 0; i < 2; i++) {
                    vertices[i] = graph.getVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.addVertex(quad[i]);
                }
                Edge edge = graph.addEdge(null, vertices[0], vertices[1], quad[2]);
                edge.setProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.count(tg.getEdges()));

            bg.shutdown();
        }

        public static string[][] generateQuads(int numVertices, int numEdges, string[] labels)
        {
            Random random = new Random();
            string[][] edges = new string[numEdges][];
            for (int i = 0; i < numEdges; i++)
            {
                edges[i] = new string[4];
                edges[i][0] = string.Concat("v", random.Next(numVertices), 1);
                edges[i][1] = string.Concat("v", random.Next(numVertices), 1);
                edges[i][2] = labels[random.Next(labels.Length)];
                edges[i][3] = random.Next().ToString();
            }
            return edges;
        }

        public void loadingTest(int total, int bufferSize, VertexIDType type, LoadingFactory ids)
        {
            VertexEdgeCounter counter = new VertexEdgeCounter();

            MockTransactionalGraph tgraph = null;
            if (_ignoreIDs)
                tgraph = new MockTransactionalGraph(new IgnoreIdTinkerGraph());
            else
                tgraph = new MockTransactionalGraph(new TinkerGraph());

            BLGraph graph = new BLGraph(this, tgraph, counter, ids);
            BatchGraph loader = new BatchGraph(graph, type, bufferSize);

            if (_assignKeys)
                loader.setVertexIdKey(vertexIdKey);
                loader.setEdgeIdKey(edgeIdKey);

            //Create a chain
            int chainLength = total;
            Vertex previous = null;
            for (int i = 0; i <= chainLength; i++)
            {
                Vertex next = loader.addVertex(ids.getVertexId(i));
                next.setProperty(_UID, i);
                counter.numVertices++;
                counter.totalVertices++;
                if (previous != null)
                {
                    Edge e = loader.addEdge(ids.getEdgeId(i), loader.getVertex(previous.getId()), loader.getVertex(next.getId()), "next");
                    e.setProperty(_UID, i);
                    counter.numEdges++;
                }
                previous = next;
            }

            loader.commit();
            Assert.True(tgraph.allSuccessful());

            loader.shutdown();
        }

        class VertexEdgeCounter
        {
            public int numVertices = 0;
            public int numEdges = 0;
            public int totalVertices = 0;
        }

        class BLGraph : TransactionalGraph
        {
            const int keepLast = 10;

            readonly VertexEdgeCounter _counter;
            bool _first = true;
            readonly LoadingFactory _ids;

            readonly TransactionalGraph _graph;
            readonly BatchGraphTest _batchGraphTest;

            public BLGraph(BatchGraphTest batchGraphTest, TransactionalGraph graph, VertexEdgeCounter counter, LoadingFactory ids)
            {
                _batchGraphTest = batchGraphTest;
                _graph = graph;
                _counter = counter;
                _ids = ids;
            }

            static object parseId(object id)
            {
                if (id is string)
                {
                    try
                    {
                        return int.Parse((string) id);
                    }
                    catch (FormatException)
                    {
                        return id;
                    }
                }
                else 
                    return id;
            }

            public void commit()
            {
                _graph.commit();
                verifyCounts();
            }

            
            public void rollback()
            {
                _graph.rollback();
                verifyCounts();
            }

            void verifyCounts() 
            {
                //System.out.println("Committed (vertices/edges): " + counter.numVertices + " / " + counter.numEdges);
                Assert.AreEqual(_counter.numVertices, BaseTest.count(_graph.getVertices()) - (_first ? 0 : keepLast));
                Assert.AreEqual(_counter.numEdges, BaseTest.count(_graph.getEdges()));
                foreach (Edge e in getEdges())
                {
                    int id = (int)e.getProperty(_UID);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.getEdgeId(id), parseId(e.getId()));
                    
                    Assert.AreEqual(1, (int)e.getVertex(Direction.IN).getProperty(_UID) - (int)e.getVertex(Direction.OUT).getProperty(_UID));
                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.getEdgeId(id), e.getProperty(edgeIdKey));
                }
                foreach (Vertex v in getVertices())
                {
                    int id = (int)v.getProperty(_UID);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.getVertexId(id), parseId(v.getId()));
                    
                    Assert.True(2 >= BaseTest.count(v.getEdges(Direction.BOTH)));
                    Assert.True(1 >= BaseTest.count(v.getEdges(Direction.IN)));
                    Assert.True(1 >= BaseTest.count(v.getEdges(Direction.OUT)));

                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.getVertexId(id), v.getProperty(vertexIdKey));
                }
                foreach (Vertex v in getVertices())
                {
                    int id = (int)v.getProperty(_UID);
                    if (id < _counter.totalVertices - keepLast)
                        removeVertex(v);
                }
                foreach (Edge e in getEdges()) removeEdge(e);
                Assert.AreEqual(keepLast, BaseTest.count(_graph.getVertices()));
                _counter.numVertices = 0;
                _counter.numEdges = 0;
                _first = false;
                //System.out.println("------");
            }

            public Features getFeatures()
            {
                return _graph.getFeatures();
            }

            public Vertex addVertex(object id)
            {
                return _graph.addVertex(id);
            }

            public Vertex getVertex(object id)
            {
                return _graph.getVertex(id);
            }

            public void removeVertex(Vertex vertex)
            {
                _graph.removeVertex(vertex);
            }

            public IEnumerable<Vertex> getVertices()
            {
                return _graph.getVertices();
            }

            public IEnumerable<Vertex> getVertices(string key, object value)
            {
                return _graph.getVertices(key, value);
            }

            public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
            {
                return _graph.addEdge(id, outVertex, inVertex, label);
            }

            public Edge getEdge(object id)
            {
                return _graph.getEdge(id);
            }

            public void removeEdge(Edge edge)
            {
                _graph.removeEdge(edge);
            }

            public IEnumerable<Edge> getEdges()
            {
                return _graph.getEdges();
            }

            public IEnumerable<Edge> getEdges(string key, object value)
            {
                return _graph.getEdges(key, value);
            }

            public void shutdown()
            {
                _graph.shutdown();
            }

            public GraphQuery query()
            {
                return _graph.query();
            }
        }

        public interface LoadingFactory
        {
            object getVertexId(int id);

            object getEdgeId(int id);
        }

        class StringLoadingFactory : LoadingFactory
        {
            public object getVertexId(int id)
            {
                return string.Concat("V", id);
            }

            public object getEdgeId(int id)
            {
                return string.Concat("E", id);
            }
        }

        class NumberLoadingFactory : LoadingFactory
        {
            public object getVertexId(int id)
            {
                return id * 2;
            }

            public object getEdgeId(int id)
            {
                return id * 2 + 1;
            }
        }

        class URLLoadingFactory : LoadingFactory
        {
            public object getVertexId(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/vertex/", + id);
            }

            public object getEdgeId(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/edge#", id);
            }
        }
    }
}
