using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq.Expressions;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IElement))]
    public abstract class ElementContract : IElement
    {
        public object GetProperty(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return null;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return null;
        }

        public void SetProperty(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public object RemoveProperty(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return null;
        }

        public void Remove()
        {
        }

        public object Id
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return null;
            }
        }

        public IGraph Graph
        {
            get
            {
                Contract.Ensures(Contract.Result<IGraph>() != null);
                return null;
            }
        }

        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();
        public abstract void Remove(object key);

        object IDictionary.this[object key]
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(KeyValuePair<string, object> item);
        public abstract bool Contains(object key);
        public abstract void Add(object key, object value);
        public abstract void Clear();
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public abstract bool Contains(KeyValuePair<string, object> item);
        public abstract void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex);
        public abstract bool Remove(KeyValuePair<string, object> item);
        public abstract void CopyTo(Array array, int index);
        public abstract int Count { get; }
        public abstract object SyncRoot { get; }
        public abstract bool IsSynchronized { get; }

        ICollection IDictionary.Values
        {
            get { throw new NotSupportedException(); }
        }

        public abstract bool IsReadOnly { get; }
        public abstract bool IsFixedSize { get; }
        public abstract bool ContainsKey(string key);
        public abstract void Add(string key, object value);
        public abstract bool Remove(string key);
        public abstract bool TryGetValue(string key, out object value);
        public abstract object this[string key] { get; set; }
        public abstract ICollection<string> Keys { get; }

        ICollection IDictionary.Keys
        {
            get { throw new NotSupportedException(); }
        }

        public abstract ICollection<object> Values { get; }
        public abstract DynamicMetaObject GetMetaObject(Expression parameter);
    }
}