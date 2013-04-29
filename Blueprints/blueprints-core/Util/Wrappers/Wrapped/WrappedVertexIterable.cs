using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    class WrappedVertexIterable : ICloseableIterable<IVertex>
    {
        readonly IEnumerable<IVertex> _iterable;
        bool _disposed;

        public WrappedVertexIterable(IEnumerable<IVertex> iterable)
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

        public IEnumerator<IVertex> GetEnumerator()
        {
            return _iterable.Select(v => new WrappedVertex(v)).Cast<IVertex>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }
    }
}
