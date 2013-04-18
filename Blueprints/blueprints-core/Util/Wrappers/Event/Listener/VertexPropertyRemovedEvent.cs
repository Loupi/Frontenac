using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event fired when a vertex property is removed. 
    /// </summary>
    public class VertexPropertyRemovedEvent : VertexPropertyEvent
    {
        public VertexPropertyRemovedEvent(Vertex vertex, string key, object removedValue)
            : base(vertex, key, removedValue, null)
        {

        }

        protected override void fire(GraphChangedListener listener, Vertex vertex, string key, object oldValue, object newValue)
        {
            listener.vertexPropertyRemoved(vertex, key, oldValue);
        }
    }
}
