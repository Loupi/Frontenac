using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace MmGraph
{
    public class Edge : Element, IEdge
    {
        private readonly Vertex _outVertex;
        private readonly Vertex _inVertex;

        public Edge(long id, string label, Vertex outVertex, Vertex inVertex, Graph graph) 
            : base(id, graph)
        {
            _outVertex = outVertex;
            _inVertex = inVertex;
            Label = label;
        }

        public string Label { get; }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return direction == Direction.In ? _inVertex : _outVertex;
        }
        
        public override object GetProperty(string key)
        {
            return RawGraph.GetEdgeProperty(RawId, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return RawGraph.GetEdgePropertyKeys(RawId);
        }

        public override void SetProperty(string key, object value)
        {
            RawGraph.SetEdgeProperty(RawId, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return RawGraph.RemoveEdgeProperty(RawId, key);
        }

        public override void Remove()
        {
            RawGraph.RemoveEdge(this);
        }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}