using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public class GraveEdge : GraveElement, IEdge
    {
        private readonly IVertex _inVertex;
        private readonly string _label;
        private readonly IVertex _outVertex;

        public GraveEdge(int id, IVertex outVertex, IVertex inVertex, string label, GraveGraph innerTinkerGraĥ)
            : base(innerTinkerGraĥ, id)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(innerTinkerGraĥ != null);

            _outVertex = outVertex;
            _inVertex = inVertex;
            _label = label;
        }

        public override void Remove()
        {
            GraveInnerTinkerGraĥ.RemoveEdge(this);
        }

        public IVertex GetVertex(Direction direction)
        {
            return direction == Direction.In ? _inVertex : _outVertex;
        }

        public string Label
        {
            get { return _label; }
        }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}