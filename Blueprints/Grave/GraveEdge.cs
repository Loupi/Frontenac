using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Grave.Esent;

namespace Grave
{
    public class GraveEdge : GraveElement, IEdge
    {
        readonly string _label;
        readonly IVertex _inVertex;
        readonly IVertex _outVertex;

        public GraveEdge(int id, IVertex outVertex, IVertex inVertex, string label,  GraveGraph graph, EsentTable table)
            : base(graph, table, id)
        {
            _outVertex = outVertex;
            _inVertex = inVertex;
            _label = label;
        }

        public IVertex GetVertex(Direction direction)
        {
            if (direction == Direction.In)
                return _inVertex;
            if (direction == Direction.Out)
                return _outVertex;
            throw ExceptionFactory.BothIsNotSupported();
        }

        public string Label
        {
            get { return _label; }
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}
