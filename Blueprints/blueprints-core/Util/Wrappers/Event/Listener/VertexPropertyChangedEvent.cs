using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event that fires when a property changes on a vertex. 
    /// </summary>
    public class VertexPropertyChangedEvent : VertexPropertyEvent
    {
        public VertexPropertyChangedEvent(Vertex vertex, string key, object oldValue, object newValue)
            : base(vertex, key, oldValue, newValue)
        {

        }

        protected override void fire(GraphChangedListener listener, Vertex vertex, string key, object oldValue, object newValue)
        {
            listener.vertexPropertyChanged(vertex, key, oldValue, newValue);
        }
    }
}
