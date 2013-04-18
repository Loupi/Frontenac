using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class VertexRemovedEvent : Event
    {
        readonly Vertex _Vertex;

        public VertexRemovedEvent(Vertex vertex)
        {
            _Vertex = vertex;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.vertexRemoved(_Vertex);
            }
        }
    }
}
