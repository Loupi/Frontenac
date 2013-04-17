using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    class TinkerIndex : Index
    {
        internal Dictionary<string, Dictionary<object, HashSet<Element>>> _Index = new Dictionary<string, Dictionary<object, HashSet<Element>>>();
        protected readonly string _IndexName;
        protected readonly Type _IndexClass;

        public TinkerIndex(string indexName, Type indexClass)
        {
            _IndexName = indexName;
            _IndexClass = indexClass;
        }

        public string GetIndexName()
        {
            return _IndexName;
        }

        public Type GetIndexClass()
        {
            return _IndexClass;
        }

        public void Put(string key, object value, Element element)
        {
            var keyMap = _Index.Get(key);
            if (keyMap == null)
            {
                keyMap = new Dictionary<object, HashSet<Element>>();
                _Index.Put(key, keyMap);
            }
            HashSet<Element> objects = keyMap.Get(value);
            if (null == objects)
            {
                objects = new HashSet<Element>();
                keyMap.Put(value, objects);
            }
            objects.Add(element);
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            var keyMap = _Index.Get(key);
            if (null == keyMap)
                return new WrappingCloseableIterable<Element>(Enumerable.Empty<Element>());
            else
            {
                HashSet<Element> set = keyMap.Get(value);
                if (null == set)
                    return new WrappingCloseableIterable<Element>(Enumerable.Empty<Element>());
                else
                    return new WrappingCloseableIterable<Element>(new List<Element>(set));
            }
        }

        public CloseableIterable<Element> Query(string key, object query)
        {
            throw new NotImplementedException();
        }

        public long Count(string key, object value)
        {
            var keyMap = _Index.Get(key);
            if (null == keyMap)
                return 0;
            else
            {
                HashSet<Element> set = keyMap.Get(value);
                if (null == set)
                    return 0;
                else
                    return set.LongCount();
            }
        }

        public void Remove(string key, object value, Element element)
        {
            var keyMap = _Index.Get(key);
            if (null != keyMap)
            {
                HashSet<Element> objects = keyMap.Get(value);
                if (null != objects)
                {
                    objects.Remove(element);
                    if (objects.Count == 0)
                        keyMap.Remove(value);
                }
            }
        }

        public void RemoveElement(Element element)
        {
            if (_IndexClass.IsAssignableFrom(element.GetType()))
            {
                foreach (var map in _Index.Values)
                {
                    foreach (HashSet<Element> set in map.Values)
                        set.Remove(element);
                }
            }
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
