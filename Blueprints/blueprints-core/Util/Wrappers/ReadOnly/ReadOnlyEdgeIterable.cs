using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    internal class ReadOnlyEdgeIterable : ICloseableIterable<IEdge>
    {
        private readonly IEnumerable<IEdge> _iterable;
        private bool _disposed;

        public ReadOnlyEdgeIterable(IEnumerable<IEdge> iterable)
        {
            Contract.Requires(iterable != null);

            _iterable = iterable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new ReadOnlyEdge(edge)).Cast<IEdge>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }

        ~ReadOnlyEdgeIterable()
        {
            Dispose(false);
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
    }
}