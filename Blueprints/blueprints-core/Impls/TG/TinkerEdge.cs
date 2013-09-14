using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerEdge : TinkerElement, IEdge
    {
        private readonly IVertex _inVertex;
        private readonly IVertex _outVertex;

        public TinkerEdge(string id, IVertex outVertex, IVertex inVertex, string label, TinkerGraph graph)
            : base(id, graph)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(graph != null);

            Label = label;
            _outVertex = outVertex;
            _inVertex = inVertex;
            Graph.EdgeKeyIndex.AutoUpdate(StringFactory.Label, Label, null, this);
        }

        public string Label { get; protected set; }

        public IVertex GetVertex(Direction direction)
        {
            if (direction == Direction.In)
                return _inVertex;
            if (direction == Direction.Out)
                return _outVertex;
            throw ExceptionFactory.BothIsNotSupported();
        }

        public override string ToString()
        {
            return StringFactory.EdgeString(this);
        }
    }
}