using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdEdgeIterable : ICloseableIterable<IEdge>
    {
        readonly IEnumerable<IElement> _iterable;
        readonly IdGraph _idGraph;
        bool _disposed;

        public IdEdgeIterable(IEnumerable<IElement> iterable, IdGraph idGraph)
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

        public IEnumerator<IEdge> GetEnumerator()
        {
            return (from IEdge edge in _iterable select new IdEdge(edge, _idGraph)).Cast<IEdge>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }
    }
}
