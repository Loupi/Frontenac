using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Grave
{
    public class GraveGraph : IndexedGraph, IIndexStore
    {
        private readonly IGraphFactory _factory;

        private const string EdgeInPrefix = "$e_i_";
        private const string EdgeOutPrefix = "$e_o_";

        private readonly List<string> _vertexIndices = new List<string>();
        private readonly List<string> _edgeIndices = new List<string>();
        private readonly List<string> _userVertexIndices = new List<string>();
        private readonly List<string> _userEdgeIndices = new List<string>();

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

        public GraveGraph(IGraphFactory factory,
                          EsentInstance instance,
                          IndexingService indexingService,
                          IGraphConfiguration configuration)
            : base(indexingService)
        {
            Contract.Requires(factory != null);
            Contract.Requires(instance != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(configuration != null);

            _factory = factory;
            EsentContext = instance.CreateContext();
            Init(configuration);
        }

        protected readonly EsentContext EsentContext;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EsentContext.Dispose();
                _indicesLock.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override IVertex GetVertexInstance(long vertexId)
        {
            return new GraveVertex(this, (int)vertexId);
        }

        public override Features Features
        {
            get { return GraveGraphFeatures; }
        }

        private int _vertexSeed = 0;

        public override IVertex AddVertex(object unused)
        {
            var esentContext = EsentContext;
            var id = unused == null ? Interlocked.Increment(ref _vertexSeed) : unused.TryToInt32().Value;
            id = esentContext.VertexTable.AddRow(id);
            return new GraveVertex(this, id);
        }

        public override IVertex GetVertex(object id)
        {
            IVertex result = null;
            var vertexId = id.TryToInt32();
            if (!vertexId.HasValue) return null;
            var esentContext = EsentContext;
            if (esentContext.VertexTable.SetCursor(vertexId.Value))
                result = new GraveVertex(this, vertexId.Value);

            return result;
        }

        public override void RemoveVertex(IVertex vertex)
        {
            foreach (var edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            var vertexId = (int) vertex.Id;
            EsentContext.VertexTable.DeleteRow(vertexId);

            base.RemoveVertex(vertex);
        }

        public override IEnumerable<IVertex> GetVertices()
        {
            var esentContext = EsentContext;
            var cursor = esentContext.GetVerticesCursor();
            try
            {
                var id = cursor.MoveFirst();
                while (id != 0)
                {
                    yield return new GraveVertex(this, id);
                    id = cursor.MoveNext();
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        private int _edgeSeed = 0;

        public override IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            var inVertexId = (int) inVertex.Id;
            var outVertexId = (int) outVertex.Id;
            var esentContext = EsentContext;
            var id = unused == null ? Interlocked.Increment(ref _edgeSeed) : unused.TryToInt32().Value;
            var edgeId = esentContext.EdgesTable.AddEdge(id, label, inVertexId, outVertexId);
            esentContext.VertexTable.AddEdge(inVertexId, Direction.In, label, edgeId, outVertexId);
            esentContext.VertexTable.AddEdge(outVertexId, Direction.Out, label, edgeId, inVertexId);
            return new GraveEdge(edgeId, outVertex, inVertex, label, this);
        }

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt32();
            if (!edgeId.HasValue) return null;

            Tuple<string, int, int> data;
            var esentContext = EsentContext;
            return esentContext.EdgesTable.TryGetEdge(edgeId.Value, out data)
                       ? new GraveEdge(edgeId.Value, GetVertex(data.Item3), GetVertex(data.Item2), data.Item1, this)
                       : null;
        }

        public override void RemoveEdge(IEdge edge)
        {
            var edgeId = (int) edge.Id;
            var esentContext = EsentContext;
            esentContext.EdgesTable.DeleteRow(edgeId);
            var inVertexId = (int) edge.GetVertex(Direction.In).Id;
            var outVertexId = (int) edge.GetVertex(Direction.Out).Id;
            esentContext.VertexTable.DeleteEdge(inVertexId, Direction.In, edge.Label, edgeId, outVertexId);
            esentContext.VertexTable.DeleteEdge(outVertexId, Direction.Out, edge.Label, edgeId, inVertexId);

            base.RemoveEdge(edge);
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            var esentContext = EsentContext;
            var cursor = esentContext.GetEdgesCursor();
            try
            {
                var id = cursor.MoveFirst();
                while (id != 0)
                {
                    var data = cursor.GetEdgeData();
                    if (data != null)
                    {
                        var vertexOut = new GraveVertex(this, data.Item3);
                        var vertexIn = new GraveVertex(this, data.Item2);
                        yield return new GraveEdge(id, vertexOut, vertexIn, data.Item1, this);
                    }
                    id = cursor.MoveNext();
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public virtual IEnumerable<IEdge> GetEdges(GraveVertex vertex, Direction direction, params string[] labels)
        {
            var esentContext = EsentContext;
            var cursor = esentContext.GetVerticesCursor();
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
                        var targetVertex = new GraveVertex(this, targetId);
                        var outVertex = isVertexIn ? targetVertex : vertex;
                        var inVertex = isVertexIn ? vertex : targetVertex;
                        yield return new GraveEdge(edgeId, outVertex, inVertex, labelName, this);
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

            var table = element is IVertex
                            ? (EsentTable)EsentContext.VertexTable
                            : EsentContext.EdgesTable;

            return table.ReadCell(element.RawId, key);
        }

        public virtual IEnumerable<string> GetPropertyKeys(GraveElement element)
        {
            Contract.Requires(element != null);

            var table = element is IVertex
                            ? (EsentTable)EsentContext.VertexTable
                            : EsentContext.EdgesTable;

            return table.GetColumnsForRow(element.RawId).Where(t => !t.StartsWith("$"));
        }

        public virtual void SetProperty(GraveElement element, string key, object value)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var table = element is IVertex
                            ? (EsentTable)EsentContext.VertexTable
                            : EsentContext.EdgesTable;

            table.WriteCell(element.RawId, key, value);
            SetIndexedKeyValue(element, key, value);
        }

        public virtual object RemoveProperty(GraveElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var table = element is IVertex
                            ? (EsentTable)EsentContext.VertexTable
                            : EsentContext.EdgesTable;

            var result = table.DeleteCell(element.RawId, key);
            SetIndexedKeyValue(element, key, null);
            return result;
        }

        public override void Shutdown()
        {
            EsentContext.Dispose();
            _factory.Destroy(this);
        }

        public override string ToString()
        {
            var esentContext = EsentContext;
            return this.GraphString(String.Format("vertices: {0} Edges: {1}",
                                           esentContext.VertexTable.GetApproximateRecordCount(30),
                                           esentContext.EdgesTable.GetApproximateRecordCount(30)));
        }

        public const int ConfigVertexId = 1;
        private readonly ReaderWriterLockSlim _indicesLock = new ReaderWriterLockSlim();

        public void LoadIndices()
        {
            if (!EsentContext.ConfigTable.SetCursor(ConfigVertexId))
                EsentContext.ConfigTable.AddRow(1);
        }

        List<string> GetIndexList(string indexColumn)
        {
            List<string> indices;
            switch (indexColumn)
            {
                case "VertexIndices":
                    indices = _vertexIndices;
                    break;
                case "EdgeIndices":
                    indices = _edgeIndices;
                    break;
                case "UserVertexIndices":
                    indices = _userVertexIndices;
                    break;
                case "UserEdgeIndices":
                    indices = _userEdgeIndices;
                    break;

                default:
                    throw new NotSupportedException(indexColumn);
            }

            return indices;
        }

        public void CreateIndex(string indexName, string indexColumn)
        {
            var indices = GetIndices(indexColumn);
            _indicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName)) return;
                indices.Add(indexName);
                EsentContext.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
            }
            finally
            {
                _indicesLock.ExitWriteLock();
            }
        }

        public List<string> GetIndices(string indexType)
        {
            return EsentContext.ConfigTable.ReadCell(ConfigVertexId, indexType) as List<string> ?? new List<string>();
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            long result;
            var indices = GetIndices(indexColumn);
            _indicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName))
                {
                    indices.Remove(indexName);
                    EsentContext.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
                    result = indexingService.DeleteIndex(indexType, indexName, isUserIndex);
                }
                else
                    result = -1;
            }
            finally
            {
                _indicesLock.ExitWriteLock();
            }

            return result;
        }
    }
}