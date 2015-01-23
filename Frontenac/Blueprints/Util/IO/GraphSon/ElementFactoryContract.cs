using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [ContractClassFor(typeof (IElementFactory))]
    public abstract class ElementFactoryContract : IElementFactory
    {
        public IEdge CreateEdge(object id, IVertex out_, IVertex in_, string label)
        {
            Contract.Requires(out_ != null);
            Contract.Requires(in_ != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return null;
        }

        public IVertex CreateVertex(object id)
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return null;
        }
    }
}