using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        public GraveVertex(GraveGraph innerTinkerGrapĥ, int id)
            : base(innerTinkerGrapĥ, id)
        {
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return GraveInnerTinkerGrapĥ.GetEdges(this, direction, labels);
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
            GraveInnerTinkerGrapĥ.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}