using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.Redis
{
    [Serializable]
    public class RedisGraph : IndexedGraph, IIndexStore
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
            :base(indexingService)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (multiplexer == null)
                throw new ArgumentNullException(nameof(multiplexer));
            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _factory = factory;
            Serializer = serializer;
            Multiplexer = multiplexer;
            Init(configuration);
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
            var nextId = id == null ? db.StringIncrement("globals:next_vertex_id") : id.ToInt64();            
            db.SetAdd("globals:vertices", nextId);
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

            db.SetRemove("globals:vertices", redisVertex.Id.ToString());

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
            return db.SetScan(key).Select(entry => new RedisVertex((long)entry, this));
        }

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            var db = Multiplexer.GetDatabase();
            var nextId = id == null ? db.StringIncrement("globals:next_edge_id") : id.ToInt64();            

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
            db.SetAdd("globals:edges", nextId);

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

            db.SetRemove("globals:edges", redisEdge.RawId);

            base.RemoveEdge(edge);
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(this, CollectionType.Edge, null);
            return db.SetScan(key).Select(entry => GetEdge((long)entry));
        }

        public virtual IEnumerable<IEdge> GetEdges(RedisVertex vertex, Direction direction, params string[] labels)
        {
            var db = Multiplexer.GetDatabase();

            if (direction == Direction.Out || direction == Direction.Both)
            {
                foreach (var outLabel in db.SortedSetScan(GetIdentifier(vertex, "labels_out")))
                {
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                    string labelVal2 = outLabel.Element.ToString();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                    if (labels.Length > 0 && !labels.Contains(labelVal2))
                        continue;

                    foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:out", labelVal2)))
                    {
                        var edgeId = (long) edge.Element;
                        var targetId = (long) edge.Score;
                        var targetVertex = new RedisVertex(targetId, this);
                        yield return new RedisEdge(edgeId, vertex, targetVertex, labelVal2, this);
                    }
                }
            }

            if (direction != Direction.In && direction != Direction.Both) yield break;

            foreach (var inLabel in db.SortedSetScan(GetIdentifier(vertex, "labels_in")))
            {
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                string labelVal = inLabel.Element.ToString();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                if(labels.Length > 0 && !labels.Contains(labelVal))
                    continue;

                foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:in", labelVal)))
                {
                    var edgeId = (long) edge.Element;
                    var targetId = (long) edge.Score;
                    var targetVertex = new RedisVertex(targetId, this);
                    yield return new RedisEdge(edgeId, targetVertex, vertex, labelVal, this);
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
                                    db.SetLength("globals:vertices"),
                                    db.SetLength("globals:edges")));
        }

        public object GetProperty(RedisElement element, string key)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var db = Multiplexer.GetDatabase();
            var val = db.HashGet(GetIdentifier(element, "properties"), key);
            return val != RedisValue.Null ? Serializer.Deserialize(val) : null;
        }

        public string GetIdentifier(RedisElement element, string suffix)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var prefix = element is RedisVertex ? "vertex:" : "edge:";
            var identifier = String.Concat(prefix, element.RawId);
            if (suffix != null)
                identifier = String.Concat(identifier, ":", suffix);
            return identifier;
        }

        public string GetLabeledIdentifier(RedisElement element, string suffix, string label)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return String.Concat(GetIdentifier(element, suffix), ":", label);
        }

        public IEnumerable<string> GetPropertyKeys(RedisElement element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var db = Multiplexer.GetDatabase();
            var keys = db.HashKeys(GetIdentifier(element, "properties"));
// ReSharper disable SpecifyACultureInStringConversionExplicitly
            return keys.Select((value => value.ToString())).ToArray();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
        }

        public void SetProperty(RedisElement element, string key, object value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var raw = Serializer.Serialize(value);
            var db = Multiplexer.GetDatabase();
            db.HashSet(GetIdentifier(element, "properties"), key, raw);
            SetIndexedKeyValue(element, key, value);
        }

        public object RemoveProperty(RedisElement element, string key)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

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

        public void LoadIndices()
        {

        }

        public void CreateIndex(string indexName, string indexColumn, Parameter[] parameters)
        {
            var db = Multiplexer.GetDatabase();
            db.SetAdd(indexColumn, indexName);
        }

        public List<string> GetIndices(string indexType)
        {
            var db = Multiplexer.GetDatabase();
            return db.SetScan(indexType).Select(value => (string)value).ToList();
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            var db = Multiplexer.GetDatabase();
            db.SetRemove(indexColumn, indexName);
            var result = indexingService.DeleteIndex(indexType, indexName, isUserIndex);
            return result;
        }

        public override void DropIndex(string indexName)
        {
            IndexableGraphContract.ValidateDropIndex(indexName);

            var indexStore = IndexingService as IIndexStore;
            if(indexStore != null)
                indexStore.DropIndex(indexName);
            else
                base.DropIndex(indexName);
        }

        public override void SetIndexedKeyValue(IElement element, string key, object value)
        {
            var indexStore = IndexingService as IIndexStore;
            if (indexStore != null)
            {
                var type = element is IVertex ? typeof(IVertex) : typeof(IEdge);
                var indices = GetIndices(type, false);
                if (!indices.HasIndex(key)) return;

                var id = element.Id.TryToInt64();
                if (!id.HasValue) throw new InvalidOperationException();

                var generation = indices.Set(id.Value, key, key, value);
                UpdateGeneration(generation);
            }
            else
                base.SetIndexedKeyValue(element, key, value);
        }
    }
}
