using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public ReadOnlyEdge(ReadOnlyGraph graph, IEdge baseEdge)
            : base(graph, baseEdge)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (baseEdge == null)
                throw new ArgumentNullException(nameof(baseEdge));

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new ReadOnlyVertex(ReadOnlyGraph, _baseEdge.GetVertex(direction));
        }

        public string Label => _baseEdge.Label;

        public IEdge GetBaseEdge()
        {
            return _baseEdge;
        }
    }
}