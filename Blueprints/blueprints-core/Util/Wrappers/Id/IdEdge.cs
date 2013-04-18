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
            : base(baseEdge, idGraph, idGraph.getSupportEdgeIds())
        {
        }

        public Edge getBaseEdge()
        {
            return (Edge)baseElement;
        }

        public Vertex getVertex(Direction direction)
        {
            return new IdVertex(((Edge)baseElement).getVertex(direction), idGraph);
        }

        public string getLabel()
        {
            return ((Edge)baseElement).getLabel();
        }

        public override string ToString()
        {
            return StringFactory.edgeString(this);
        }
    }
}
