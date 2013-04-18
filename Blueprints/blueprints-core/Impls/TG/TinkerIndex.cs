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
        internal Dictionary<string, Dictionary<object, HashSet<Element>>> index = new Dictionary<string, Dictionary<object, HashSet<Element>>>();
        protected readonly string indexName;
        protected readonly Type indexClass;

        public TinkerIndex(string indexName, Type indexClass)
        {
            this.indexName = indexName;
            this.indexClass = indexClass;
        }

        public string getIndexName()
        {
            return indexName;
        }

        public Type getIndexClass()
        {
            return indexClass;
        }

        public void put(string key, object value, Element element)
        {
            var keyMap = index.get(key);
            if (keyMap == null)
            {
                keyMap = new Dictionary<object, HashSet<Element>>();
                index.put(key, keyMap);
            }
            HashSet<Element> objects = Portability.get(keyMap, value);
            if (null == objects)
            {
                objects = new HashSet<Element>();
                keyMap.put(value, objects);
            }
            objects.Add(element);
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            var keyMap = index.get(key);
            if (null == keyMap)
                return new WrappingCloseableIterable<Element>(Enumerable.Empty<Element>());
            else
            {
                HashSet<Element> set = Portability.get(keyMap, value);
                if (null == set)
                    return new WrappingCloseableIterable<Element>(Enumerable.Empty<Element>());
                else
                    return new WrappingCloseableIterable<Element>(new List<Element>(set));
            }
        }

        public CloseableIterable<Element> query(string key, object query)
        {
            throw new NotImplementedException();
        }

        public long count(string key, object value)
        {
            var keyMap = index.get(key);
            if (null == keyMap)
                return 0;
            else
            {
                HashSet<Element> set = keyMap.get(value);
                if (null == set)
                    return 0;
                else
                    return set.LongCount();
            }
        }

        public void remove(string key, object value, Element element)
        {
            var keyMap = index.get(key);
            if (null != keyMap)
            {
                HashSet<Element> objects = Portability.get(keyMap, value);
                if (null != objects)
                {
                    objects.Remove(element);
                    if (objects.Count == 0)
                        keyMap.Remove(value);
                }
            }
        }

        public void removeElement(Element element)
        {
            if (indexClass.IsInstanceOfType(element))
            {
                foreach (var map in index.Values)
                {
                    foreach (HashSet<Element> set in map.Values)
                        set.Remove(element);
                }
            }
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
