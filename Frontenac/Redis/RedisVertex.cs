using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Redis
{
    public class RedisVertex : RedisElement, IVertex
    {        
        public RedisVertex(long id, RedisGraph graph) 
            : base(id, graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return RedisGraph.GetEdges(this, direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            return RedisGraph.GetNbEdges(this, direction, label);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[]ids)
        {
            return ids.Length == 0 
                ? GetVertices(direction, new[] {label}) 
                : RedisGraph.GetVertices(this, direction, label, ids);
        }

        public IVertexQuery Query()
        {
            throw new NotImplementedException();
        }

        public IEdge AddEdge(object id, string label, IVertex inVertex)
        {
            VertexContract.ValidateAddEdge(id, label, inVertex);

            return Graph.AddEdge(id, this, inVertex, label);
        }

        public override void Remove()
        {
            RedisGraph.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}
