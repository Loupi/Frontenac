using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.Redis
{
    public class RedisEdge : RedisElement, IEdge
    {
        private readonly IVertex _inVertex;
        private readonly string _label;
        private readonly IVertex _outVertex;

        public RedisEdge(long id, IVertex outVertex, IVertex inVertex, string label, RedisGraph innerTinkerGrapĥ)
            : base(id, innerTinkerGrapĥ)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(innerTinkerGrapĥ != null);

            _outVertex = outVertex;
            _inVertex = inVertex;
            _label = label;
        }

        public override void Remove()
        {
            RedisInnerTinkerGrapĥ.RemoveEdge(this);
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
