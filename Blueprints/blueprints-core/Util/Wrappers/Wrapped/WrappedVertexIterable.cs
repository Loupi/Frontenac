using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    class WrappedVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Vertex> _iterable;
        bool _disposed;

        public WrappedVertexIterable(IEnumerable<Vertex> iterable)
        {
            _iterable = iterable;
        }

        ~WrappedVertexIterable()
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
                yield return new WrappedVertex(v);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
