namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedEdge : WrappedElement, IEdge
    {
        public WrappedEdge(IEdge baseEdge)
            : base(baseEdge)
        {
        }

        public IVertex GetVertex(Direction direction)
        {
            return new WrappedVertex(((IEdge)BaseElement).GetVertex(direction));
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
