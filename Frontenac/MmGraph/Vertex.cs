using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace MmGraph
{
    public class Vertex : Element, IVertex
    {
        public Vertex(long id, Graph graph) 
            : base(id, graph)
        {

        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return RawGraph.GetEdges(this, direction, labels);
        }

        public long GetNbEdges(Direction direction, string label)
        {
            var labels = label == null 
                ? new[] { label } 
                : new string[0];

            return GetEdges(direction, labels).Count();
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, string label, params object[] ids)
        {
            var vertices = GetVertices(direction, new[] {label});

            if (ids.Length <= 0)
                return vertices;

            var typedIds = ids
                .Cast<long>()
                .ToList();

            vertices = vertices.Where(v => typedIds.Contains((long)v.Id));

            return vertices;
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            throw new System.NotImplementedException();
        }

        public IEdge AddEdge(object id, string label, IVertex inVertex)
        {
            return RawGraph.AddEdge(null, this, inVertex, label);
        }

        public override object GetProperty(string key)
        {
            return RawGraph.GetVertexProperty(RawId, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return RawGraph.GetVertexPropertyKeys(RawId);
        }

        public override void SetProperty(string key, object value)
        {
            RawGraph.SetVertexProperty(RawId, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return RawGraph.RemoveVertexProperty(RawId, key);
        }

        public override void Remove()
        {
            RawGraph.RemoveVertex(this);
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}
