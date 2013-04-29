using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    class TinkerIndex : IIndex
    {
        internal Dictionary<string, Dictionary<object, HashSet<IElement>>> Index = new Dictionary<string, Dictionary<object, HashSet<IElement>>>();
        protected readonly string IndexName;
        protected readonly Type IndexClass;

        public TinkerIndex(string indexName, Type indexClass)
        {
            IndexName = indexName;
            IndexClass = indexClass;
        }

        public string GetIndexName()
        {
            return IndexName;
        }

        public Type GetIndexClass()
        {
            return IndexClass;
        }

        public void Put(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (keyMap == null)
            {
                keyMap = new Dictionary<object, HashSet<IElement>>();
                Index.Put(key, keyMap);
            }
            HashSet<IElement> objects = keyMap.Get(value);
            if (null == objects)
            {
                objects = new HashSet<IElement>();
                keyMap.Put(value, objects);
            }
            objects.Add(element);
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            var keyMap = Index.Get(key);
            if (null == keyMap)
                return new WrappingCloseableIterable<IElement>(Enumerable.Empty<IElement>());
            var set = keyMap.Get(value);
            if (null == set)
                return new WrappingCloseableIterable<IElement>(Enumerable.Empty<IElement>());
            return new WrappingCloseableIterable<IElement>(new List<IElement>(set));
        }

        public ICloseableIterable<IElement> Query(string key, object query)
        {
            throw new NotImplementedException();
        }

        public long Count(string key, object value)
        {
            var keyMap = Index.Get(key);
            if (null == keyMap)
                return 0;
            var set = keyMap.Get(value);
            if (null == set)
                return 0;
            return set.LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (null != keyMap)
            {
                HashSet<IElement> objects = keyMap.Get(value);
                if (null != objects)
                {
                    objects.Remove(element);
                    if (objects.Count == 0)
                        keyMap.Remove(value);
                }
            }
        }

        public void RemoveElement(IElement element)
        {
            if (IndexClass.IsInstanceOfType(element))
            {
                foreach (var map in Index.Values)
                {
                    foreach (HashSet<IElement> set in map.Values)
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
