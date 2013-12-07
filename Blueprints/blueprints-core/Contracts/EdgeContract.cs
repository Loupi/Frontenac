using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq.Expressions;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IEdge))]
    public abstract class EdgeContract : IEdge
    {
        public IVertex GetVertex(Direction direction)
        {
            Contract.Requires(direction != Direction.Both);
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return null;
        }

        public string Label
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return null;
            }
        }

        public abstract object GetProperty(string key);
        public abstract IEnumerable<string> GetPropertyKeys();
        public abstract void SetProperty(string key, object value);
        public abstract object RemoveProperty(string key);
        public abstract void Remove();
        public abstract object Id { get; }
        public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(KeyValuePair<string, object> item);
        public abstract void Clear();
        public abstract bool Contains(KeyValuePair<string, object> item);
        public abstract void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex);
        public abstract bool Remove(KeyValuePair<string, object> item);
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool ContainsKey(string key);
        public abstract void Add(string key, object value);
        public abstract bool Remove(string key);
        public abstract bool TryGetValue(string key, out object value);
        public abstract object this[string key] { get; set; }
        public abstract ICollection<string> Keys { get; }
        public abstract ICollection<object> Values { get; }
        public abstract DynamicMetaObject GetMetaObject(Expression parameter);
    }
}