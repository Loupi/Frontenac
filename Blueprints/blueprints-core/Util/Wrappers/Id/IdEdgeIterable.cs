using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Element> _Iterable;
        readonly IdGraph _IdGraph;
        bool _Disposed;

        public IdEdgeIterable(IEnumerable<Element> iterable, IdGraph idGraph)
        {
            _Iterable = iterable;
            _IdGraph = idGraph;
        }

        ~IdEdgeIterable()
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
                yield return new IdEdge(edge, _IdGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
