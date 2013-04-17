using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public class WrappingCloseableIterable<T> : CloseableIterable<T>
    {
        readonly IEnumerable<T> _Iterable;
        bool _Disposed;

        public WrappingCloseableIterable(IEnumerable<T> iterable)
        {
            _Iterable = iterable;
        }

        ~WrappingCloseableIterable()
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

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in _Iterable)
                yield return t;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        public override string ToString()
        {
            return _Iterable.ToString();
        }
    }
}
