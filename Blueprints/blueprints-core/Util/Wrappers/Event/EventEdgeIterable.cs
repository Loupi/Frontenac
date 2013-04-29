using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// A sequence of edges that applies the list of listeners into each edge.
    /// </summary>
    public class EventEdgeIterable : ICloseableIterable<IEdge>
    {
        readonly IEnumerable<IEdge> _iterable;
        readonly EventGraph _eventGraph;
        bool _disposed;

        public EventEdgeIterable(IEnumerable<IEdge> iterable, EventGraph eventGraph)
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

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new EventEdge(edge, _eventGraph)).Cast<IEdge>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }
    }
}
