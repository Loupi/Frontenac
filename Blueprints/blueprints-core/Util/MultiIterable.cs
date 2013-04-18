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
        readonly List<IEnumerable<S>> _iterables;
        bool _disposed;

        public MultiIterable(List<IEnumerable<S>> iterables)
        {
            _iterables = iterables;
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
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (IEnumerable<S> itty in _iterables)
                {
                    if (itty is IDisposable)
                        (itty as IDisposable).Dispose();
                }
            }

            _disposed = true;
        }

        public IEnumerator<S> GetEnumerator()
        {
            return new MultiIterableIterable<S>(_iterables).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<S>).GetEnumerator();
        }

        class MultiIterableIterable<T> : IEnumerable<T>
        {
            readonly List<IEnumerable<T>> _iterables;
            IEnumerator<T> _currentIterator;
            int _current = 0;

            public MultiIterableIterable(List<IEnumerable<T>> iterables)
            {
                _iterables = iterables;
                _currentIterator = iterables[0].GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_iterables.Count > 0)
                {
                    while (hasNext())
                    {
                        while (true)
                        {
                            if (_currentIterator.MoveNext())
                            {
                                yield return _currentIterator.Current;
                            }
                            else
                            {
                                _current++;
                                if (_current >= _iterables.Count)
                                    break;
                                _currentIterator = _iterables[_current].GetEnumerator();
                            }
                        }
                    }
                }
            }

            bool hasNext()
            {
                while (true)
                {
                    if (_currentIterator.MoveNext())
                    {
                        _currentIterator.Reset();
                        return true;
                    }
                    else
                    {
                        _current++;
                        if (_current >= _iterables.Count)
                            break;
                        _currentIterator = _iterables[_current].GetEnumerator();
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
