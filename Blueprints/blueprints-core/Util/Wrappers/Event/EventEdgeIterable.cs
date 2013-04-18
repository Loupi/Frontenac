using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// A sequence of edges that applies the list of listeners into each edge.
    /// </summary>
    class EventEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Edge> _iterable;
        readonly EventGraph _eventGraph;
        bool _disposed;

        public EventEdgeIterable(IEnumerable<Edge> iterable, EventGraph eventGraph)
        {
            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        ~EventEdgeIterable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_iterable is IDisposable)
                    (_iterable as IDisposable).Dispose();
            }

            _disposed = true;
        }

        public IEnumerator<Edge> GetEnumerator()
        {
            foreach (Edge edge in _iterable)
                yield return new EventEdge(edge, _eventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
