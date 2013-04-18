using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, Edge
    {
        public PartitionEdge(Edge baseEdge, PartitionGraph graph)
            : base(baseEdge, graph)
        {
        }

        public Vertex getVertex(Direction direction)
        {
            return new PartitionVertex(((Edge)baseElement).getVertex(direction), graph);
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
