namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdge : IdElement, IEdge
    {
        public IdEdge(IEdge baseEdge, IdGraph idGraph)
            : base(baseEdge, idGraph, idGraph.GetSupportEdgeIds())
        {
        }

        public IEdge GetBaseEdge()
        {
            return (IEdge)BaseElement;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new IdVertex(((IEdge)BaseElement).GetVertex(direction), IdGraph);
        }

        public string GetLabel()
        {
            return ((IEdge)BaseElement).GetLabel();
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}
