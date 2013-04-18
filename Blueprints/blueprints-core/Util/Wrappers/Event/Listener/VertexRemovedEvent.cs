using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Event fired when a vertex is removed.
    /// </summary>
    public class VertexRemovedEvent : Event
    {
        readonly Vertex _vertex;
        private readonly IDictionary<string, object> _props;

        public VertexRemovedEvent(Vertex vertex, IDictionary<string, object> props)
        {
            _vertex = vertex;
            _props = props;
        }

        public void fireEvent(IEnumerator<GraphChangedListener> eventListeners)
        {
            while (eventListeners.MoveNext())
            {
                eventListeners.Current.vertexRemoved(_vertex, _props);
            }
        }
    }
}
