using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, IEdge
    {
        private readonly IEdge _edge;

        public PartitionEdge(IEdge edge, PartitionGraph graph)
            : base(edge, graph)
        {
            Contract.Requires(edge != null);
            Contract.Requires(graph != null);

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new PartitionVertex(_edge.GetVertex(direction), Graph);
        }

        public string Label
        {
            get { return _edge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return _edge;
        }
    }
}