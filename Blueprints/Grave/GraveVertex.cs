using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    public class GraveVertex : GraveElement, IVertex
    {
        public GraveVertex(GraveGraph graph, EsentTable vertexTable, int id)
            : base(graph, vertexTable, id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(vertexTable != null);
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            return Graph.AddEdge(0, this, inVertex, label);
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return Graph.GetEdges(this, direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}