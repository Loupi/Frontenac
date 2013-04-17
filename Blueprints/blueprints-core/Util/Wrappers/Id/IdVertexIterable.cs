using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Element> _Iterable;
        readonly IdGraph _IdGraph;
        bool _Disposed;

        public IdVertexIterable(IEnumerable<Element> iterable, IdGraph idGraph)
        {
            _Iterable = iterable;
            _IdGraph = idGraph;
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
                yield return new IdVertex(v, _IdGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
