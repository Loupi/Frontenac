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
        readonly IEnumerable<Vertex> _Iterable;
        readonly EventGraph _EventGraph;
        bool _Disposed;

        public EventVertexIterable(IEnumerable<Vertex> iterable, EventGraph eventGraph)
        {
            _Iterable = iterable;
            _EventGraph = eventGraph;
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
            if (_Disposed)
                return;

            if (disposing)
            {
                if (_Iterable is IDisposable)
                    (_Iterable as IDisposable).Dispose();
            }

            _Disposed = true;
        }

        public IEnumerator<Vertex> GetEnumerator()
        {
            foreach (Vertex v in _Iterable)
                yield return new EventVertex(v, _EventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
