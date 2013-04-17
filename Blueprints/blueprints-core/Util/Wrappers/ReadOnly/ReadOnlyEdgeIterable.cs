using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    class ReadOnlyEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Edge> _Iterable;
        bool _Disposed;

        public ReadOnlyEdgeIterable(IEnumerable<Edge> iterable)
        {
            _Iterable = iterable;
        }

        ~ReadOnlyEdgeIterable()
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
                yield return new ReadOnlyEdge(edge);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
