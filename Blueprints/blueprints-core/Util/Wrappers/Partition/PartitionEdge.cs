namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, IEdge
    {
        public PartitionEdge(IEdge baseEdge, PartitionGraph graph)
            : base(baseEdge, graph)
        {
        }

        public IVertex GetVertex(Direction direction)
        {
            return new PartitionVertex(((IEdge)BaseElement).GetVertex(direction), Graph);
        }

        public string GetLabel()
        {
            return ((IEdge)BaseElement).GetLabel();
        }

        public IEdge GetBaseEdge()
        {
            return (IEdge)BaseElement;
        }
    }
}
