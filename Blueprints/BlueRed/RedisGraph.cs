using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public class RedisGraph : IKeyIndexableGraph, IIndexableGraph, IGenerationBasedIndex
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

        internal readonly IContentSerializer Serializer;
        internal readonly ConnectionMultiplexer Multiplexer;

        public class ThreadContext
        {
            public ThreadContext()
            {
                Indices = new Dictionary<string, Index>();
            }

            public IndexingService IndexingService { get; set; }
            public Dictionary<string, Index> Indices { get; private set; }
            public long Generation { get; set; }
            public bool RefreshRequired { get; set; }
        }

        protected readonly ThreadLocal<ThreadContext> Contexts;

        public RedisGraph(IContentSerializer serializer, 
                          ConnectionMultiplexer multiplexer, 
                          IIndexingServiceFactory indexingServiceFactory)
        {
            Contract.Requires(serializer != null);
            Contract.Requires(multiplexer != null);
            Contract.Requires(indexingServiceFactory != null);

            Serializer = serializer;
            Multiplexer = multiplexer;

            Contexts = new ThreadLocal<ThreadContext>(() =>
            {
                var indexing = indexingServiceFactory.Create();
                indexing.LoadFromStore(new RedisIndexStore(multiplexer));

                return new ThreadContext()
                {
                    IndexingService = indexing
                };
            }, true);
        }

        public ThreadContext Context
        {
            get { return Contexts.Value; }
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (GetIndices(typeof(IVertex), true).HasIndex(indexName) ||
                GetIndices(typeof(IEdge), true).HasIndex(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            var indexCollection = GetIndices(indexClass, true);
            var userIndexCollection = GetIndices(indexClass, false);
            indexCollection.CreateIndex(indexName);
            return CreateIndexObject(indexName, indexClass, indexCollection, userIndexCollection);
        }

        public virtual IIndex GetIndex(string indexName, Type indexClass)
        {
            var indexCollection = GetIndices(indexClass, true);
            var userIndexCollection = GetIndices(indexClass, false);
            return indexCollection.HasIndex(indexName)
                       ? CreateIndexObject(indexName, indexClass, indexCollection, userIndexCollection)
                       : null;
        }

        public virtual IEnumerable<IIndex> GetIndices()
        {
            var vertexIndexCollection = GetIndices(typeof(IVertex), true);
            var edgeIndexCollection = GetIndices(typeof(IEdge), true);

            var userVertexIndexCollection = GetIndices(typeof(IVertex), false);
            var userEdgeIndexCollection = GetIndices(typeof(IEdge), false);

            return vertexIndexCollection.GetIndices()
                .Select(t => CreateIndexObject(t, typeof(IVertex), vertexIndexCollection, userVertexIndexCollection))
                .Concat(edgeIndexCollection.GetIndices()
                        .Select(t => CreateIndexObject(t, typeof(IEdge), edgeIndexCollection, userEdgeIndexCollection)));
        }

        protected virtual IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection, IIndexCollection userIndexCollection)
        {
            var key = string.Concat(indexName, indexType);
            Index index;
            if (!Context.Indices.TryGetValue(key, out index))
            {
                index = new Index(indexName, indexType, this, this, Context.IndexingService);
                Context.Indices.Add(key, index);
            }
            return index;
        }

        public virtual void DropIndex(string indexName)
        {
            long generation = -1;
            if (GetIndices(typeof(IVertex), true).HasIndex(indexName))
                generation = GetIndices(typeof(IVertex), true).DropIndex(indexName);
            else if (GetIndices(typeof(IEdge), true).HasIndex(indexName))
                generation = GetIndices(typeof(IEdge), true).DropIndex(indexName);

            if (generation != -1)
                UpdateGeneration(generation);
        }

        internal IIndexCollection GetIndices(Type indexType, bool isUserIndex)
        {
            Contract.Requires(indexType != null);
            Contract.Requires(indexType.IsAssignableFrom(typeof(IVertex)) || indexType.IsAssignableFrom(typeof(IEdge)));

            if (indexType == null)
                throw new ArgumentNullException("indexType");

            if (isUserIndex)
                return indexType == typeof(IVertex)
                           ? Context.IndexingService.UserVertexIndices
                           : Context.IndexingService.UserEdgeIndices;

            return indexType == typeof(IVertex) ? Context.IndexingService.VertexIndices : Context.IndexingService.EdgeIndices;
        }

        public void UpdateGeneration(long generation)
        {
            Context.Generation = generation;
            //Context.RefreshRequired = true;
        }

        public void WaitForGeneration()
        {
            //if (!Context.RefreshRequired) return;
            Context.IndexingService.WaitForGeneration(Context.Generation);
            //Context.RefreshRequired = false;
        }

        protected static long? TryToInt64(object value)
        {
            long? result;

            if (value is long)
                result = (long)value;
            else if (value is string)
            {
                long intVal;
                result = Int64.TryParse(value as string, out intVal) ? (long?)intVal : null;
            }
            else if (value == null)
                result = null;
            else
            {
                try
                {
                    result = Convert.ToInt64(value);
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                        result = null;
                    else
                        throw;
                }
            }

            return result;
        }

        public Features Features
        {
            get { return RedisGraphFeatures; }
        }

        public IVertex AddVertex(object id)
        {
            var db = Multiplexer.GetDatabase();
            var nextId = db.StringIncrement("globals:next_vertex_id");
            db.SortedSetAdd("globals:vertices", nextId, nextId);
            var vertex = new RedisVertex(nextId, this);
            db.StringSet(vertex.GetIdentifier(null), nextId);
            return vertex;
        }

        public IVertex GetVertex(object id)
        {
            var vertexId = TryToInt64(id);
            if (!vertexId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var val = db.StringGet(string.Format("vertex:{0}", vertexId));
            return val != RedisValue.Null ? new RedisVertex(vertexId.Value, this) : null;
        }

        public void RemoveVertex(IVertex vertex)
        {
            var redisVertex = (RedisVertex) vertex;
            foreach (var edge in redisVertex.GetEdges(Direction.Both))
            {
                edge.Remove();
            }

            var db = Multiplexer.GetDatabase();
            db.KeyDelete(redisVertex.GetIdentifier(null));
            db.KeyDelete(redisVertex.GetIdentifier("properties"));
            db.KeyDelete(redisVertex.GetIdentifier("edges:in"));
            db.KeyDelete(redisVertex.GetIdentifier("edges:out"));

            var labelsIn = redisVertex.GetIdentifier("labels_in");
            foreach (var inLabel in db.SortedSetScan(labelsIn))
            {
                db.KeyDelete(redisVertex.GetLabeledIdentifier("edges:in", inLabel.Element));
            }
            db.KeyDelete(labelsIn);

            var labelsOut = redisVertex.GetIdentifier("labels_out");
            foreach (var outLabel in db.SortedSetScan(labelsOut))
            {
                db.KeyDelete(redisVertex.GetLabeledIdentifier("edges:out", outLabel.Element));
            }
            db.KeyDelete(labelsOut);

            db.SortedSetRemove("globals:vertices", (long)redisVertex.Id);

            var generation = Context.IndexingService.VertexIndices.DeleteDocuments((long)redisVertex.Id);
            UpdateGeneration(generation);
        }

        enum CollectionType
        {
            Vertex,
            Edge,
            EdgesIn,
            EdgesOut
        }

        static string GetCollectionKey(CollectionType type, RedisElement element)
        {
            if (element == null)
            {
                if (type == CollectionType.Vertex) return "globals:vertices";
                if (type == CollectionType.Edge) return "globals:edges";
            }
            else if (element is IVertex)
            {
                if (type == CollectionType.EdgesIn) return element.GetIdentifier("edges:in");
                if (type == CollectionType.EdgesOut) return element.GetIdentifier("edges:out");
            }
            return string.Empty;
        }

        public IEnumerable<IVertex> GetVertices()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(CollectionType.Vertex, null);
            return db.SortedSetScan(key).Select(entry => new RedisVertex((long)entry.Element, this));
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            if (!Context.IndexingService.VertexIndices.HasIndex(key))
                return new PropertyFilteredIterable<IVertex>(key, value, GetVertices());

            WaitForGeneration();
            return Context.IndexingService.VertexIndices.Get(key, key, value, int.MaxValue)
                .Select(vertexId => new RedisVertex(vertexId, this));
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            var db = Multiplexer.GetDatabase();
            var nextId = db.StringIncrement("globals:next_edge_id");
            var edge = new RedisEdge(nextId, outVertex, inVertex, label, this);
            db.StringSet(edge.GetIdentifier("out"), (long)outVertex.Id);
            db.StringSet(edge.GetIdentifier("in"), (long)inVertex.Id);
            db.StringSet(edge.GetIdentifier("label"), label);

            var vin = (RedisVertex) inVertex;
            var vout = (RedisVertex) outVertex;
            db.SortedSetAdd(vin.GetIdentifier("edges:in"), nextId, vout.RawId);
            db.SortedSetAdd(vout.GetIdentifier("edges:out"), nextId, vin.RawId);

            db.SortedSetAdd(vin.GetLabeledIdentifier("edges:in", label), nextId, vout.RawId);
            db.SortedSetAdd(vout.GetLabeledIdentifier("edges:out", label), nextId, vin.RawId);

            db.SortedSetIncrement(vin.GetIdentifier("labels_in"), label, 1);
            db.SortedSetIncrement(vout.GetIdentifier("labels_out"), label, 1);

            db.StringSet(edge.GetIdentifier(null), nextId);
            db.SortedSetAdd("globals:edges", nextId, nextId);

            return edge;
        }

        public IEdge GetEdge(object id)
        {
            var edgeId = TryToInt64(id);
            if (!edgeId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var val = db.StringGet(string.Format("edge:{0}", edgeId));
            if (val == RedisValue.Null) return null;

            var idIn = (long)db.StringGet(string.Format("edge:{0}:in", edgeId));
            var idOut = (long)db.StringGet(string.Format("edge:{0}:out", edgeId));
            var label = (string)db.StringGet(string.Format("edge:{0}:label", edgeId));
            var vin = new RedisVertex(idIn, this);
            var vout = new RedisVertex(idOut, this);

            return new RedisEdge(edgeId.Value, vout, vin, label, this);
        }

        public void RemoveEdge(IEdge edge)
        {
            var redisEdge = (RedisEdge) edge;
            var vin = (RedisVertex)edge.GetVertex(Direction.In);
            var vout = (RedisVertex)edge.GetVertex(Direction.Out);
            var db = Multiplexer.GetDatabase();

            db.KeyDelete(redisEdge.GetIdentifier(null));
            db.KeyDelete(redisEdge.GetIdentifier("in"));
            db.KeyDelete(redisEdge.GetIdentifier("out"));
            db.KeyDelete(redisEdge.GetIdentifier("label"));
            db.KeyDelete(redisEdge.GetIdentifier("properties"));

            db.SortedSetRemove(vout.GetIdentifier("edges:out"), redisEdge.RawId);
            db.SortedSetRemove(vin.GetIdentifier("edges:in"), redisEdge.RawId);

            db.SortedSetRemove(vout.GetLabeledIdentifier("edges:out", redisEdge.Label), redisEdge.RawId);
            db.SortedSetRemove(vin.GetLabeledIdentifier("edges:in", redisEdge.Label), redisEdge.RawId);

            var outLabels = vout.GetIdentifier("labels_out");
            db.SortedSetDecrement(outLabels, redisEdge.Label, 1);
            db.SortedSetRemoveRangeByScore(outLabels, -1, 0);

            var inLabels = vin.GetIdentifier("labels_in");
            db.SortedSetDecrement(inLabels, redisEdge.Label, 1);
            db.SortedSetRemoveRangeByScore(inLabels, -1, 0);

            db.SortedSetRemove("globals:edges", redisEdge.RawId);

            var generation = Context.IndexingService.EdgeIndices.DeleteDocuments(redisEdge.RawId);
            UpdateGeneration(generation);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(CollectionType.Edge, null);
            return db.SortedSetScan(key).Select(entry => GetEdge((long)entry.Element));
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (!Context.IndexingService.EdgeIndices.HasIndex(key))
                return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());

            WaitForGeneration();
            return IterateEdges(key, value);
        }

        public virtual IEnumerable<IEdge> GetEdges(RedisVertex vertex, Direction direction, params string[] labels)
        {
            var db = Multiplexer.GetDatabase();

            if (direction == Direction.Out || direction == Direction.Both)
            {
                foreach (var outLabel in db.SortedSetScan(vertex.GetIdentifier("labels_out")))
                {
                    var label = (string)outLabel.Element;
                    if(labels.Length > 0 && !labels.Contains(label))
                        continue;

                    foreach (var edge in db.SortedSetScan(vertex.GetLabeledIdentifier("edges:out", label)))
                    {
                        var edgeId = (long) edge.Element;
                        var targetId = (long) edge.Score;
                        var targetVertex = new RedisVertex(targetId, this);
                        yield return new RedisEdge(edgeId, vertex, targetVertex, label, this);
                    }
                }
            }

            if (direction != Direction.In && direction != Direction.Both) yield break;

            foreach (var inLabel in db.SortedSetScan(vertex.GetIdentifier("labels_in")))
            {
                var label = (string)inLabel.Element;
                if(labels.Length > 0 && !labels.Contains(label))
                    continue;

                foreach (var edge in db.SortedSetScan(vertex.GetLabeledIdentifier("edges:in", label)))
                {
                    var edgeId = (long) edge.Element;
                    var targetId = (long) edge.Score;
                    var targetVertex = new RedisVertex(targetId, this);
                    yield return new RedisEdge(edgeId, targetVertex, vertex, label, this);
                }
            }
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            var generation = GetIndices(elementClass, false).DropIndex(key);
            if (generation != -1)
                UpdateGeneration(generation);
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            var indices = GetIndices(elementClass, false);
            if (indices.HasIndex(key)) return;

            indices.CreateIndex(key);

            if (elementClass == typeof(IVertex))
                this.ReIndexElements(GetVertices(), new[] { key });
            else
                this.ReIndexElements(GetEdges(), new[] { key });
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            return GetIndices(elementClass, false).GetIndices();
        }

        public IQuery Query()
        {
            WaitForGeneration();
            return new IndexQuery(this, Context.IndexingService);
        }

        public void Shutdown()
        {
            
        }

        private IEnumerable<IEdge> IterateEdges(string key, object value)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(key));

            var edgeIds = Context.IndexingService.EdgeIndices.Get(key, key, value, int.MaxValue);
            return edgeIds.Select(edgeId => GetEdge(edgeId)).Where(edge => edge != null);
        }

        public override string ToString()
        {
            var db = Multiplexer.GetDatabase();
            return this.GraphString(String.Format("vertices: {0} Edges: {1}",
                                    db.SortedSetLength("globals:vertices"),
                                    db.SortedSetLength("globals:edges")));
        }
    }
}
