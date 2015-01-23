using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        public GraveVertex(GraveGraph innerTinkerGraĥ, int id)
            : base(innerTinkerGraĥ, id)
        {
            Contract.Requires(innerTinkerGraĥ != null);
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return GraveInnerTinkerGraĥ.GetEdges(this, direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new NotImplementedException();
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            return Graph.AddEdge(0, this, inVertex, label);
        }

        public override void Remove()
        {
            GraveInnerTinkerGraĥ.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}