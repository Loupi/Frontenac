using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgePropertyRemovedEvent : EdgePropertyEvent
    {
        public EdgePropertyRemovedEvent(Edge vertex, string key, object oldValue)
            : base(vertex, key, oldValue, null)
        {

        }

        protected override void fire(GraphChangedListener listener, Edge edge, string key, object oldValue, object newValue)
        {
            listener.edgePropertyRemoved(edge, key, oldValue);
        }
    }
}
