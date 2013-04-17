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
        readonly string _Label;
        readonly Vertex _InVertex;
        readonly Vertex _OutVertex;

        public TinkerEdge(string id, Vertex outVertex, Vertex inVertex, string label, TinkerGraph graph)
            : base(id, graph)
        {
            _Label = label;
            _OutVertex = outVertex;
            _InVertex = inVertex;
            _Graph._EdgeKeyIndex.AutoUpdate(StringFactory.LABEL, _Label, null, this);
        }

        public string GetLabel()
        {
            return _Label;
        }

        public Vertex GetVertex(Direction direction)
        {
            if (direction == Direction.IN)
                return _InVertex;
            else if (direction == Direction.OUT)
                return _OutVertex;
            else
                throw ExceptionFactory.BothIsNotSupported();
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}
