using System.Linq;
using Frontenac.Blueprints;
using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Util;
using Grave.Esent;
using Grave.Indexing;

namespace Grave
{
    public class GraveGraph : IKeyIndexableGraph, IIndexableGraph
    {
        internal readonly IndexingService IndexingService;

        static readonly Features GraveGraphFeatures = new Features
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

        readonly IGraveGraphFactory _factory;
        internal readonly EsentContext Context;
        bool _refreshRequired;
        long _generation;

        public GraveGraph(IGraveGraphFactory factory, IndexingService indexingService, EsentContext context)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            if (context == null)
                throw new ArgumentNullException("context");

            if (indexingService == null)
                throw new ArgumentNullException("indexingService");

            _factory = factory;
            Context = context;
            IndexingService = indexingService;
        }

        internal void UpdateGeneration(long generation)
        {
            _generation = generation;
            _refreshRequired = true;
        }

        internal void WaitForGeneration()
        {
            if (_refreshRequired)
            {
                IndexingService.WaitForGeneration(_generation);
                _refreshRequired = false;
            }
        }

        public virtual Features Features
        {
            get { return GraveGraphFeatures; }
        }

        public virtual IVertex AddVertex(object unused)
        {
            var id = Context.VertexTable.AddRow();
            return new GraveVertex(this, Context.VertexTable, id);
        }

        public virtual IVertex GetVertex(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            IVertex result = null;
            var vertexId = TryToInt32(id);
            if (!vertexId.HasValue) return null;

            if (Context.VertexTable.SetCursor(vertexId.Value))
                result = new GraveVertex(this, Context.VertexTable, vertexId.Value);

            return result;
        }

