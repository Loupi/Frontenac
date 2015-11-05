using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        public GraveVertex(GraveGraph innerTinkerGrapĥ, int id)
            : base(innerTinkerGrapĥ, id)
        {
            Contract.Requires(innerTinkerGrapĥ != null);
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
            return GetEdges(direction, label).Where(edge => ids.Contains(edge.GetVertex(direction).Id)).Select(edge => edge.GetVertex(direction));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new NotImplementedException();
        }

        public IEdge AddEdge(object id, string label, IVertex inVertex)
        {
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