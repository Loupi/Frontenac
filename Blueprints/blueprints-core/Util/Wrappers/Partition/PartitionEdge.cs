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

        public Vertex GetVertex(Direction direction)
        {
            return new PartitionVertex(((Edge)_BaseElement).GetVertex(direction), _Graph);
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
