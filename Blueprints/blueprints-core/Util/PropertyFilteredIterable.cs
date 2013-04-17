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
        readonly string _Key;
        readonly object _Value;
        readonly IEnumerable<T> _Iterable;
        bool _Disposed;

        public PropertyFilteredIterable(string key, object value, IEnumerable<T> iterable)
        {
            _Key = key;
            _Value = value;
            _Iterable = iterable;
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
            return new PropertyFilteredIterableIterable<T>(this).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        class PropertyFilteredIterableIterable<U> : IEnumerable<U> where U : Element
        {
            readonly PropertyFilteredIterable<U> _PropertyFilteredIterable;
            readonly IEnumerator<U> _Itty;
            U _NextElement = default(U);

            public PropertyFilteredIterableIterable(PropertyFilteredIterable<U> propertyFilteredIterable)
            {
                _PropertyFilteredIterable = propertyFilteredIterable;
                _Itty = _PropertyFilteredIterable._Iterable.GetEnumerator();
            }

            public IEnumerator<U> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _NextElement)
                    {
                        U temp = _NextElement;
                        _NextElement = default(U);
                        yield return temp;
                    }
                    else
                    {
                        while (_Itty.MoveNext())
                        {
                            U element = _Itty.Current;
                            if (element.GetPropertyKeys().Contains(_PropertyFilteredIterable._Key) &&
                                element.GetProperty(_PropertyFilteredIterable._Key).Equals(_PropertyFilteredIterable._Value))
                                yield return element;
                        }
                    }
                }
            }

            bool HasNext()
            {
                if (null != _NextElement)
                    return true;
                else
                {
                    while (_Itty.MoveNext())
                    {
                        U element = _Itty.Current;
                        object temp = element.GetProperty(_PropertyFilteredIterable._Key);
                        if (null != temp)
                        {
                            if (temp.Equals(_PropertyFilteredIterable._Value))
                            {
                                _NextElement = element;
                                return true;
                            }
                        }
                        else
                        {
                            if (_PropertyFilteredIterable._Value == null)
                            {
                                _NextElement = element;
                                return true;
                            }
                        }
                    }

                    _NextElement = default(U);
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