        public virtual void RemoveVertex(IVertex vertex)
        {
            if (!(vertex is GraveVertex))
                throw new ArgumentException("vertex");

            foreach (var edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            var vertexId = (int)vertex.Id;
            Context.VertexTable.DeleteRow(vertexId);
            var generation = IndexingService.VertexIndices.DeleteDocuments(vertexId);
            UpdateGeneration(generation);
        }

        public virtual IEnumerable<IVertex> GetVertices()
        {
            var cursor = Context.GetVerticesCursor();
            try
            {
                var id = cursor.MoveFirst();
                while (id != 0)
                {
                    yield return new GraveVertex(this, Context.VertexTable, id);
                    id = cursor.MoveNext();
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public virtual IEnumerable<IVertex> GetVertices(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (!IndexingService.VertexIndices.HasIndex(key))
                return new PropertyFilteredIterable<IVertex>(key, value, GetVertices());

            WaitForGeneration();
            return IndexingService.VertexIndices.Get(key, key, value)
                .Select(vertexId => new GraveVertex(this, Context.VertexTable, vertexId));
        }

        public virtual IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            if (outVertex == null)
                throw new ArgumentNullException("outVertex");

            if (inVertex == null)
                throw new ArgumentNullException("inVertex");

            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("label");

            var id = Context.EdgesTable.AddEdge(label, (int)inVertex.Id, (int)outVertex.Id);
            Context.VertexTable.AddEdge((int)inVertex.Id, Direction.In, label, id);
            Context.VertexTable.AddEdge((int)outVertex.Id, Direction.Out, label, id);
            return new GraveEdge(id, outVertex, inVertex, label, this, Context.EdgesTable);
        }

        public virtual IEdge GetEdge(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            IEdge result = null;
            var edgeId = TryToInt32(id);
            if (!edgeId.HasValue) return null;

            var data = Context.EdgesTable.TryGetEdge(edgeId.Value);
            if (data != null)
                result = new GraveEdge(edgeId.Value, GetVertex(data.Item3), GetVertex(data.Item2), data.Item1, this, Context.EdgesTable);

            return result;
        }

        static int? TryToInt32(object value)
        {
            int? result;

            if (value is int)
                result = (int)value;
            else if (value is string)
            {
                int intVal;
                result = int.TryParse(value as string, out intVal) ? (int?)intVal : null;
            }
            else if (value == null)
                result = null;
            else
            {
                try
                {
                    result = Convert.ToInt32(value);
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

        public virtual void RemoveEdge(IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            var edgeId = (int)edge.Id;
            Context.EdgesTable.DeleteRow(edgeId);
            Context.VertexTable.DeleteEdge((int)edge.GetVertex(Direction.In).Id, Direction.In, edge.Label, edgeId);
            Context.VertexTable.DeleteEdge((int)edge.GetVertex(Direction.Out).Id, Direction.Out, edge.Label, edgeId);
            var generation = IndexingService.EdgeIndices.DeleteDocuments(edgeId);
            UpdateGeneration(generation);
        }

        public virtual IEnumerable<IEdge> GetEdges()
        {
            var cursor = Context.GetEdgesCursor();
            try
            {
                var id = cursor.MoveFirst();
                while (id != 0)
                {
                    var data = cursor.GetEdgeData();
                    if (data != null)
                    {
                        var vertexOut = new GraveVertex(this, Context.VertexTable, data.Item3);
                        var vertexIn = new GraveVertex(this, Context.VertexTable, data.Item2);
                        yield return new GraveEdge(id, vertexOut, vertexIn, data.Item1, this, Context.EdgesTable);
                    }
                    id = cursor.MoveNext();
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public virtual IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (!IndexingService.EdgeIndices.HasIndex(key))
                return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());

            WaitForGeneration();
            return IterateEdges(key, value);
        }

        IEnumerable<IEdge> IterateEdges(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            var cursor = Context.GetEdgesCursor();
            try
            {
                var edgeIds = IndexingService.EdgeIndices.Get(key, key, value);
                foreach (var edgeId in edgeIds)
                {
                    var data = cursor.TryGetEdge(edgeId);
                    if (data != null)
                    {
                        var vertexOut = new GraveVertex(this, Context.VertexTable, data.Item3);
                        var vertexIn = new GraveVertex(this, Context.VertexTable, data.Item2);
                        yield return new GraveEdge(edgeId, vertexOut, vertexIn, data.Item1, this, Context.EdgesTable);
                    }
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public void DropKeyIndex(string key, Type elementClass)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (elementClass == null)
                throw new ArgumentNullException("elementClass");

            var generation = GetIndices(elementClass, false).DropIndex(key);
            if (generation != -1)
                UpdateGeneration(generation);
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (elementClass == null)
                throw new ArgumentNullException("elementClass");

            var indices = GetIndices(elementClass, false);
            if (indices.HasIndex(key)) return;
            
            indices.CreateIndex(key);

            if (elementClass == typeof(IVertex))
                KeyIndexableGraphHelper.ReIndexElements(this, GetVertices(), new HashSet<string>(new[] { key }));
            else
                KeyIndexableGraphHelper.ReIndexElements(this, GetEdges(), new HashSet<string>(new[] { key }));
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            if (elementClass == null)
                throw new ArgumentNullException("elementClass");

            return GetIndices(elementClass, false).GetIndices();
        }

        internal IndexCollection GetIndices(Type indexType, bool isUserIndex)
        {
            if (indexType == null)
                throw new ArgumentNullException("indexType");

            if (isUserIndex)
                return indexType == typeof(IVertex) ? IndexingService.UserVertexIndices : IndexingService.UserEdgeIndices;

            return indexType == typeof(IVertex) ? IndexingService.VertexIndices : IndexingService.EdgeIndices;
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("indexName");

            if (indexClass == null)
                throw new ArgumentNullException("indexClass");

            if (GetIndices(typeof(IVertex), true).HasIndex(indexName) ||
                GetIndices(typeof(IEdge), true).HasIndex(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            GetIndices(indexClass, true).CreateIndex(indexName);
            return new GraveIndex(indexName, indexClass, this, IndexingService);
        }

        public virtual IIndex GetIndex(string indexName, Type indexClass)
        {
            IIndex result = null;
            if (GetIndices(indexClass, true).HasIndex(indexName))
                result = new GraveIndex(indexName, indexClass, this, IndexingService);
            return result;
        }

        public virtual IEnumerable<IIndex> GetIndices()
        {
            return GetIndices(typeof(IVertex), true).GetIndices().Select(t => new GraveIndex(t, typeof(IVertex), this, IndexingService))
                .Concat(GetIndices(typeof(IEdge), true).GetIndices().Select(t => new GraveIndex(t, typeof(IEdge), this, IndexingService)));
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

        public virtual IGraphQuery Query()
        {
            WaitForGeneration();
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this,
                string.Format("vertices: {0} Edges: {1}",
                Context.VertexTable.GetApproximateRecordCount(15),
                Context.EdgesTable.GetApproximateRecordCount(15)));
        }

        public virtual void Shutdown()
        {
            _factory.Destroy(this);
        }
    }
}
