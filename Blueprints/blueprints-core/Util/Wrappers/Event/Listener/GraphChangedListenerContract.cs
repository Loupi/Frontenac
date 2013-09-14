using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    [ContractClassFor(typeof (IGraphChangedListener))]
    public abstract class GraphChangedListenerContract : IGraphChangedListener
    {
        public void VertexAdded(IVertex vertex)
        {
            Contract.Requires(vertex != null);
        }

        public void VertexPropertyChanged(IVertex vertex, string key, object oldValue, object setValue)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public void VertexPropertyRemoved(IVertex vertex, string key, object removedValue)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public void VertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(props != null);
        }

        public void EdgeAdded(IEdge edge)
        {
            Contract.Requires(edge != null);
        }

        public void EdgePropertyChanged(IEdge edge, string key, object oldValue, object setValue)
        {
            Contract.Requires(edge != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public void EdgePropertyRemoved(IEdge edge, string key, object removedValue)
        {
            Contract.Requires(edge != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public void EdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            Contract.Requires(edge != null);
            Contract.Requires(props != null);
        }
    }
}