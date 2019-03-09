using System;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public class GraveEdge : GraveElement, IEdge
    {
        private readonly IVertex _inVertex;
        private readonly IVertex _outVertex;

        public GraveEdge(int id, IVertex outVertex, IVertex inVertex, string label, GraveGraph graph)
            : base(graph, id)
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _outVertex = outVertex;
            _inVertex = inVertex;
            Label = label;
        }

        public override void Remove()
        {
            GraveGraph.RemoveEdge(this);
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return direction == Direction.In ? _inVertex : _outVertex;
        }

        public string Label { get; }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}