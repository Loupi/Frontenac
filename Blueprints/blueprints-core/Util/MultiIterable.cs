using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// A helper class that is used to combine multiple iterables into a single closeable IEnumerable.
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    public class MultiIterable<TS> : ICloseableIterable<TS>
    {
        readonly List<IEnumerable<TS>> _iterables;
        bool _disposed;

        public MultiIterable(List<IEnumerable<TS>> iterables)
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
                foreach (IEnumerable<TS> itty in _iterables)
                {
                    if (itty is IDisposable)
                        (itty as IDisposable).Dispose();
                }
            }

            _disposed = true;
        }

        public IEnumerator<TS> GetEnumerator()
        {
            return new MultiIterableIterable<TS>(_iterables).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TS>).GetEnumerator();
        }

        class MultiIterableIterable<T> : IEnumerable<T>
        {
            readonly List<IEnumerable<T>> _iterables;
            IEnumerator<T> _currentIterator;
            int _current;

            public MultiIterableIterable(List<IEnumerable<T>> iterables)
            {
                _iterables = iterables;
                _currentIterator = iterables[0].GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_iterables.Count > 0)
                {
                    while (HasNext())
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

            bool HasNext()
            {
                while (true)
                {
                    if (_currentIterator.MoveNext())
                    {
                        _currentIterator.Reset();
                        return true;
                    }
                    _current++;
                    if (_current >= _iterables.Count)
                        break;
                    _currentIterator = _iterables[_current].GetEnumerator();
                }
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
