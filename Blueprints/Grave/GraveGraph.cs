using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Indexing;

namespace Frontenac.Grave
{
    public class GraveGraph : IKeyIndexableGraph, IIndexableGraph
    {
        private const string EdgeInPrefix = "$e_i_";
        private const string EdgeOutPrefix = "$e_o_";

        private static readonly Features GraveGraphFeatures = new Features
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

        internal readonly EsentContext Context;
        internal readonly IndexingService IndexingService;
        private readonly IGraveGraphFactory _factory;
        private readonly Dictionary<string, GraveIndex> _indices = new Dictionary<string, GraveIndex>();
        private long _generation;
        private bool _refreshRequired;
        
        public GraveGraph(IGraveGraphFactory factory, IndexingService indexingService)
        {
            Contract.Requires(factory != null);
            Contract.Requires(indexingService != null);

            _factory = factory;
            Context = factory.GetEsentContext();
            IndexingService = indexingService;
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (GetIndices(typeof (IVertex), true).HasIndex(indexName) ||
                GetIndices(typeof (IEdge), true).HasIndex(indexName))
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
            var vertexIndexCollection = GetIndices(typeof (IVertex), true);
            var edgeIndexCollection = GetIndices(typeof (IEdge), true);

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
            GraveIndex index;
            if (!_indices.TryGetValue(key, out index))
            {
                index = new GraveIndex(indexName, indexType, this, IndexingService);
                _indices.Add(key, index);
            }
            return index;
        }

        public virtual void DropIndex(string indexName)
        {
            long generation = -1;
            if (GetIndices(typeof (IVertex), true).HasIndex(indexName))
                generation = GetIndices(typeof (IVertex), true).DropIndex(indexName);
            else if (GetIndices(typeof (IEdge), true).HasIndex(indexName))
                generation = GetIndices(typeof (IEdge), true).DropIndex(indexName);

            if (generation != -1)
                UpdateGeneration(generation);
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
            IVertex result = null;
            var vertexId = TryToInt32(id);
            if (!vertexId.HasValue) return null;

            if (Context.VertexTable.SetCursor(vertexId.Value))
                result = new GraveVertex(this, Context.VertexTable, vertexId.Value);

            return result;
        }

        public virtual void RemoveVertex(IVertex vertex)
        {
            foreach (var edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            var vertexId = (int) vertex.Id;
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
            if (!IndexingService.VertexIndices.HasIndex(key))
                return new PropertyFilteredIterable<IVertex>(key, value, GetVertices());

            WaitForGeneration();
            return IndexingService.VertexIndices.Get(key, key, value, int.MaxValue)
                                  .Select(vertexId => new GraveVertex(this, Context.VertexTable, vertexId));
        }

        public virtual IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            var inVertexId = (int) inVertex.Id;
            var outVertexId = (int) outVertex.Id;
            var edgeId = Context.EdgesTable.AddEdge(label, inVertexId, outVertexId);
            Context.VertexTable.AddEdge(inVertexId, Direction.In, label, edgeId, outVertexId);
            Context.VertexTable.AddEdge(outVertexId, Direction.Out, label, edgeId, inVertexId);
            return new GraveEdge(edgeId, outVertex, inVertex, label, this, Context.EdgesTable);
        }

        public virtual IEdge GetEdge(object id)
        {
            var edgeId = TryToInt32(id);
            if (!edgeId.HasValue) return null;

            Tuple<string, int, int> data;
            return Context.EdgesTable.TryGetEdge(edgeId.Value, out data)
                       ? new GraveEdge(edgeId.Value, GetVertex(data.Item3), GetVertex(data.Item2), data.Item1, this,
                                       Context.EdgesTable)
                       : null;
        }

        public virtual void RemoveEdge(IEdge edge)
        {
            var edgeId = (int) edge.Id;
            Context.EdgesTable.DeleteRow(edgeId);
            var inVertexId = (int) edge.GetVertex(Direction.In).Id;
            var outVertexId = (int) edge.GetVertex(Direction.Out).Id;
            Context.VertexTable.DeleteEdge(inVertexId, Direction.In, edge.Label, edgeId, outVertexId);
            Context.VertexTable.DeleteEdge(outVertexId, Direction.Out, edge.Label, edgeId, inVertexId);
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
            if (!IndexingService.EdgeIndices.HasIndex(key))
                return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());

            WaitForGeneration();
            return IterateEdges(key, value);
        }

