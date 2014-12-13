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

        internal readonly IContentSerializer Serializer;
        internal readonly ConnectionMultiplexer Multiplexer;

        public RedisGraph(IContentSerializer serializer, 
                          ConnectionMultiplexer multiplexer, 
                          IIndexingServiceFactory indexingServiceFactory)
            :base(indexingServiceFactory)
        {
            Contract.Requires(serializer != null);
            Contract.Requires(multiplexer != null);
            Contract.Requires(indexingServiceFactory != null);

            Serializer = serializer;
            Multiplexer = multiplexer;
        }

        protected override ThreadContext CreateThreadContext(IIndexingServiceFactory indexingServiceFactory)
        {
            var indexing = indexingServiceFactory.Create();
            indexing.LoadFromStore(new RedisIndexStore(Multiplexer));
            return new ThreadContext { IndexingService = indexing };
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
            db.StringSet(vertex.GetIdentifier(null), nextId);
            return vertex;
        }

        public override IVertex GetVertex(object id)
        {
            var vertexId = id.TryToInt64();
            if (!vertexId.HasValue) return null;

            var db = Multiplexer.GetDatabase();
            var val = db.StringGet(string.Format("vertex:{0}", vertexId));
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

            base.RemoveVertex(vertex);
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

        public override IEnumerable<IVertex> GetVertices()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(CollectionType.Vertex, null);
            return db.SortedSetScan(key).Select(entry => new RedisVertex((long)entry.Element, this));
        }

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
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

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt64();
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

        public override void RemoveEdge(IEdge edge)
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

            base.RemoveEdge(edge);
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            var db = Multiplexer.GetDatabase();
            var key = GetCollectionKey(CollectionType.Edge, null);
            return db.SortedSetScan(key).Select(entry => GetEdge((long)entry.Element));
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

        public override void Shutdown()
        {
            
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
