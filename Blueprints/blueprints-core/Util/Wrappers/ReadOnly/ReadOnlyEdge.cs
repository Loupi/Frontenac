using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, Edge
    {
        public ReadOnlyEdge(Edge baseEdge)
            : base(baseEdge)
        {
        }

        public Vertex GetVertex(Direction direction)
        {
            return new ReadOnlyVertex(((Edge)_BaseElement).GetVertex(direction));
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
