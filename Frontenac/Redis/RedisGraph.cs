using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace Frontenac.Redis
{
    [Serializable]
    public class RedisGraph : IndexedGraph, IIndexStore
    {
        private static readonly IdGenerator IdGenerator = 
            new IdGenerator(1, new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), new MaskConfig(46, 5, 12));
        
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

        public static string GetConnectionString()
        {
            string connectionString = null;
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;
            }
            catch
            {
                //ignored
            }

            return string.IsNullOrWhiteSpace(connectionString) ? "192.168.112.102:6379" : connectionString;
        }

        private readonly IGraphFactory _factory;
        internal readonly IContentSerializer Serializer;

        [NonSerialized]
        internal readonly ConnectionMultiplexer Multiplexer;

        [NonSerialized]
        protected readonly TransactionManager TransactionManager;

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
            
            _factory = factory;
            Serializer = serializer;
            Multiplexer = multiplexer;

            Init(configuration);

            if(this is RedisTransactionalGraph)
                TransactionManager = new TransactionManager(RedisTransactionMode.BatchTransaction, multiplexer, indexingService);
            else
                TransactionManager = new TransactionManager(RedisTransactionMode.SingleTransaction, multiplexer, indexingService);
        }

        protected override IVertex GetVertexInstance(long vertexId)
        {
            return new RedisVertex(vertexId, this);
        }

        public override Features Features => RedisGraphFeatures;
        
        public override IVertex AddVertex(object id)
        {
            IDatabase db;
            var batch = TransactionManager.Begin(out db);

            var nextId = id.TryToInt64();
            if (!nextId.HasValue)
                nextId = IdGenerator.CreateId();
                
            var vertex = new RedisVertex(nextId.Value, this);

            batch.SetAddAsync("globals:vertices", nextId);
            batch.StringSetAsync(GetIdentifier(vertex, null), nextId);

            TransactionManager.End();
            
            return vertex;
        }

        public override IVertex GetVertex(object id)
        {
            var vertexId = id.TryToInt64();
            if (!vertexId.HasValue) return null;
            var db = Multiplexer.GetDatabase();
            var val = db.StringGet($"vertex:{vertexId}");
            return val != RedisValue.Null ? new RedisVertex(vertexId.Value, this) : null;
        }

        public override void RemoveVertex(IVertex vertex)
        {
            IDatabase db;
            var batch = TransactionManager.Begin(out db);

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

            TransactionManager.End();
        }

        private enum CollectionType
        {
            Vertex,
            Edge,
            EdgesIn,
            EdgesOut
        }

        private static string GetCollectionKey(RedisGraph graph, CollectionType type, RedisElement element)
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
            return string.Empty;
        }

        public override IEnumerable<IVertex> GetVertices()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(this, CollectionType.Vertex, null);
            return db.SetScan(key).Select(entry => new RedisVertex((long)entry, this));
        }

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            IDatabase db;
            var batch = TransactionManager.Begin(out db);
            
            var nextId = id.TryToInt64();
            if (!nextId.HasValue)
                nextId = IdGenerator.CreateId();

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

            TransactionManager.End();

            return edge;
        }

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt64();
            if (!edgeId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var tasks = new Task<RedisValue>[4];

            var batch = db.CreateTransaction();
            var edgeKey = $"edge:{edgeId}";
            batch.AddCondition(Condition.KeyExists(edgeKey));
            tasks[0] = batch.StringGetAsync($"edge:{edgeId}");
            tasks[1] = batch.StringGetAsync($"edge:{edgeId}:in");
            tasks[2] = batch.StringGetAsync($"edge:{edgeId}:out");
            tasks[3] = batch.StringGetAsync($"edge:{edgeId}:label");
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
            var batch = TransactionManager.Begin(out db);

            RemoveEdge(edge, batch);
            batch.Execute();

            TransactionManager.End();
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
                    var labelVal2 = outLabel.Element.ToString();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                    if (labels.Length > 0 && !labels.Contains(labelVal2))
                        continue;

                    foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:out", labelVal2), default(RedisValue), 1000))
                    {
                        var edgeId = Math.Abs((long) edge.Element);
                        var targetId = Math.Abs((long) edge.Score);
                        var targetVertex = new RedisVertex(targetId, this);
                        yield return new RedisEdge(edgeId, vertex, targetVertex, labelVal2, this);
                    }
                }
            }

            if (direction != Direction.In && direction != Direction.Both) yield break;

            foreach (var inLabel in db.SortedSetScan(GetIdentifier(vertex, "labels_in")))
            {
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                var labelVal = inLabel.Element.ToString();
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                if(labels.Length > 0 && !labels.Contains(labelVal))
                    continue;

                foreach (var edge in db.SortedSetScan(GetLabeledIdentifier(vertex, "edges:in", labelVal), default(RedisValue), 1000))
                {
                    var edgeId = Math.Abs((long) edge.Element);
                    var targetId = Math.Abs((long) edge.Score);
                    var targetVertex = new RedisVertex(targetId, this);
                    yield return new RedisEdge(edgeId, targetVertex, vertex, labelVal, this);
                }
            }
        }

        public virtual long GetNbEdges(RedisVertex vertex, Direction direction, string label)
        {
            Contract.Requires(vertex != null);

            var db = Multiplexer.GetDatabase();

            return db.SortedSetLength(direction == Direction.Out 
                ? GetLabeledIdentifier(vertex, "edges:out", label) 
                : GetLabeledIdentifier(vertex, "edges:in", label));
        }

        public IEnumerable<IVertex> GetVertices(RedisVertex vertex, Direction direction, string label, IEnumerable<object> ids)
        {
            Contract.Requires(ids != null);

            var db = Multiplexer.GetDatabase();

            var key = GetLabeledIdentifier(vertex, direction == Direction.Out 
                ? "edges:out" 
                : "edges:in", label);

            return ids.Select(id => new {id, matches = db.SortedSetRangeByScore(key, (long) id, (long) id)})
                    .Where(t => t.matches.Length > 0)
                    .Select(t => GetVertex(t.id));
        }

        public override void Shutdown()
        {
            _factory.Destroy(this);
        }

        public override string ToString()
        {
            var db = Multiplexer.GetDatabase();
            return this.GraphString(
                $"vertices: {db.SetLength("globals:vertices")} Edges: {db.SetLength("globals:edges")}");
        }

        public object GetProperty(RedisElement element, string key)
        {
            Contract.Requires(element != null);

            var retry = 0;

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
            var identifier = string.Concat(prefix, element.RawId);
            if (suffix != null)
                identifier = string.Concat(identifier, ":", suffix);
            return identifier;
        }

        public string GetLabeledIdentifier(RedisElement element, string suffix, string label)
        {
            Contract.Requires(element != null);

            return string.Concat(GetIdentifier(element, suffix), ":", label);
        }

        public List<string> GetPropertyKeys(RedisElement element)
        {
            Contract.Requires(element != null);

            var retry = 0;

            while (retry < 3)
            {
                try
                {
                    var db = Multiplexer.GetDatabase();
                    var keys = db.HashKeys(GetIdentifier(element, "properties"));
                    return keys.Select(t => t.ToString()).ToList();
                }
                catch (TimeoutException)
                {
                    retry++;
                    if (retry == 3)
                        throw;
                }
            }
            return null;
        }

        public void SetProperty(RedisElement element, string key, object value)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var raw = Serializer.Serialize(value);

            IDatabase db;
            var batch = TransactionManager.Begin(out db);

            batch.HashSetAsync(GetIdentifier(element, "properties"), key, raw);
            SetIndexedKeyValue(element, key, value);

            TransactionManager.End();
        }

        public object RemoveProperty(RedisElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var result = GetProperty(element, key);

            IDatabase db;
            var batch = TransactionManager.Begin(out db);
 
            batch.HashDeleteAsync(GetIdentifier(element, "properties"), key);
            SetIndexedKeyValue(element, key, null);

            TransactionManager.End();

            return result;
        }

        public static void DeleteDb()
        {
            var config = new ConfigurationOptions
            {
                ConnectTimeout = 60000,
                ResponseTimeout = 60000,
                ConnectRetry = 3,
                SyncTimeout = 60000,
                AllowAdmin = true,
                AbortOnConnectFail = false
            };

            var endpoints = GetConnectionString().Split(';');
            foreach (var endpoint in endpoints)
                config.EndPoints.Add(endpoint);

            var mp = ConnectionMultiplexer.Connect(config);
            mp.GetServer(config.EndPoints[0]).FlushDatabase();
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

        protected override IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection,
            IIndexCollection userIndexCollection)
        {
            return new TransactedIndex(indexName, indexType, this, this, IndexingService, TransactionManager);
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

        public Dictionary<long, Dictionary<string, object>> GetElementsMap(IEnumerable<long> ids)
        {
            Contract.Requires(ids != null);

            var db = Multiplexer.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = ids.Select(
                id => new {id, task = batch.HashGetAllAsync(GetRawIdentifier("vertex:", id, "properties"))})
                .Select(t => new Tuple<Task<HashEntry[]>, long>(t.task, t.id)).ToList();

            batch.Execute();

            var result = new Dictionary<long, Dictionary<string, object>>();
            foreach (var task in tasks)
            {
                task.Item1.Wait();
                var dict = task.Item1.Result.ToDictionary(entry => entry.Name.ToString(), 
                    entry => entry.Value != RedisValue.Null 
                        ? Serializer.Deserialize(entry.Value) 
                        : null);

                result.Add(task.Item2, dict);
            }

            return result;
        }

        public string GetRawIdentifier(string prefix, long id, string suffix)
        {

            var identifier = string.Concat(prefix, id);
            if (suffix != null)
                identifier = string.Concat(identifier, ":", suffix);
            return identifier;
        }
    }
}
