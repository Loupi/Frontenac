using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     A sequence of edges that applies the list of listeners into each edge.
    /// </summary>
    public class EventEdgeIterable : ICloseableIterable<IEdge>
    {
        private readonly EventGraph _eventGraph;
        private readonly IEnumerable<IEdge> _iterable;
        private bool _disposed;

        public EventEdgeIterable(IEnumerable<IEdge> iterable, EventGraph eventGraph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(eventGraph != null);

            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new EventEdge(edge, _eventGraph)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }

        ~EventEdgeIterable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                var iterable = _iterable as IDisposable;
                iterable?.Dispose();
            }

            _disposed = true;
        }
    }
}