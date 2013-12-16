using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerIndex : IIndex
    {
        protected readonly Type IndexClass;
        protected readonly string IndexName;

        internal Dictionary<string, Dictionary<object, HashSet<IElement>>> Index =
            new Dictionary<string, Dictionary<object, HashSet<IElement>>>();

        public TinkerIndex(string indexName, Type indexClass)
        {
            Contract.Requires(indexClass != null);
            Contract.Requires(typeof (IVertex).IsAssignableFrom(indexClass) ||
                              typeof (IEdge).IsAssignableFrom(indexClass));

            IndexName = indexName;
            IndexClass = indexClass;
        }

        public string Name
        {
            get { return IndexName; }
        }

        public Type Type
        {
            get { return IndexClass; }
        }

        public void Put(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (keyMap == null)
            {
                keyMap = new Dictionary<object, HashSet<IElement>>();
                Index.Put(key, keyMap);
            }
            var objects = keyMap.Get(value);
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
            return null == set
                       ? new WrappingCloseableIterable<IElement>(Enumerable.Empty<IElement>())
                       : new WrappingCloseableIterable<IElement>(new List<IElement>(set));
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
            return null == set ? 0 : set.LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (null != keyMap)
            {
                var objects = keyMap.Get(value);
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
            Contract.Requires(element != null);

            if (IndexClass.IsInstanceOfType(element))
            {
                foreach (var map in Index.Values)
                {
                    foreach (var set in map.Values)
                        set.Remove(element);
                }
            }
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}