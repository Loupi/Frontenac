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

        public Vertex getVertex(Direction direction)
        {
            return new ReadOnlyVertex(((Edge)baseElement).getVertex(direction));
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
