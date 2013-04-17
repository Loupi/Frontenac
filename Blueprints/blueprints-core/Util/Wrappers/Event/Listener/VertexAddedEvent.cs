using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class VertexAddedEvent : Event
    {
        readonly Vertex _Vertex;

        public VertexAddedEvent(Vertex vertex)
        {
            _Vertex = vertex;
        }

        public void FireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.VertexAdded(_Vertex);
            }
        }
    }
}
