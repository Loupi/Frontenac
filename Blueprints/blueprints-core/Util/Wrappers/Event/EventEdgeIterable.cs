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
        readonly IEnumerable<Edge> _Iterable;
        readonly EventGraph _EventGraph;
        bool _Disposed;

        public EventEdgeIterable(IEnumerable<Edge> iterable, EventGraph eventGraph)
        {
            _Iterable = iterable;
            _EventGraph = eventGraph;
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
            if (_Disposed)
                return;

            if (disposing)
            {
                if (_Iterable is IDisposable)
                    (_Iterable as IDisposable).Dispose();
            }

            _Disposed = true;
        }

        public IEnumerator<Edge> GetEnumerator()
        {
            foreach (Edge edge in _Iterable)
                yield return new EventEdge(edge, _EventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
