using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgePropertyChangedEvent : EdgePropertyEvent
    {
        public EdgePropertyChangedEvent(Edge edge, string key, object oldValue, object newValue) :
            base(edge, key, oldValue, newValue)
        {

        }

        protected override void Fire(GraphChangedListener listener, Edge edge, string key, object oldValue, object newValue)
        {
            listener.EdgePropertyChanged(edge, key, oldValue, newValue);
        }
    }
}
