using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Serializers;
using IdGen;
using StackExchange.Redis;
using UniqueIdGenerator.Net;
using RustFlakes;

namespace Frontenac.Redis
{
    [Serializable]
    public class RedisGraph : IndexedGraph, IIndexStore
    {
        static readonly IdGenerator IdGenerator = new IdGenerator(1);
        static readonly Generator IdGenerator2 = new Generator(1, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        static readonly UInt64Oxidation IdGenerator3 = new UInt64Oxidation(1);
        
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
            IgnoresSuppliedIds = false,
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

        private readonly RedisTransaction _transaction;

        public RedisGraph(IGraphFactory factory,
                          IContentSerializer serializer, 
                          ConnectionMultiplexer multiplexer, 
                          IndexingService indexingService,
                          IGraphConfiguration configuration)
            :base(indexingService)
        {
            Contract.Requires(factory != null);
            Contract.Requires(serializer != null);
            Contract.Requires(multiplexer != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(configuration != null);

            _transaction = new RedisTransaction(RedisTransactionMode.SingleTransaction, multiplexer);
            
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

        private int iv = 1;
        public override IVertex AddVertex(object id)
        {
            IDatabase db;
            var batch = _transaction.Begin(out db);

            var nextId = id.TryToInt64();
            if (!nextId.HasValue)
                nextId = db.StringIncrement("globals:next_vertex_id");
                //nextId = (long)IdGenerator.CreateId();
                //nextId = (long)IdGenerator2.NextLong();
                //nextId = iv++;
                //nextId = (long)IdGenerator3.Oxidize();

            var vertex = new RedisVertex(nextId.Value, this);

            batch.SetAddAsync("globals:vertices", nextId);
            batch.StringSetAsync(GetIdentifier(vertex, null), nextId);

            _transaction.End();
            
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
            IDatabase db;
            var batch = _transaction.Begin(out db);

            var redisVertex = (RedisVertex) vertex;
            foreach (var edge in redisVertex.GetEdges(Direction.Both))
            {
                RemoveEdge(edge, batch);
            }

            batch.KeyDeleteAsync(GetIdentifier(redisVertex, null));
            batch.KeyDeleteAsync(GetIdentifier(redisVertex, "properties"));
            batch.KeyDeleteAsync(GetIdentifier(redisVertex, "edges:in"));
            batch.KeyDeleteAsync(GetIdentifier(redisVertex, "edges:out"));

            var labelsIn = GetIdentifier(redisVertex, "labels_in");
            foreach (var inLabel in db.SortedSetScan(labelsIn))
            {
                batch.KeyDeleteAsync(GetLabeledIdentifier(redisVertex, "edges:in", inLabel.Element));
            }
            batch.KeyDeleteAsync(labelsIn);

            var labelsOut = GetIdentifier(redisVertex, "labels_out");
            foreach (var outLabel in db.SortedSetScan(labelsOut))
            {
                batch.KeyDeleteAsync(GetLabeledIdentifier(redisVertex, "edges:out", outLabel.Element));
            }
            batch.KeyDeleteAsync(labelsOut);

            batch.SetRemoveAsync("globals:vertices", redisVertex.Id.ToString());

            base.RemoveVertex(vertex);

            _transaction.End();
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

        private int ie = 1;

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            IDatabase db;
            var batch = _transaction.Begin(out db);

            var nextId = id.TryToInt64();
            if (!nextId.HasValue)
                nextId = db.StringIncrement("globals:next_edge_id");
                //nextId = (long)IdGenerator.CreateId();
                //nextId = (long)IdGenerator2.NextLong();
                //nextId = ie++;
                //nextId = (long)IdGenerator3.Oxidize();

            var edge = new RedisEdge(nextId.Value, outVertex, inVertex, label, this);
            var vin = (RedisVertex)inVertex;
            var vout = (RedisVertex)outVertex;

            batch.StringSetAsync(GetIdentifier(edge, "out"), (long) outVertex.Id);
            batch.StringSetAsync(GetIdentifier(edge, "in"), (long)inVertex.Id);
            batch.StringSetAsync(GetIdentifier(edge, "label"), label);

            batch.SortedSetAddAsync(GetIdentifier(vin, "edges:in"), nextId, vout.RawId);
            batch.SortedSetAddAsync(GetIdentifier(vout, "edges:out"), nextId, vin.RawId);

            batch.SortedSetAddAsync(GetLabeledIdentifier(vin, "edges:in", label), nextId, vout.RawId);
            batch.SortedSetAddAsync(GetLabeledIdentifier(vout, "edges:out", label), nextId, vin.RawId);

            batch.SortedSetIncrementAsync(GetIdentifier(vin, "labels_in"), label, 1);
            batch.SortedSetIncrementAsync(GetIdentifier(vout, "labels_out"), label, 1);

            batch.StringSetAsync(GetIdentifier(edge, null), nextId);
            batch.SetAddAsync("globals:edges", nextId);

            _transaction.End();

            return edge;
        }

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt64();
            if (!edgeId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var tasks = new Task<RedisValue>[4];

            var batch = db.CreateTransaction();
            var edgeKey = String.Format("edge:{0}", edgeId);
            batch.AddCondition(Condition.KeyExists(edgeKey));
            tasks[0] = batch.StringGetAsync(String.Format("edge:{0}", edgeId));
            tasks[1] = batch.StringGetAsync(String.Format("edge:{0}:in", edgeId));
            tasks[2] = batch.StringGetAsync(String.Format("edge:{0}:out", edgeId));
            tasks[3] = batch.StringGetAsync(String.Format("edge:{0}:label", edgeId));
            if (!batch.Execute()) return null;
            Task.WaitAll(tasks.OfType<Task>().ToArray());

            var val = tasks[0].Result;
            if (val == RedisValue.Null) return null;

            var idIn = (long)tasks[1].Result;
            var idOut = (long)tasks[2].Result;
            var label = (string)tasks[3].Result;
            var vin = new RedisVertex(idIn, this);
            var vout = new RedisVertex(idOut, this);

            return new RedisEdge(edgeId.Value, vout, vin, label, this);
        }

        public override void RemoveEdge(IEdge edge)
        {
            IDatabase db;
            var batch = _transaction.Begin(out db);

            RemoveEdge(edge, batch);
            batch.Execute();

            _transaction.End();
        }

        private void RemoveEdge(IEdge edge, IBatch batch)
        {
            var redisEdge = (RedisEdge)edge;
            var vin = (RedisVertex)edge.GetVertex(Direction.In);
            var vout = (RedisVertex)edge.GetVertex(Direction.Out);

            batch.KeyDeleteAsync(GetIdentifier(redisEdge, null));
            batch.KeyDeleteAsync(GetIdentifier(redisEdge, "in"));
            batch.KeyDeleteAsync(GetIdentifier(redisEdge, "out"));
            batch.KeyDeleteAsync(GetIdentifier(redisEdge, "label"));
            batch.KeyDeleteAsync(GetIdentifier(redisEdge, "properties"));

            batch.SortedSetRemoveAsync(GetIdentifier(vout, "edges:out"), redisEdge.RawId);
            batch.SortedSetRemoveAsync(GetIdentifier(vin, "edges:in"), redisEdge.RawId);

            batch.SortedSetRemoveAsync(GetLabeledIdentifier(vout, "edges:out", redisEdge.Label), redisEdge.RawId);
            batch.SortedSetRemoveAsync(GetLabeledIdentifier(vin, "edges:in", redisEdge.Label), redisEdge.RawId);

            var outLabels = GetIdentifier(vout, "labels_out");
            batch.SortedSetDecrementAsync(outLabels, redisEdge.Label, 1);
            batch.SortedSetRemoveRangeByScoreAsync(outLabels, -1, 0);

            var inLabels = GetIdentifier(vin, "labels_in");
            batch.SortedSetDecrementAsync(inLabels, redisEdge.Label, 1);
            batch.SortedSetRemoveRangeByScoreAsync(inLabels, -1, 0);

            batch.SetRemoveAsync("globals:edges", redisEdge.RawId);

            base.RemoveEdge(edge);

            batch.Execute();
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
            Contract.Requires(element != null);

            int retry = 0;

            while (retry < 3)
            {
                try
                {
                    var db = Multiplexer.GetDatabase();
                    var val = db.HashGet(GetIdentifier(element, "properties"), key);
                    return val != RedisValue.Null ? Serializer.Deserialize(val) : null;
                }
                catch (TimeoutException)
                {
                    retry++;
                    if(retry == 3)
                        throw;
                }
            }

            return null;
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
// ReSharper disable SpecifyACultureInStringConversionExplicitly
            return keys.Select((value => value.ToString())).ToArray();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
        }

        public void SetProperty(RedisElement element, string key, object value)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var raw = Serializer.Serialize(value);

            IDatabase db;
            var batch = _transaction.Begin(out db);

            batch.HashSetAsync(GetIdentifier(element, "properties"), key, raw);
            SetIndexedKeyValue(element, key, value);

            _transaction.End();
        }

        public object RemoveProperty(RedisElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var result = GetProperty(element, key);

            IDatabase db;
            var batch = _transaction.Begin(out db);
 
            batch.HashDeleteAsync(GetIdentifier(element, "properties"), key);
            SetIndexedKeyValue(element, key, null);

            _transaction.End();

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

                var id = element.Id.ToInt64();
                var generation = indices.Set(id, key, key, value);
                UpdateGeneration(generation);
            }
            else
                base.SetIndexedKeyValue(element, key, value);
        }
    }
}
