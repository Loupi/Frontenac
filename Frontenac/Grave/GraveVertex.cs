using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        public GraveVertex(GraveGraph graph, int id)
            : base(graph, id)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return GraveGraph.GetEdges(this, direction, labels);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return GetEdges(direction, label).Count();
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            return ids.Length == 0 
                ? GetVertices(direction, new[] {label}) 
                : GetEdges(direction, label)
                    .Where(edge => ids.Contains(edge.GetVertex(direction).Id))
                    .Select(edge => edge.GetVertex(direction));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new NotSupportedException();
        }

        public IEdge AddEdge(object id, string label, IVertex inVertex)
        {
            VertexContract.ValidateAddEdge(id, label, inVertex);

            return Graph.AddEdge(id, this, inVertex, label);
        }

        public override void Remove()
        {
            GraveGraph.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}