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

        public Vertex getVertex(Direction direction)
        {
            return new WrappedVertex(((Edge)baseElement).getVertex(direction));
        }

        public string getLabel()
        {
            return ((Edge)baseElement).getLabel();
        }

        public Edge getBaseEdge()
        {
            return (Edge)baseElement;
        }
    }
}
