using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public class WrappingCloseableIterable<T> : CloseableIterable<T>
    {
        readonly IEnumerable<T> _iterable;
        bool _disposed;

        public WrappingCloseableIterable(IEnumerable<T> iterable)
        {
            _iterable = iterable;
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
            if (_disposed)
                return;

            if (disposing)
            {
                if (_iterable is IDisposable)
                    (_iterable as IDisposable).Dispose();
            }

            _disposed = true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in _iterable)
                yield return t;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        public override string ToString()
        {
            return _iterable.ToString();
        }
    }
}
