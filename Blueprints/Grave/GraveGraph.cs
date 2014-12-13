using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Grave
{
    public class GraveGraph : IndexedGraph
    {
        private readonly IGraveGraphFactory _factory;
        protected readonly EsentInstance Instance;
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

        public class GraveThreadContext : ThreadContext
        {
            public GraveThreadContext()
            {
                NewVertices = new List<int>();
                NewEdges = new List<int>();
            }

            public EsentContext Context { get; set; }
            public List<int> NewVertices { get; private set; }
            public List<int> NewEdges { get; private set; }
        }

        public GraveGraph(IGraveGraphFactory factory,
                          EsentInstance instance, 
                          IIndexingServiceFactory indexingServiceFactory)
            : base(indexingServiceFactory)
        {
            Contract.Requires(factory != null);
            Contract.Requires(instance != null);
            Contract.Requires(indexingServiceFactory != null);

            _factory = factory;
            Instance = instance;
        }

        protected override ThreadContext CreateThreadContext(IIndexingServiceFactory indexingServiceFactory)
        {
            var context = Instance.CreateContext();
            var indexing = indexingServiceFactory.Create();
            indexing.LoadFromStore(new EsentIndexStore(context));

            return new GraveThreadContext
                {
                Context = context,
                IndexingService = indexing
            };
        }

        protected override IVertex GetVertexInstance(long vertexId)
        {
            return new GraveVertex(this, EsentContext.VertexTable, (int) vertexId);
        }

        public override Features Features
        {
            get { return GraveGraphFeatures; }
        }

        public GraveThreadContext GraveContext
        {
            get { return (GraveThreadContext)Context; }
        }

        public EsentContext EsentContext
        {
            get { return GraveContext.Context; }
        }

        public override IVertex AddVertex(object unused)
        {
            var esentContext = EsentContext;
            var id = esentContext.VertexTable.AddRow();
            return new GraveVertex(this, esentContext.VertexTable, id);
        }

        public override IVertex GetVertex(object id)
        {
            IVertex result = null;
            var vertexId = id.TryToInt32();
            if (!vertexId.HasValue) return null;
            var esentContext = EsentContext;
            if (esentContext.VertexTable.SetCursor(vertexId.Value))
                result = new GraveVertex(this, esentContext.VertexTable, vertexId.Value);

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
                    yield return new GraveVertex(this, esentContext.VertexTable, id);
                    id = cursor.MoveNext();
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public override IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            var inVertexId = (int) inVertex.Id;
            var outVertexId = (int) outVertex.Id;
            var esentContext = EsentContext;
            var edgeId = esentContext.EdgesTable.AddEdge(label, inVertexId, outVertexId);
            esentContext.VertexTable.AddEdge(inVertexId, Direction.In, label, edgeId, outVertexId);
            esentContext.VertexTable.AddEdge(outVertexId, Direction.Out, label, edgeId, inVertexId);
            return new GraveEdge(edgeId, outVertex, inVertex, label, this, esentContext.EdgesTable);
        }

        public override IEdge GetEdge(object id)
        {
            var edgeId = id.TryToInt32();
            if (!edgeId.HasValue) return null;

            Tuple<string, int, int> data;
            var esentContext = EsentContext;
            return esentContext.EdgesTable.TryGetEdge(edgeId.Value, out data)
                       ? new GraveEdge(edgeId.Value, GetVertex(data.Item3), GetVertex(data.Item2), data.Item1, this,
                                       esentContext.EdgesTable)
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
                        var vertexOut = new GraveVertex(this, esentContext.VertexTable, data.Item3);
                        var vertexIn = new GraveVertex(this, esentContext.VertexTable, data.Item2);
                        yield return new GraveEdge(id, vertexOut, vertexIn, data.Item1, this, esentContext.EdgesTable);
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
                        var targetVertex = new GraveVertex(this, esentContext.VertexTable, targetId);
                        var outVertex = isVertexIn ? targetVertex : vertex;
                        var inVertex = isVertexIn ? vertex : targetVertex;
                        yield return new GraveEdge(edgeId, outVertex, inVertex, labelName, this, esentContext.EdgesTable);
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
            SetIndexedKeyValue(element, key, value);
        }

        public virtual object RemoveProperty(GraveElement element, string key)
        {
            Contract.Requires(element != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var result = element.Table.DeleteCell(element.RawId, key);
            SetIndexedKeyValue(element, key, null);
            return result;
        }

        public override void Shutdown()
        {
            _factory.Destroy(this);
            foreach (var context in Contexts.Values.OfType<GraveThreadContext>())
            {
                context.Context.Dispose();    
            }
        }

        public override string ToString()
        {
            var esentContext = EsentContext;
            return this.GraphString(String.Format("vertices: {0} Edges: {1}",
                                           esentContext.VertexTable.GetApproximateRecordCount(30),
                                           esentContext.EdgesTable.GetApproximateRecordCount(30)));
        }
    }
}