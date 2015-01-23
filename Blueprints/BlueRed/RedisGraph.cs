using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public class RedisGraph : IndexedGraph
    {
        private static readonly Features RedisGraphFeatures = new Features
        {
            SupportsDuplicateEdges = true,
            SupportsSelfLoops = true,
            SupportsSerializableObjectProperty = true,
            SupportsBooleanProperty = true,
            SupportsDoubleProperty = true,
            SupportsFloatProperty = true,
            SupportsIntegerProperty = true,
            SupportsPrimitiveArrayProperty = true,
            SupportsUniformListProperty = true,
            SupportsMixedListProperty = true,
            SupportsLongProperty = true,
            SupportsMapProperty = true,
            SupportsStringProperty = true,
            IgnoresSuppliedIds = true,
            IsPersistent = true,
            IsRdfModel = false,
            IsWrapper = false,
            SupportsIndices = true,
            SupportsKeyIndices = true,
            SupportsVertexKeyIndex = true,
            SupportsEdgeKeyIndex = true,
            SupportsVertexIndex = true,
            SupportsEdgeIndex = true,
            SupportsTransactions = false,
            SupportsVertexIteration = true,
            SupportsEdgeIteration = true,
            SupportsEdgeRetrieval = true,
            SupportsVertexProperties = true,
            SupportsEdgeProperties = true,
            SupportsThreadedTransactions = false,
            SupportsIdProperty = true,
            SupportsLabelProperty = true
        };

        private readonly IGraphFactory _factory;
        internal readonly IContentSerializer Serializer;

        internal readonly ConnectionMultiplexer Multiplexer;

        public RedisGraph(IGraphFactory factory,
                          IContentSerializer serializer, 
                          ConnectionMultiplexer multiplexer, 
                          IndexingService indexingService,
                          IGraphConfiguration configuration)
            :base(indexingService, configuration)
        {
            Contract.Requires(factory != null);
            Contract.Requires(serializer != null);
            Contract.Requires(multiplexer != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(configuration != null);

            _factory = factory;
            Serializer = serializer;
            Multiplexer = multiplexer;
        }

        protected override ThreadContext CreateThreadContext(IndexingService indexingService)
        {
            indexingService.LoadFromStore(new RedisIndexStore(Multiplexer));
            return new ThreadContext { IndexingService = indexingService };
        }

        protected override IVertex GetVertexInstance(long vertexId)
        {
            return new RedisVertex(vertexId, this);
        }

        public override Features Features
        {
            get { return RedisGraphFeatures; }
        }

        public override IVertex AddVertex(object id)
        {
            var db = Multiplexer.GetDatabase();
            var nextId = db.StringIncrement("globals:next_vertex_id");
            db.SortedSetAdd("globals:vertices", nextId, nextId);
            var vertex = new RedisVertex(nextId, this);
            db.StringSet(GetIdentifier(vertex, null), nextId);
            return vertex;
        }

        public override IVertex GetVertex(object id)
        {
            var vertexId = id.TryToInt64();
            if (!vertexId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var val = db.StringGet(String.Format("vertex:{0}", vertexId));
            return val != RedisValue.Null ? new RedisVertex(vertexId.Value, this) : null;
        }

        public override void RemoveVertex(IVertex vertex)
        {
            var redisVertex = (RedisVertex) vertex;
            foreach (var edge in redisVertex.GetEdges(Direction.Both))
            {
                edge.Remove();
            }

            var db = Multiplexer.GetDatabase();
            db.KeyDelete(GetIdentifier(redisVertex, null));
            db.KeyDelete(GetIdentifier(redisVertex, "properties"));
            db.KeyDelete(GetIdentifier(redisVertex, "edges:in"));
            db.KeyDelete(GetIdentifier(redisVertex, "edges:out"));

            var labelsIn = GetIdentifier(redisVertex, "labels_in");
            foreach (var inLabel in db.SortedSetScan(labelsIn))
            {
                db.KeyDelete(GetLabeledIdentifier(redisVertex, "edges:in", inLabel.Element));
            }
            db.KeyDelete(labelsIn);

            var labelsOut = GetIdentifier(redisVertex, "labels_out");
            foreach (var outLabel in db.SortedSetScan(labelsOut))
            {
                db.KeyDelete(GetLabeledIdentifier(redisVertex, "edges:out", outLabel.Element));
            }
            db.KeyDelete(labelsOut);

            db.SortedSetRemove("globals:vertices", (long)redisVertex.Id);

            base.RemoveVertex(vertex);
        }

        enum CollectionType
        {
            Vertex,
            Edge,
            EdgesIn,
            EdgesOut
        }

        static string GetCollectionKey(RedisGraph graph, CollectionType type, RedisElement element)
        {
            if (element == null)
            {
                if (type == CollectionType.Vertex) return "globals:vertices";
                if (type == CollectionType.Edge) return "globals:edges";
            }
            else if (element is IVertex)
            {
                if (type == CollectionType.EdgesIn) return graph.GetIdentifier(element, "edges:in");
                if (type == CollectionType.EdgesOut) return graph.GetIdentifier(element, "edges:out");
            }
            return String.Empty;
        }

        public override IEnumerable<IVertex> GetVertices()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(this, CollectionType.Vertex, null);
            return db.SortedSetScan(key).Select(entry => new RedisVertex((long)entry.Element, this));
        }

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            var db = Multiplexer.GetDatabase();
            var nextId = db.StringIncrement("globals:next_edge_id");
            var edge = new RedisEdge(nextId, outVertex, inVertex, label, this);
            db.StringSet(GetIdentifier(edge, "out"), (long)outVertex.Id);
            db.StringSet(GetIdentifier(edge, "in"), (long)inVertex.Id);
            db.StringSet(GetIdentifier(edge, "label"), label);

            var vin = (RedisVertex) inVertex;
            var vout = (RedisVertex) outVertex;
            db.SortedSetAdd(GetIdentifier(vin, "edges:in"), nextId, vout.RawId);
            db.SortedSetAdd(GetIdentifier(vout, "edges:out"), nextId, vin.RawId);

            db.SortedSetAdd(GetLabeledIdentifier(vin, "edges:in", label), nextId, vout.RawId);
            db.SortedSetAdd(GetLabeledIdentifier(vout, "edges:out", label), nextId, vin.RawId);

            db.SortedSetIncrement(GetIdentifier(vin, "labels_in"), label, 1);
            db.SortedSetIncrement(GetIdentifier(vout, "labels_out"), label, 1);

            db.StringSet(GetIdentifier(edge, null), nextId);
            db.SortedSetAdd("globals:edges", nextId, nextId);

            return edge;
        }

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt64();
            if (!edgeId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var val = db.StringGet(String.Format("edge:{0}", edgeId));
            if (val == RedisValue.Null) return null;

            var idIn = (long)db.StringGet(String.Format("edge:{0}:in", edgeId));
            var idOut = (long)db.StringGet(String.Format("edge:{0}:out", edgeId));
            var label = (string)db.StringGet(String.Format("edge:{0}:label", edgeId));
            var vin = new RedisVertex(idIn, this);
            var vout = new RedisVertex(idOut, this);

            return new RedisEdge(edgeId.Value, vout, vin, label, this);
        }

        public override void RemoveEdge(IEdge edge)
        {
            var redisEdge = (RedisEdge) edge;
            var vin = (RedisVertex)edge.GetVertex(Direction.In);
            var vout = (RedisVertex)edge.GetVertex(Direction.Out);
            var db = Multiplexer.GetDatabase();

            db.KeyDelete(GetIdentifier(redisEdge, null));
            db.KeyDelete(GetIdentifier(redisEdge, "in"));
            db.KeyDelete(GetIdentifier(redisEdge, "out"));
            db.KeyDelete(GetIdentifier(redisEdge, "label"));
            db.KeyDelete(GetIdentifier(redisEdge, "properties"));

            db.SortedSetRemove(GetIdentifier(vout, "edges:out"), redisEdge.RawId);
            db.SortedSetRemove(GetIdentifier(vin, "edges:in"), redisEdge.RawId);

            db.SortedSetRemove(GetLabeledIdentifier(vout, "edges:out", redisEdge.Label), redisEdge.RawId);
            db.SortedSetRemove(GetLabeledIdentifier(vin, "edges:in", redisEdge.Label), redisEdge.RawId);

            var outLabels = GetIdentifier(vout, "labels_out");
            db.SortedSetDecrement(outLabels, redisEdge.Label, 1);
            db.SortedSetRemoveRangeByScore(outLabels, -1, 0);

            var inLabels = GetIdentifier(vin, "labels_in");
            db.SortedSetDecrement(inLabels, redisEdge.Label, 1);
            db.SortedSetRemoveRangeByScore(inLabels, -1, 0);

            db.SortedSetRemove("globals:edges", redisEdge.RawId);

            base.RemoveEdge(edge);
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(this, CollectionType.Edge, null);
            return db.SortedSetScan(key).Select(entry => GetEdge((long)entry.Element));
        }

        public virtual IEnumerable<IEdge> GetEdges(RedisVertex vertex, Direction direction, params string[] labels)
        {
            var db = Multiplexer.GetDatabase();

            if (direction == Direction.Out || direction == Direction.Both)
            {
                foreach (var outLabel in db.SortedSetScan(GetIdentifier(vertex, "labels_out")))
                {
                    var label = (string)outLabel.Element;
                    if(labels.Length > 0 && !labels.Contains(label))
                        continue;

                    foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:out", label)))
                    {
                        var edgeId = (long) edge.Element;
                        var targetId = (long) edge.Score;
                        var targetVertex = new RedisVertex(targetId, this);
                        yield return new RedisEdge(edgeId, vertex, targetVertex, label, this);
                    }
                }
            }

            if (direction != Direction.In && direction != Direction.Both) yield break;

            foreach (var inLabel in db.SortedSetScan(GetIdentifier(vertex, "labels_in")))
            {
                var label = (string)inLabel.Element;
                if(labels.Length > 0 && !labels.Contains(label))
                    continue;

                foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:in", label)))
                {
                    var edgeId = (long) edge.Element;
                    var targetId = (long) edge.Score;
                    var targetVertex = new RedisVertex(targetId, this);
                    yield return new RedisEdge(edgeId, targetVertex, vertex, label, this);
                }
            }
        }

        public override void Shutdown()
        {
            _factory.Destroy(this);
        }

        public override string ToString()
        {
            var db = Multiplexer.GetDatabase();
            return this.GraphString(String.Format("vertices: {0} Edges: {1}",
                                    db.SortedSetLength("globals:vertices"),
                                    db.SortedSetLength("globals:edges")));
        }

        public object GetProperty(RedisElement element, string key)
        {
            Contract.Requires(element != null);

            var db = Multiplexer.GetDatabase();
            var val = db.HashGet(GetIdentifier(element, "properties"), key);
            return val != RedisValue.Null ? Serializer.Deserialize(val) : null;
        }

        public string GetIdentifier(RedisElement element, string suffix)
        {
            Contract.Requires(element != null);

            var prefix = element is RedisVertex ? "vertex:" : "edge:";
            var identifier = String.Concat(prefix, element.RawId);
            if (suffix != null)
                identifier = String.Concat(identifier, ":", suffix);
            return identifier;
        }

        public string GetLabeledIdentifier(RedisElement element, string suffix, string label)
        {
            Contract.Requires(element != null);

            return String.Concat(GetIdentifier(element, suffix), ":", label);
        }

        public IEnumerable<string> GetPropertyKeys(RedisElement element)
        {
            Contract.Requires(element != null);

            var db = Multiplexer.GetDatabase();
            var keys = db.HashKeys(GetIdentifier(element, "properties"));
            return keys.Select((value => value.ToString())).ToArray();
        }

        public void SetProperty(RedisElement element, string key, object value)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var raw = Serializer.Serialize(value);
            var db = Multiplexer.GetDatabase();
            db.HashSet(GetIdentifier(element, "properties"), key, raw);
            SetIndexedKeyValue(element, key, value);
        }

        public object RemoveProperty(RedisElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var result = GetProperty(element, key);
            var db = Multiplexer.GetDatabase();
            db.HashDelete(GetIdentifier(element, "properties"), key);
            SetIndexedKeyValue(element, key, null);
            return result;
        }

        public static void DeleteDb()
        {
            var mp = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
            mp.GetServer("localhost:6379").FlushDatabase();
            mp.Dispose();
        }
    }
}
