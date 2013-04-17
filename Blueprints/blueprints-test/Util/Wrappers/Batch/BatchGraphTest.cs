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

        const string _VertexIDKey = "vid";
        const string _EdgeIDKey = "eid";
        bool _AssignKeys = false;
        bool _IgnoreIDs = false;

        [Test]
        public void TestNumberIdLoading()
        {
            LoadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            LoadingTest(200000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());

            _AssignKeys = true;
            LoadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            LoadingTest(50000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());
            _AssignKeys = false;

            _IgnoreIDs = true;
            LoadingTest(5000, 100, VertexIDType.NUMBER, new NumberLoadingFactory());
            LoadingTest(50000, 10000, VertexIDType.NUMBER, new NumberLoadingFactory());
            _IgnoreIDs = false;
        }

        [Test]
        public void TestObjectIdLoading()
        {
            LoadingTest(5000, 100, VertexIDType.OBJECT, new StringLoadingFactory());
            LoadingTest(200000, 10000, VertexIDType.OBJECT, new StringLoadingFactory());
        }

        [Test]
        public void TestStringIdLoading()
        {
            LoadingTest(5000, 100, VertexIDType.STRING, new StringLoadingFactory());
            LoadingTest(200000, 10000, VertexIDType.STRING, new StringLoadingFactory());
        }

        [Test]
        public void TestURLIdLoading()
        {
            LoadingTest(5000, 100, VertexIDType.URL, new URLLoadingFactory());
            LoadingTest(200000, 10000, VertexIDType.URL, new URLLoadingFactory());
        }

        [Test]
        public void TestQuadLoading()
        {
            int numEdges = 10000;
            string[][] quads = GenerateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph graph = new TinkerGraph();
            BatchGraph bgraph = new BatchGraph(new WritethroughGraph(graph), VertexIDType.STRING, 1000);
            foreach (string[] quad in quads)
            {
                Vertex[] vertices = new Vertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = bgraph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = bgraph.AddVertex(quad[i]);
                }
                Edge edge = bgraph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
            }
            Assert.AreEqual(numEdges, BaseTest.Count(graph.GetEdges()));

            bgraph.Shutdown();
        }

        [Test]
        public void TestLoadingWithExisting1()
        {
            int numEdges = 1000;
            string[][] quads = GenerateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph tg = new TinkerGraph();
            BatchGraph bg = new BatchGraph(new WritethroughGraph(tg), VertexIDType.STRING, 100);
            bg.SetLoadingFromScratch(false);
            Graph graph = null;
            int counter = 0;
            foreach (string[] quad in quads)
            {
                if (counter < numEdges / 2) graph = tg;
                else graph = bg;

                Vertex[] vertices = new Vertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = graph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                }
                Edge edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));

            bg.Shutdown();
        }

        [Test]
        public void TestLoadingWithExisting2()
        {
            int numEdges = 1000;
            string[][] quads = GenerateQuads(100, numEdges, new string[]{"knows", "friend"});
            TinkerGraph tg = new IgnoreIdTinkerGraph();
            BatchGraph bg = new BatchGraph(new WritethroughGraph(tg), VertexIDType.STRING, 100);
            try
            {
                bg.SetLoadingFromScratch(false);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
            bg.SetVertexIdKey("uid");
            bg.SetLoadingFromScratch(false);
            try
            {
                bg.SetVertexIdKey(null);
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
                    vertices[i] = graph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                }
                Edge edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));

            bg.Shutdown();
        }

        public static string[][] GenerateQuads(int numVertices, int numEdges, string[] labels)
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

        public void LoadingTest(int total, int bufferSize, VertexIDType type, LoadingFactory ids)
        {
            VertexEdgeCounter counter = new VertexEdgeCounter();

            MockTransactionalGraph tgraph = null;
            if (_IgnoreIDs)
                tgraph = new MockTransactionalGraph(new IgnoreIdTinkerGraph());
            else
                tgraph = new MockTransactionalGraph(new TinkerGraph());

            BLGraph graph = new BLGraph(this, tgraph, counter, ids);
            BatchGraph loader = new BatchGraph(graph, type, bufferSize);

            if (_AssignKeys)
                loader.SetVertexIdKey(_VertexIDKey);
                loader.SetEdgeIdKey(_EdgeIDKey);

            //Create a chain
            int chainLength = total;
            Vertex previous = null;
            for (int i = 0; i <= chainLength; i++)
            {
                Vertex next = loader.AddVertex(ids.GetVertexID(i));
                next.SetProperty(_UID, i);
                counter._NumVertices++;
                counter._TotalVertices++;
                if (previous != null)
                {
                    Edge e = loader.AddEdge(ids.GetEdgeID(i), loader.GetVertex(previous.GetId()), loader.GetVertex(next.GetId()), "next");
                    e.SetProperty(_UID, i);
                    counter._NumEdges++;
                }
                previous = next;
            }

            loader.Commit();
            Assert.True(tgraph.AllSuccessful());

            loader.Shutdown();
        }

        class VertexEdgeCounter
        {
            public int _NumVertices = 0;
            public int _NumEdges = 0;
            public int _TotalVertices = 0;
        }

        class BLGraph : TransactionalGraph
        {
            const int keepLast = 10;

            readonly VertexEdgeCounter _Counter;
            bool _First = true;
            readonly LoadingFactory _Ids;

            readonly TransactionalGraph _Graph;
            readonly BatchGraphTest _BatchGraphTest;

            public BLGraph(BatchGraphTest batchGraphTest, TransactionalGraph graph, VertexEdgeCounter counter, LoadingFactory ids)
            {
                _BatchGraphTest = batchGraphTest;
                _Graph = graph;
                _Counter = counter;
                _Ids = ids;
            }

            static object ParseID(object id)
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

            public void Commit()
            {
                _Graph.Commit();
                VerifyCounts();
            }

            
            public void Rollback()
            {
                _Graph.Rollback();
                VerifyCounts();
            }

            void VerifyCounts() 
            {
                //System.out.println("Committed (vertices/edges): " + counter.numVertices + " / " + counter.numEdges);
                Assert.AreEqual(_Counter._NumVertices, BaseTest.Count(_Graph.GetVertices()) - (_First ? 0 : keepLast));
                Assert.AreEqual(_Counter._NumEdges, BaseTest.Count(_Graph.GetEdges()));
                foreach (Edge e in GetEdges())
                {
                    int id = (int)e.GetProperty(_UID);
                    if (!_BatchGraphTest._IgnoreIDs)
                        Assert.AreEqual(_Ids.GetEdgeID(id), ParseID(e.GetId()));
                    
                    Assert.AreEqual(1, (int)e.GetVertex(Direction.IN).GetProperty(_UID) - (int)e.GetVertex(Direction.OUT).GetProperty(_UID));
                    if (_BatchGraphTest._AssignKeys)
                        Assert.AreEqual(_Ids.GetEdgeID(id), e.GetProperty(_EdgeIDKey));
                }
                foreach (Vertex v in GetVertices())
                {
                    int id = (int)v.GetProperty(_UID);
                    if (!_BatchGraphTest._IgnoreIDs)
                        Assert.AreEqual(_Ids.GetVertexID(id), ParseID(v.GetId()));
                    
                    Assert.True(2 >= BaseTest.Count(v.GetEdges(Direction.BOTH)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.IN)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.OUT)));

                    if (_BatchGraphTest._AssignKeys)
                        Assert.AreEqual(_Ids.GetVertexID(id), v.GetProperty(_VertexIDKey));
                }
                foreach (Vertex v in GetVertices())
                {
                    int id = (int)v.GetProperty(_UID);
                    if (id < _Counter._TotalVertices - keepLast)
                        RemoveVertex(v);
                }
                foreach (Edge e in GetEdges()) RemoveEdge(e);
                Assert.AreEqual(keepLast, BaseTest.Count(_Graph.GetVertices()));
                _Counter._NumVertices = 0;
                _Counter._NumEdges = 0;
                _First = false;
                //System.out.println("------");
            }

            public Features GetFeatures()
            {
                return _Graph.GetFeatures();
            }

            public Vertex AddVertex(object id)
            {
                return _Graph.AddVertex(id);
            }

            public Vertex GetVertex(object id)
            {
                return _Graph.GetVertex(id);
            }

            public void RemoveVertex(Vertex vertex)
            {
                _Graph.RemoveVertex(vertex);
            }

            public IEnumerable<Vertex> GetVertices()
            {
                return _Graph.GetVertices();
            }

            public IEnumerable<Vertex> GetVertices(string key, object value)
            {
                return _Graph.GetVertices(key, value);
            }

            public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
            {
                return _Graph.AddEdge(id, outVertex, inVertex, label);
            }

            public Edge GetEdge(object id)
            {
                return _Graph.GetEdge(id);
            }

            public void RemoveEdge(Edge edge)
            {
                _Graph.RemoveEdge(edge);
            }

            public IEnumerable<Edge> GetEdges()
            {
                return _Graph.GetEdges();
            }

            public IEnumerable<Edge> GetEdges(string key, object value)
            {
                return _Graph.GetEdges(key, value);
            }

            public void Shutdown()
            {
                _Graph.Shutdown();
            }

            public GraphQuery Query()
            {
                return _Graph.Query();
            }
        }

        public interface LoadingFactory
        {
            object GetVertexID(int id);

            object GetEdgeID(int id);
        }

        class StringLoadingFactory : LoadingFactory
        {
            public object GetVertexID(int id)
            {
                return string.Concat("V", id);
            }

            public object GetEdgeID(int id)
            {
                return string.Concat("E", id);
            }
        }

        class NumberLoadingFactory : LoadingFactory
        {
            public object GetVertexID(int id)
            {
                return id * 2;
            }

            public object GetEdgeID(int id)
            {
                return id * 2 + 1;
            }
        }

        class URLLoadingFactory : LoadingFactory
        {
            public object GetVertexID(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/vertex/", + id);
            }

            public object GetEdgeID(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/edge#", id);
            }
        }
    }
}
