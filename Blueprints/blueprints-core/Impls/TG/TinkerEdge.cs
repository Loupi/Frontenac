using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    class TinkerEdge : TinkerElement, Edge
    {
        readonly string _label;
        readonly Vertex _inVertex;
        readonly Vertex _outVertex;

        public TinkerEdge(string id, Vertex outVertex, Vertex inVertex, string label, TinkerGraph graph)
            : base(id, graph)
        {
            _label = label;
            _outVertex = outVertex;
            _inVertex = inVertex;
            base.graph.edgeKeyIndex.autoUpdate(StringFactory.LABEL, _label, null, this);
        }

        public string getLabel()
        {
            return _label;
        }

        public Vertex getVertex(Direction direction)
        {
            if (direction == Direction.IN)
                return _inVertex;
            else if (direction == Direction.OUT)
                return _outVertex;
            else
                throw ExceptionFactory.bothIsNotSupported();
        }

        public override string ToString()
        {
            return StringFactory.edgeString(this);
        }
    }
}
