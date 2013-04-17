using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdge : IdElement, Edge
    {
        public IdEdge(Edge baseEdge, IdGraph idGraph)
            : base(baseEdge, idGraph, idGraph.GetSupportEdgeIds())
        {
        }

        public Edge GetBaseEdge()
        {
            return (Edge)_BaseElement;
        }

        public Vertex GetVertex(Direction direction)
        {
            return new IdVertex(((Edge)_BaseElement).GetVertex(direction), _IdGraph);
        }

        public string GetLabel()
        {
            return ((Edge)_BaseElement).GetLabel();
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}
