using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.BlueRed
{
    public class RedisVertex : RedisElement, IVertex
    {
        public RedisVertex(long id, RedisGraph innerTinkerGraĥ) 
            : base(id, innerTinkerGraĥ)
        {
            Contract.Requires(innerTinkerGraĥ != null);
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return RedisInnerTinkerGraĥ.GetEdges(this, direction, labels);
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
            RedisInnerTinkerGraĥ.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}
