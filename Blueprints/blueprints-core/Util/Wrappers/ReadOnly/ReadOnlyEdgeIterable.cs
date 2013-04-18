using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    class ReadOnlyEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Edge> _iterable;
        bool _disposed;

        public ReadOnlyEdgeIterable(IEnumerable<Edge> iterable)
        {
            _iterable = iterable;
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
                yield return new ReadOnlyEdge(edge);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
