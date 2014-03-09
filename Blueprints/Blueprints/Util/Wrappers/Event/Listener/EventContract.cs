using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    [ContractClassFor(typeof (IEvent))]
    public abstract class EventContract : IEvent
    {
        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            Contract.Requires(eventListeners != null);
        }
    }
}