using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    class IdVertexIterable : ICloseableIterable<IVertex>
    {
        readonly IEnumerable<IElement> _iterable;
        readonly IdGraph _idGraph;
        bool _disposed;

        public IdVertexIterable(IEnumerable<IElement> iterable, IdGraph idGraph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(idGraph != null);

            _iterable = iterable;
            _idGraph = idGraph;
        }

        ~IdVertexIterable()
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
            return (_iterable.OfType<IVertex>().Select(v => new IdVertex(v, _idGraph))).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }
    }
}
