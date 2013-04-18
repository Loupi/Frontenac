using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// A sequence of vertices that applies the list of listeners into each vertex.
    /// </summary>
    class EventVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Vertex> _iterable;
        readonly EventGraph _eventGraph;
        bool _disposed;

        public EventVertexIterable(IEnumerable<Vertex> iterable, EventGraph eventGraph)
        {
            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        ~EventVertexIterable()
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

        public IEnumerator<Vertex> GetEnumerator()
        {
            foreach (Vertex v in _iterable)
                yield return new EventVertex(v, _eventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
