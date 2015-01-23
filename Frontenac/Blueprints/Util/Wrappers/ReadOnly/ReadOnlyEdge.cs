﻿using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public ReadOnlyEdge(ReadOnlyGraph innerTinkerGrapĥ, IEdge baseEdge)
            : base(innerTinkerGrapĥ, baseEdge)
        {
            Contract.Requires(innerTinkerGrapĥ != null);
            Contract.Requires(baseEdge != null);

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            return new ReadOnlyVertex(ReadOnlyInnerTinkerGrapĥ, _baseEdge.GetVertex(direction));
        }

        public string Label
        {
            get { return _baseEdge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return _baseEdge;
        }
    }
}