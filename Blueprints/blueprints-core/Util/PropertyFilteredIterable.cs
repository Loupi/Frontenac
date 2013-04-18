using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// This is a helper class for filtering an IEnumerable of elements by their key/value.
    /// Useful for graph implementations that do no support automatic key indices and need to filter on Graph.getVertices/Edges(key,value).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyFilteredIterable<T> : CloseableIterable<T> where T : Element
    {
        readonly string _key;
        readonly object _value;
        readonly IEnumerable<T> _iterable;
        bool _disposed;

        public PropertyFilteredIterable(string key, object value, IEnumerable<T> iterable)
        {
            _key = key;
            _value = value;
            _iterable = iterable;
        }

        ~PropertyFilteredIterable()
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
            return new PropertyFilteredIterableIterable<T>(this).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        class PropertyFilteredIterableIterable<U> : IEnumerable<U> where U : Element
        {
            readonly PropertyFilteredIterable<U> _propertyFilteredIterable;
            readonly IEnumerator<U> _itty;
            U _nextElement = default(U);

            public PropertyFilteredIterableIterable(PropertyFilteredIterable<U> propertyFilteredIterable)
            {
                _propertyFilteredIterable = propertyFilteredIterable;
                _itty = _propertyFilteredIterable._iterable.GetEnumerator();
            }

            public IEnumerator<U> GetEnumerator()
            {
                while (hasNext())
                {
                    if (null != _nextElement)
                    {
                        U temp = _nextElement;
                        _nextElement = default(U);
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            U element = _itty.Current;
                            if (element.getPropertyKeys().Contains(_propertyFilteredIterable._key) &&
                                element.getProperty(_propertyFilteredIterable._key).Equals(_propertyFilteredIterable._value))
                                yield return element;
                        }
                    }
                }
            }

            bool hasNext()
            {
                if (null != _nextElement)
                    return true;
                else
                {
                    while (_itty.MoveNext())
                    {
                        U element = _itty.Current;
                        object temp = element.getProperty(_propertyFilteredIterable._key);
                        if (null != temp)
                        {
                            if (temp.Equals(_propertyFilteredIterable._value))
                            {
                                _nextElement = element;
                                return true;
                            }
                        }
                        else
                        {
                            if (_propertyFilteredIterable._value == null)
                            {
                                _nextElement = element;
                                return true;
                            }
                        }
                    }

                    _nextElement = default(U);
                    return false;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<U>).GetEnumerator();
            }
        }
    }
}
