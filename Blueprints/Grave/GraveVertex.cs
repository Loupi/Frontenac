using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Grave.Esent;

namespace Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        private const string EdgeInPrefix = "$e_i_";
        private const string EdgeOutPrefix = "$e_o_";

        public GraveVertex(GraveGraph graph, EsentTable vertexTable, int id)
            : base(graph, vertexTable, id)
        {
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            return Graph.AddEdge(0, this, inVertex, label);
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            var cursor = Graph.Context.GetVerticesCursor();
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
                    foreach (var edgeData in cursor.GetEdges(RawId, label))
                    {
                        var edgeId = (int) (edgeData >> 32);
                        var targetId = (int) (edgeData & 0xFFFF);
                        var vertex = new GraveVertex(Graph, Graph.Context.VertexTable, targetId);
                        var outVertex = isVertexIn ? vertex : this;
                        var inVertex = isVertexIn ? this : vertex;
                        yield return
                            new GraveEdge(edgeId, outVertex, inVertex, labelName, Graph, Graph.Context.EdgesTable);
                    }
                }
            }
            finally
            {
                cursor.Close();
            }
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<string> FilterLabels(Direction direction, ICollection<string> labels, Direction directionFilter,
                                                        IEnumerable<string> columns, string prefix)
        {
            Contract.Requires(labels != null);
            Contract.Requires(columns != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(prefix));

            if (direction == directionFilter || direction == Direction.Both)
            {
                if (!labels.Any())
                    return columns.Where(t => t.StartsWith(prefix));

                var labelsFilter = labels.Select(t => string.Format("{0}{1}", prefix, t)).ToArray();
                return columns.Where(labelsFilter.Contains);
            }
            return Enumerable.Empty<string>();
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}