using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.Redis
{
    public class RedisVertex : RedisElement, IVertex
    {        
        public RedisVertex(long id, RedisGraph innerTinkerGrapĥ) 
            : base(id, innerTinkerGrapĥ)
        {
            Contract.Requires(innerTinkerGrapĥ != null);
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return RedisInnerTinkerGrapĥ.GetEdges(this, direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return RedisInnerTinkerGrapĥ.GetNbEdges(this, direction, label);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[]ids)
        {
            return RedisInnerTinkerGrapĥ.GetVertices(this, direction, label, ids);
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
            RedisInnerTinkerGrapĥ.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}
