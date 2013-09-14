using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    [ContractClassFor(typeof (VertexPropertyEvent))]
    public abstract class VertexPropertyEventContract : VertexPropertyEvent
    {
        protected VertexPropertyEventContract(IVertex vertex, string key, object oldValue, object newValue)
            : base(vertex, key, oldValue, newValue)
        {
        }

        protected override void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue)
        {
            Contract.Requires(listener != null);
            Contract.Requires(vertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }
    }
}