using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    [ContractClassFor(typeof (EdgePropertyEvent))]
    public abstract class EdgePropertyEventContract : EdgePropertyEvent
    {
        protected EdgePropertyEventContract(IEdge edge, string key, object oldValue, object newValue)
            : base(edge, key, oldValue, newValue)
        {
        }

        protected override void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue)
        {
            Contract.Requires(listener != null);
            Contract.Requires(edge != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }
    }
}