using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, IEdge
    {
        private readonly IEdge _edge;

        public PartitionEdge(IEdge edge, PartitionGraph graph)
            : base(edge, graph)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new PartitionVertex(_edge.GetVertex(direction), PartitionGraph);
        }

        public string Label => _edge.Label;

        public IEdge GetBaseEdge()
        {
            return _edge;
        }
    }
}