using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Element> _iterable;
        readonly IdGraph _idGraph;
        bool _disposed;

        public IdVertexIterable(IEnumerable<Element> iterable, IdGraph idGraph)
        {
            _iterable = iterable;
            _idGraph = idGraph;
        }

        ~IdVertexIterable()
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
                yield return new IdVertex(v, _idGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
