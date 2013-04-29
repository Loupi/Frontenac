using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    class ReadOnlyEdgeIterable : ICloseableIterable<IEdge>
    {
        readonly IEnumerable<IEdge> _iterable;
        bool _disposed;

        public ReadOnlyEdgeIterable(IEnumerable<IEdge> iterable)
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

        public IEnumerator<IEdge> GetEnumerator()
        {
            return _iterable.Select(edge => new ReadOnlyEdge(edge)).Cast<IEdge>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }
    }
}
