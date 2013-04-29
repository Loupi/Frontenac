using System.Globalization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    [TestFixture(Category = "BatchGraphTest")]
    public class BatchGraphTest
    {
        const string Uid = "uid";

        const string VertexIdKey = "vid";
        const string EdgeIdKey = "eid";
        bool _assignKeys;
        bool _ignoreIDs;

        [Test]
        public void TestNumberIdLoading()
        {
            LoadingTest(5000, 100, VertexIdType.Number, new NumberLoadingFactory());
            LoadingTest(200000, 10000, VertexIdType.Number, new NumberLoadingFactory());

            _assignKeys = true;
            LoadingTest(5000, 100, VertexIdType.Number, new NumberLoadingFactory());
            LoadingTest(50000, 10000, VertexIdType.Number, new NumberLoadingFactory());
            _assignKeys = false;

            _ignoreIDs = true;
            LoadingTest(5000, 100, VertexIdType.Number, new NumberLoadingFactory());
            LoadingTest(50000, 10000, VertexIdType.Number, new NumberLoadingFactory());
            _ignoreIDs = false;
        }

        [Test]
        public void TestObjectIdLoading()
        {
            LoadingTest(5000, 100, VertexIdType.Object, new StringLoadingFactory());
            LoadingTest(200000, 10000, VertexIdType.Object, new StringLoadingFactory());
        }

        [Test]
        public void TestStringIdLoading()
        {
            LoadingTest(5000, 100, VertexIdType.String, new StringLoadingFactory());
            LoadingTest(200000, 10000, VertexIdType.String, new StringLoadingFactory());
        }

        [Test]
        public void TestUrlIdLoading()
        {
            LoadingTest(5000, 100, VertexIdType.Url, new UrlLoadingFactory());
            LoadingTest(200000, 10000, VertexIdType.Url, new UrlLoadingFactory());
        }

        [Test]
        public void TestQuadLoading()
        {
            const int numEdges = 10000;
            string[][] quads = GenerateQuads(100, numEdges, new[]{"knows", "friend"});
            var graph = new TinkerGraph();
            var bgraph = new BatchGraph(new WritethroughGraph(graph), VertexIdType.String, 1000);
            foreach (string[] quad in quads)
            {
                var vertices = new IVertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = bgraph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = bgraph.AddVertex(quad[i]);
                }
                IEdge edge = bgraph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
            }
            Assert.AreEqual(numEdges, BaseTest.Count(graph.GetEdges()));

            bgraph.Shutdown();
        }

        [Test]
        public void TestLoadingWithExisting1()
        {
            const int numEdges = 1000;
            string[][] quads = GenerateQuads(100, numEdges, new[]{"knows", "friend"});
            var tg = new TinkerGraph();
            var bg = new BatchGraph(new WritethroughGraph(tg), VertexIdType.String, 100);
            bg.SetLoadingFromScratch(false);
            int counter = 0;
            foreach (string[] quad in quads)
            {
                IGraph graph;
                if (counter < numEdges / 2) graph = tg;
                else graph = bg;

                var vertices = new IVertex[2];
                for (int i = 0; i < 2; i++)
                {
                    vertices[i] = graph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                }
                IEdge edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));

            bg.Shutdown();
        }

        [Test]
        public void TestLoadingWithExisting2()
        {
            const int numEdges = 1000;
            string[][] quads = GenerateQuads(100, numEdges, new[]{"knows", "friend"});
            TinkerGraph tg = new IgnoreIdTinkerGraph();
            var bg = new BatchGraph(new WritethroughGraph(tg), VertexIdType.String, 100);
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

            int counter = 0;
            foreach (string[] quad in quads)
            {
                IGraph graph;
                if (counter < numEdges / 2) graph = tg;
                else graph = bg;

                var vertices = new IVertex[2];
                for (int i = 0; i < 2; i++) {
                    vertices[i] = graph.GetVertex(quad[i]);
                    if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                }
                IEdge edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                edge.SetProperty("annotation", quad[3]);
                counter++;
            }
            Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));

            bg.Shutdown();
        }

        public static string[][] GenerateQuads(int numVertices, int numEdges, string[] labels)
        {
            var random = new Random();
            var edges = new string[numEdges][];
            for (int i = 0; i < numEdges; i++)
            {
                edges[i] = new string[4];
                edges[i][0] = string.Concat("v", random.Next(numVertices), 1);
                edges[i][1] = string.Concat("v", random.Next(numVertices), 1);
                edges[i][2] = labels[random.Next(labels.Length)];
                edges[i][3] = random.Next().ToString(CultureInfo.InvariantCulture);
            }
            return edges;
        }

        public void LoadingTest(int total, int bufferSize, VertexIdType type, ILoadingFactory ids)
        {
            var counter = new VertexEdgeCounter();

            MockTransactionalGraph tgraph = _ignoreIDs ? new MockTransactionalGraph(new IgnoreIdTinkerGraph()) : new MockTransactionalGraph(new TinkerGraph());

            var graph = new BlGraph(this, tgraph, counter, ids);
            var loader = new BatchGraph(graph, type, bufferSize);

            if (_assignKeys)
                loader.SetVertexIdKey(VertexIdKey);
                loader.SetEdgeIdKey(EdgeIdKey);

            //Create a chain
            int chainLength = total;
            IVertex previous = null;
            for (int i = 0; i <= chainLength; i++)
            {
                IVertex next = loader.AddVertex(ids.GetVertexId(i));
                next.SetProperty(Uid, i);
                counter.NumVertices++;
                counter.TotalVertices++;
                if (previous != null)
                {
                    IEdge e = loader.AddEdge(ids.GetEdgeId(i), loader.GetVertex(previous.GetId()), loader.GetVertex(next.GetId()), "next");
                    e.SetProperty(Uid, i);
                    counter.NumEdges++;
                }
                previous = next;
            }

            loader.Commit();
            Assert.True(tgraph.AllSuccessful());

            loader.Shutdown();
        }

        class VertexEdgeCounter
        {
            public int NumVertices;
            public int NumEdges;
            public int TotalVertices;
        }

        class BlGraph : ITransactionalGraph
        {
            const int KeepLast = 10;

            readonly VertexEdgeCounter _counter;
            bool _first = true;
            readonly ILoadingFactory _ids;

            readonly ITransactionalGraph _graph;
            readonly BatchGraphTest _batchGraphTest;

            public BlGraph(BatchGraphTest batchGraphTest, ITransactionalGraph graph, VertexEdgeCounter counter, ILoadingFactory ids)
            {
                _batchGraphTest = batchGraphTest;
                _graph = graph;
                _counter = counter;
                _ids = ids;
            }

            static object ParseId(object id)
            {
                var s = id as string;
                if (s != null)
                {
                    try
                    {
                        return int.Parse(s);
                    }
                    catch (FormatException)
                    {
                        return id;
                    }
                }
                return id;
            }

            public void Commit()
            {
                _graph.Commit();
                VerifyCounts();
            }

            
            public void Rollback()
            {
                _graph.Rollback();
                VerifyCounts();
            }

            void VerifyCounts() 
            {
                //System.out.println("Committed (vertices/edges): " + counter.numVertices + " / " + counter.numEdges);
                Assert.AreEqual(_counter.NumVertices, BaseTest.Count(_graph.GetVertices()) - (_first ? 0 : KeepLast));
                Assert.AreEqual(_counter.NumEdges, BaseTest.Count(_graph.GetEdges()));
                foreach (IEdge e in GetEdges())
                {
                    var id = (int)e.GetProperty(Uid);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.GetEdgeId(id), ParseId(e.GetId()));
                    
                    Assert.AreEqual(1, (int)e.GetVertex(Direction.In).GetProperty(Uid) - (int)e.GetVertex(Direction.Out).GetProperty(Uid));
                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.GetEdgeId(id), e.GetProperty(EdgeIdKey));
                }
                foreach (IVertex v in GetVertices())
                {
                    var id = (int)v.GetProperty(Uid);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.GetVertexId(id), ParseId(v.GetId()));
                    
                    Assert.True(2 >= BaseTest.Count(v.GetEdges(Direction.Both)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.In)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.Out)));

                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.GetVertexId(id), v.GetProperty(VertexIdKey));
                }
                foreach (IVertex v in GetVertices())
                {
                    var id = (int)v.GetProperty(Uid);
                    if (id < _counter.TotalVertices - KeepLast)
                        RemoveVertex(v);
                }
                foreach (IEdge e in GetEdges()) RemoveEdge(e);
                Assert.AreEqual(KeepLast, BaseTest.Count(_graph.GetVertices()));
                _counter.NumVertices = 0;
                _counter.NumEdges = 0;
                _first = false;
                //System.out.println("------");
            }

            public Features GetFeatures()
            {
                return _graph.GetFeatures();
            }

            public IVertex AddVertex(object id)
            {
                return _graph.AddVertex(id);
            }

            public IVertex GetVertex(object id)
            {
                return _graph.GetVertex(id);
            }

            public void RemoveVertex(IVertex vertex)
            {
                _graph.RemoveVertex(vertex);
            }

            public IEnumerable<IVertex> GetVertices()
            {
                return _graph.GetVertices();
            }

            public IEnumerable<IVertex> GetVertices(string key, object value)
            {
                return _graph.GetVertices(key, value);
            }

            public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
            {
                return _graph.AddEdge(id, outVertex, inVertex, label);
            }

            public IEdge GetEdge(object id)
            {
                return _graph.GetEdge(id);
            }

            public void RemoveEdge(IEdge edge)
            {
                _graph.RemoveEdge(edge);
            }

            public IEnumerable<IEdge> GetEdges()
            {
                return _graph.GetEdges();
            }

            public IEnumerable<IEdge> GetEdges(string key, object value)
            {
                return _graph.GetEdges(key, value);
            }

            public void Shutdown()
            {
                _graph.Shutdown();
            }

            public IGraphQuery Query()
            {
                return _graph.Query();
            }
        }

        public interface ILoadingFactory
        {
            object GetVertexId(int id);

            object GetEdgeId(int id);
        }

        class StringLoadingFactory : ILoadingFactory
        {
            public object GetVertexId(int id)
            {
                return string.Concat("V", id);
            }

            public object GetEdgeId(int id)
            {
                return string.Concat("E", id);
            }
        }

        class NumberLoadingFactory : ILoadingFactory
        {
            public object GetVertexId(int id)
            {
                return id * 2;
            }

            public object GetEdgeId(int id)
            {
                return id * 2 + 1;
            }
        }

        class UrlLoadingFactory : ILoadingFactory
        {
            public object GetVertexId(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/vertex/", + id);
            }

            public object GetEdgeId(int id)
            {
                return string.Concat("http://www.tinkerpop.com/rdf/ns/edge#", id);
            }
        }
    }
}