        public virtual IEnumerable<IEdge> GetEdges(GraveVertex vertex, Direction direction, params string[] labels)
        {
            var cursor = Context.GetVerticesCursor();
            try
            {
                var columns = cursor.GetColumns().ToArray();
                var edgeColumns = new List<string>();
                edgeColumns.AddRange(FilterLabels(direction, labels, Direction.In, columns, EdgeInPrefix));
                edgeColumns.AddRange(FilterLabels(direction, labels, Direction.Out, columns, EdgeOutPrefix));

                foreach (var label in edgeColumns)
                {
                    var isVertexIn = label.StartsWith(EdgeInPrefix);
                    var labelName = label.Substring(EdgeInPrefix.Length);
                    foreach (var edgeData in cursor.GetEdges((int)vertex.Id, label))
                    {
                        var edgeId = (int)(edgeData >> 32);
                        var targetId = (int)(edgeData & 0xFFFF);
                        var targetVertex = new GraveVertex(this, Context.VertexTable, targetId);
                        var outVertex = isVertexIn ? targetVertex : vertex;
                        var inVertex = isVertexIn ? vertex : targetVertex;
                        yield return
                            new GraveEdge(edgeId, outVertex, inVertex, labelName, this, Context.EdgesTable);
                    }
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        private static IEnumerable<string> FilterLabels(Direction direction, ICollection<string> labels, Direction directionFilter,
                                                        IEnumerable<string> columns, string prefix)
        {
            Contract.Requires(labels != null);
            Contract.Requires(columns != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(prefix));

            if (direction == directionFilter || direction == Direction.Both)
            {
                if (!labels.Any())
                    return columns.Where(t => t.StartsWith(prefix));

                var labelsFilter = labels.Select(t => String.Format("{0}{1}", prefix, t)).ToArray();
                return columns.Where(labelsFilter.Contains);
            }
            return Enumerable.Empty<string>();
        }

        public virtual object GetProperty(GraveElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            return element.Table.ReadCell(element.RawId, key);
        }

        public virtual IEnumerable<string> GetPropertyKeys(GraveElement element)
        {
            Contract.Requires(element != null);

            return element.Table.GetColumnsForRow(element.RawId).Where(t => !t.StartsWith("$"));
        }

        public virtual void SetProperty(GraveElement element, string key, object value)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            element.Table.WriteCell(element.RawId, key, value);
            element.SetIndexedKeyValue(key, value);
        }

        public virtual object RemoveProperty(GraveElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var result = element.Table.DeleteCell(element.RawId, key);
            element.SetIndexedKeyValue(key, null);
            return result;
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

            if (elementClass == typeof (IVertex))
                this.ReIndexElements(GetVertices(), new[] {key});
            else
                this.ReIndexElements(GetEdges(), new[] {key});
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            return GetIndices(elementClass, false).GetIndices();
        }

        public virtual IQuery Query()
        {
            WaitForGeneration();
            return new GraveQuery(this, IndexingService);
        }

        public virtual void Shutdown()
        {
            //_factory.Destroy(Context);
            Context.Dispose();
            _factory.Destroy(this);
        }

        internal void UpdateGeneration(long generation)
        {
            _generation = generation;
            _refreshRequired = true;
        }

        internal void WaitForGeneration()
        {
            if (!_refreshRequired) return;
            IndexingService.WaitForGeneration(_generation);
            _refreshRequired = false;
        }

        private static int? TryToInt32(object value)
        {
            int? result;

            if (value is int)
                result = (int) value;
            else if (value is string)
            {
                int intVal;
                result = Int32.TryParse(value as string, out intVal) ? (int?) intVal : null;
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

        private IEnumerable<IEdge> IterateEdges(string key, object value)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(key));

            var cursor = Context.GetEdgesCursor();
            try
            {
                var edgeIds = IndexingService.EdgeIndices.Get(key, key, value, int.MaxValue);
                foreach (var edgeId in edgeIds)
                {
                    Tuple<string, int, int> data;
                    if (!cursor.TryGetEdge(edgeId, out data)) continue;
                    var vertexOut = new GraveVertex(this, Context.VertexTable, data.Item3);
                    var vertexIn = new GraveVertex(this, Context.VertexTable, data.Item2);
                    yield return new GraveEdge(edgeId, vertexOut, vertexIn, data.Item1, this, Context.EdgesTable);
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        internal IIndexCollection GetIndices(Type indexType, bool isUserIndex)
        {
            Contract.Requires(indexType != null);
            Contract.Requires(indexType.IsAssignableFrom(typeof (IVertex)) || indexType.IsAssignableFrom(typeof (IEdge)));

            if (indexType == null)
                throw new ArgumentNullException("indexType");

            if (isUserIndex)
                return indexType == typeof (IVertex)
                           ? IndexingService.UserVertexIndices
                           : IndexingService.UserEdgeIndices;

            return indexType == typeof (IVertex) ? IndexingService.VertexIndices : IndexingService.EdgeIndices;
        }

        public override string ToString()
        {
            return this.GraphString(String.Format("vertices: {0} Edges: {1}",
                                           Context.VertexTable.GetApproximateRecordCount(30),
                                           Context.EdgesTable.GetApproximateRecordCount(30)));
        }
    }
}