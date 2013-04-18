using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// Interface for a listener to EventGraph change events.
    /// 
    /// Implementations of this interface should be added to the list of listeners on the addListener method on
    /// the EventGraph.
    /// </summary>
    public interface GraphChangedListener
    {
        /// <summary>
        /// Raised when a new Vertex is added.
        /// </summary>
        /// <param name="vertex">the vertex that was added</param>
        void vertexAdded(Vertex vertex);

        /// <summary>
        /// Raised after the property of a vertex changed.
        /// </summary>
        /// <param name="vertex">the vertex that changed</param>
        /// <param name="key">the key of the property that changed</param>
        /// <param name="oldValue">the old value of the property</param>
        /// <param name="setValue">the new value of the property</param>
        void vertexPropertyChanged(Vertex vertex, string key, object oldValue, object setValue);

        /// <summary>
        /// Raised after a vertex property was removed.
        /// </summary>
        /// <param name="vertex">the vertex that changed</param>
        /// <param name="key">the key that was removed</param>
        /// <param name="removedValue">the value of the property that was removed</param>
        void vertexPropertyRemoved(Vertex vertex, string key, object removedValue);

        /// <summary>
        /// Raised after a vertex was removed from the graph.
        /// </summary>
        /// <param name="vertex">the vertex that was removed</param>
        void vertexRemoved(Vertex vertex, IDictionary<string, object> props);

        /// <summary>
        /// Raised after a new edge is added.
        /// </summary>
        /// <param name="Edge">the edge that was added</param>
        void edgeAdded(Edge edge);

        /// <summary>
        /// Raised after the property of a edge changed.
        /// </summary>
        /// <param name="edge">the edge that changed</param>
        /// <param name="key">the key of the property that changed</param>
        /// <param name="oldValue">the old value of the property</param>
        /// <param name="setValue">the new value of the property</param>
        void edgePropertyChanged(Edge edge, string key, object oldValue, object setValue);

        /// <summary>
        /// Raised after an edge property was removed.
        /// </summary>
        /// <param name="edge">the edge that changed</param>
        /// <param name="key">the key that was removed</param>
        /// <param name="removedValue">the value of the property that was removed</param>
        void edgePropertyRemoved(Edge edge, string key, object removedValue);

        /// <summary>
        /// Raised after an edge was removed from the graph.
        /// </summary>
        /// <param name="edge">the edge that was removed</param>
        void edgeRemoved(Edge edge, IDictionary<string, object> props);
    }
}
