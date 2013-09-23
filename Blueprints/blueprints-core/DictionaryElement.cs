using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints
{
    [Serializable]
    public abstract class DictionaryElement : IElement
    {
        public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetPropertyKeys()
                .Select(property => new KeyValuePair<string, object>(property, GetProperty(property)))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(KeyValuePair<string, object> item)
        {
            SetProperty(item.Key, item.Value);
        }

        public virtual void Clear()
        {
            foreach (var property in GetPropertyKeys())
                RemoveProperty(property);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ContainsKey(item.Key) && GetProperty(item.Key) == item.Value;
        }

        public virtual void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var item in array)
            {
                SetProperty(item.Key, item.Value);
            }
        }

        public virtual bool Remove(KeyValuePair<string, object> item)
        {
            var result = (Contains(item));
            if (result)
                RemoveProperty(item.Key);
            return result;
        }

        public virtual int Count
        {
            get { return GetPropertyKeys().Count(); }
        }

        public bool IsReadOnly { get; protected set; }

        public virtual bool ContainsKey(string key)
        {
            return GetPropertyKeys().Contains(key);
        }

        public virtual void Add(string key, object value)
        {
            SetProperty(key, value);
        }

        public virtual bool Remove(string key)
        {
            var result = ContainsKey(key);
            if (result)
                RemoveProperty(key);
            return result;
        }

        public virtual bool TryGetValue(string key, out object value)
        {
            var result = ContainsKey(key);
            value = result ? GetProperty(key) : null;
            return result;
        }

        public virtual object this[string key]
        {
            get { return GetProperty(key); }
            set { SetProperty(key, value); }
        }

        public virtual ICollection<string> Keys
        {
            get { return GetPropertyKeys().ToList(); }
        }

        public virtual ICollection<object> Values
        {
            get { return GetPropertyKeys().Select(GetProperty).ToList(); }
        }

        public abstract object Id { get; }
        public abstract object GetProperty(string key);
        public abstract IEnumerable<string> GetPropertyKeys();
        public abstract void SetProperty(string key, object value);
        public abstract object RemoveProperty(string key);
        public abstract void Remove();
    }
}