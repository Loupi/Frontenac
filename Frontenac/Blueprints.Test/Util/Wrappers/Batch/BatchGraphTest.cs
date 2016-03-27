using System;
using System.Collections.Generic;
using System.Globalization;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    [TestFixture(Category = "BatchGraphTest")]
    public class BatchGraphTest
    {
        private const string Uid = "uid";

        private const string VertexIdKey = "vid";
        private const string EdgeIdKey = "eid";
        private bool _assignKeys;
        private bool _ignoreIDs;

        public static string[][] GenerateQuads(int numVertices, int numEdges, string[] labels)
        {
            var random = new Random();
            var edges = new string[numEdges][];
            for (var i = 0; i < numEdges; i++)
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

            var tgraph = _ignoreIDs
                             ? new MockTransactionalGraph(new IgnoreIdTinkerGrapĥ())
                             : new MockTransactionalGraph(new TinkerGrapĥ());
            try
            {
                var graph = new BlGraph(this, tgraph, counter, ids);
                var loader = new BatchGraph(graph, type, bufferSize);

                if (_assignKeys)
                    loader.SetVertexIdKey(VertexIdKey);
                loader.SetEdgeIdKey(EdgeIdKey);

                //Create a chain
                var chainLength = total;
                IVertex previous = null;
                for (var i = 0; i <= chainLength; i++)
                {
                    var next = loader.AddVertex(ids.GetVertexId(i));
                    next.SetProperty(Uid, i);
                    counter.NumVertices++;
                    counter.TotalVertices++;
                    if (previous != null)
                    {
                        var e = loader.AddEdge(ids.GetEdgeId(i), loader.GetVertex(previous.Id),
                                               loader.GetVertex(next.Id), "next");
                        e.SetProperty(Uid, i);
                        counter.NumEdges++;
                    }
                    previous = next;
                }

                loader.Commit();
                Assert.True(tgraph.AllSuccessful());
            }
            finally
            {
                tgraph.Shutdown();
            }
        }

        private class VertexEdgeCounter
        {
            public int NumEdges;
            public int NumVertices;
            public int TotalVertices;
        }

        private class BlGraph : ITransactionalGraph
        {
            private const int KeepLast = 10;
            private readonly BatchGraphTest _batchGraphTest;

            private readonly VertexEdgeCounter _counter;

            private readonly ITransactionalGraph _graph;
            private readonly ILoadingFactory _ids;
            private bool _first = true;

            public BlGraph(BatchGraphTest batchGraphTest, ITransactionalGraph graph, VertexEdgeCounter counter,
                           ILoadingFactory ids)
            {
                _batchGraphTest = batchGraphTest;
                _graph = graph;
                _counter = counter;
                _ids = ids;
            }

            public void Shutdown()
            {
                _graph?.Shutdown();
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

            public Features Features => _graph.Features;

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

            public IQuery Query()
            {
                return _graph.Query();
            }

            private static object ParseId(object id)
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

            private void VerifyCounts()
            {
                Assert.AreEqual(_counter.NumVertices, BaseTest.Count(_graph.GetVertices()) - (_first ? 0 : KeepLast));
                Assert.AreEqual(_counter.NumEdges, BaseTest.Count(_graph.GetEdges()));
                foreach (var e in GetEdges())
                {
                    var id = (int) e.GetProperty(Uid);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.GetEdgeId(id), ParseId(e.Id));

                    Assert.AreEqual(1,
                                    (int) e.GetVertex(Direction.In).GetProperty(Uid) -
                                    (int) e.GetVertex(Direction.Out).GetProperty(Uid));
                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.GetEdgeId(id), e.GetProperty(EdgeIdKey));
                }
                foreach (var v in GetVertices())
                {
                    var id = (int) v.GetProperty(Uid);
                    if (!_batchGraphTest._ignoreIDs)
                        Assert.AreEqual(_ids.GetVertexId(id), ParseId(v.Id));

                    Assert.True(2 >= BaseTest.Count(v.GetEdges(Direction.Both)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.In)));
                    Assert.True(1 >= BaseTest.Count(v.GetEdges(Direction.Out)));

                    if (_batchGraphTest._assignKeys)
                        Assert.AreEqual(_ids.GetVertexId(id), v.GetProperty(VertexIdKey));
                }
                foreach (var v in GetVertices())
                {
                    var id = (int) v.GetProperty(Uid);
                    if (id < _counter.TotalVertices - KeepLast)
                        RemoveVertex(v);
                }
                foreach (var e in GetEdges()) RemoveEdge(e);
                Assert.AreEqual(KeepLast, BaseTest.Count(_graph.GetVertices()));
                _counter.NumVertices = 0;
                _counter.NumEdges = 0;
                _first = false;
            }
        }

        public interface ILoadingFactory
        {
            object GetVertexId(int id);

            object GetEdgeId(int id);
        }

        private class StringLoadingFactory : ILoadingFactory
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

        private class NumberLoadingFactory : ILoadingFactory
        {
            public object GetVertexId(int id)
            {
                return id*2;
            }

            public object GetEdgeId(int id)
            {
                return id*2 + 1;
            }
        }

        private class UrlLoadingFactory : ILoadingFactory
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

        [Test]
        public void TestLoadingWithExisting1()
        {
            const int numEdges = 1000;
            var quads = GenerateQuads(100, numEdges, new[] {"knows", "friend"});
            var tg = new TinkerGrapĥ();
            try
            {
                var bg = new BatchGraph(new WritethroughGraph(tg), VertexIdType.String, 100);

                bg.SetLoadingFromScratch(false);
                var counter = 0;
                foreach (var quad in quads)
                {
                    IGraph graph;
                    if (counter < numEdges/2) graph = tg;
                    else graph = bg;

                    var vertices = new IVertex[2];
                    for (var i = 0; i < 2; i++)
                    {
                        vertices[i] = graph.GetVertex(quad[i]);
                        if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                    }
                    var edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                    edge.SetProperty("annotation", quad[3]);
                    counter++;
                }
                Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));
            }
            finally
            {
                tg.Shutdown();
            }
        }

        [Test]
        public void TestLoadingWithExisting2()
        {
            const int numEdges = 1000;
            var quads = GenerateQuads(100, numEdges, new[] {"knows", "friend"});
            var tg = new IgnoreIdTinkerGrapĥ();
            try
            {
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

                var counter = 0;
                foreach (var quad in quads)
                {
                    IGraph graph;
                    if (counter < numEdges/2) graph = tg;
                    else graph = bg;

                    var vertices = new IVertex[2];
                    for (var i = 0; i < 2; i++)
                    {
                        vertices[i] = graph.GetVertex(quad[i]);
                        if (vertices[i] == null) vertices[i] = graph.AddVertex(quad[i]);
                    }
                    var edge = graph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                    edge.SetProperty("annotation", quad[3]);
                    counter++;
                }
                Assert.AreEqual(numEdges, BaseTest.Count(tg.GetEdges()));
            }
            finally
            {
                tg.Shutdown();
            }
        }

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
        public void TestQuadLoading()
        {
            const int numEdges = 10000;
            var quads = GenerateQuads(100, numEdges, new[] {"knows", "friend"});
            var graph = new TinkerGrapĥ();
            try
            {
                var bgraph = new BatchGraph(new WritethroughGraph(graph), VertexIdType.String, 1000);
                foreach (var quad in quads)
                {
                    var vertices = new IVertex[2];
                    for (var i = 0; i < 2; i++)
                    {
                        vertices[i] = bgraph.GetVertex(quad[i]);
                        if (vertices[i] == null) vertices[i] = bgraph.AddVertex(quad[i]);
                    }
                    var edge = bgraph.AddEdge(null, vertices[0], vertices[1], quad[2]);
                    edge.SetProperty("annotation", quad[3]);
                }
                Assert.AreEqual(numEdges, BaseTest.Count(graph.GetEdges()));
            }
            finally
            {
                graph.Shutdown();
            }
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
    }
}