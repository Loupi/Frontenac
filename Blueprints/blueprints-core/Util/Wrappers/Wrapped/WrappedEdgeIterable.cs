using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    class WrappedEdgeIterable : ICloseableIterable<IEdge>
    {
        readonly IEnumerable<IEdge> _iterable;
        bool _disposed;

        public WrappedEdgeIterable(IEnumerable<IEdge> iterable)
        {
            Contract.Requires(iterable != null);

            _iterable = iterable;
        }

        ~WrappedEdgeIterable()
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

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new WrappedEdge(edge)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }
    }
}
