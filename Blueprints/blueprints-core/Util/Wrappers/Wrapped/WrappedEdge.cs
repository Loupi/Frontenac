using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedEdge : WrappedElement, Edge
    {
        public WrappedEdge(Edge baseEdge)
            : base(baseEdge)
        {
        }

        public Vertex GetVertex(Direction direction)
        {
            return new WrappedVertex(((Edge)_BaseElement).GetVertex(direction));
        }

        public string GetLabel()
        {
            return ((Edge)_BaseElement).GetLabel();
        }

        public Edge GetBaseEdge()
        {
            return (Edge)_BaseElement;
        }
    }
}
