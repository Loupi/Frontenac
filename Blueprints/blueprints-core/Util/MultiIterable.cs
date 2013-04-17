using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// A helper class that is used to combine multiple iterables into a single closeable IEnumerable.
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class MultiIterable<S> : CloseableIterable<S>
    {
        readonly List<IEnumerable<S>> _Iterables;
        bool _Disposed;

        public MultiIterable(List<IEnumerable<S>> iterables)
        {
            _Iterables = iterables;
        }

        ~MultiIterable()
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
                foreach (IEnumerable<S> itty in _Iterables)
                {
                    if (itty is IDisposable)
                        (itty as IDisposable).Dispose();
                }
            }

            _Disposed = true;
        }

        public IEnumerator<S> GetEnumerator()
        {
            return new MultiIterableIterable<S>(_Iterables).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<S>).GetEnumerator();
        }

        class MultiIterableIterable<T> : IEnumerable<T>
        {
            readonly List<IEnumerable<T>> _Iterables;
            IEnumerator<T> _CurrentIterator;
            int _Current = 0;

            public MultiIterableIterable(List<IEnumerable<T>> iterables)
            {
                _Iterables = iterables;
                _CurrentIterator = iterables[0].GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_Iterables.Count > 0)
                {
                    while (HasNext())
                    {
                        while (true)
                        {
                            if (_CurrentIterator.MoveNext())
                            {
                                yield return _CurrentIterator.Current;
                            }
                            else
                            {
                                _Current++;
                                if (_Current >= _Iterables.Count)
                                    break;
                                _CurrentIterator = _Iterables[_Current].GetEnumerator();
                            }
                        }
                    }
                }
            }

            bool HasNext()
            {
                while (true)
                {
                    if (_CurrentIterator.MoveNext())
                    {
                        _CurrentIterator.Reset();
                        return true;
                    }
                    else
                    {
                        _Current++;
                        if (_Current >= _Iterables.Count)
                            break;
                        _CurrentIterator = _Iterables[_Current].GetEnumerator();
                    }
                }
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<T>).GetEnumerator();
            }
        }
    }
}
