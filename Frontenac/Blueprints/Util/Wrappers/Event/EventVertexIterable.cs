using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     A sequence of vertices that applies the list of listeners into each vertex.
    /// </summary>
    public class EventVertexIterable : ICloseableIterable<IVertex>
    {
        private readonly EventGraph _eventGraph;
        private readonly IEnumerable<IVertex> _iterable;
        private bool _disposed;

        public EventVertexIterable(IEnumerable<IVertex> iterable, EventGraph eventGraph)
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

        public IEnumerator<IVertex> GetEnumerator()
        {
            return _iterable.Select(v => new EventVertex(v, _eventGraph)).Cast<IVertex>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }

        ~EventVertexIterable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                (_iterable as IDisposable)?.Dispose();
            }

            _disposed = true;
        }
    }
}