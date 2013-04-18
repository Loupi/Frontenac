using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Element> _iterable;
        readonly IdGraph _idGraph;
        bool _disposed;

        public IdEdgeIterable(IEnumerable<Element> iterable, IdGraph idGraph)
        {
            _iterable = iterable;
            _idGraph = idGraph;
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
                yield return new IdEdge(edge, _idGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
