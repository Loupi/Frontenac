namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        public ReadOnlyEdge(IEdge baseEdge)
            : base(baseEdge)
        {
        }

        public IVertex GetVertex(Direction direction)
        {
            return new ReadOnlyVertex(((IEdge)BaseElement).GetVertex(direction));
        }

        public string Label
        {
            get { return ((IEdge)BaseElement).Label; }
        }

        public IEdge GetBaseEdge()
        {
            return (IEdge)BaseElement;
        }
    }
}
