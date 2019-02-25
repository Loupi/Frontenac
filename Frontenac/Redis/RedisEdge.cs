using System;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));

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
            EdgeContract.ValidateGetVertex(direction);
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
