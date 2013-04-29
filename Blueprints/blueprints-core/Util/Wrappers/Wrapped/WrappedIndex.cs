using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndex : IIndex
    {
        protected IIndex RawIndex;

        public WrappedIndex(IIndex rawIndex)
        {
            RawIndex = rawIndex;
        }

        public string GetIndexName()
        {
            return RawIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return RawIndex.GetIndexClass();
        }

        public long Count(string key, object value)
        {
            return RawIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            var wrappedElement = element as WrappedElement;
            if (wrappedElement != null)
                RawIndex.Remove(key, value, wrappedElement.GetBaseElement());
        }

        public void Put(string key, object value, IElement element)
        {
            var wrappedElement = element as WrappedElement;
            if (wrappedElement != null)
                RawIndex.Put(key, value, wrappedElement.GetBaseElement());
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            if (typeof(IVertex).IsAssignableFrom(GetIndexClass()))
                return new WrappedVertexIterable((IEnumerable<IVertex>)RawIndex.Get(key, value));
            return new WrappedEdgeIterable((IEnumerable<IEdge>)RawIndex.Get(key, value));
        }

        public ICloseableIterable<IElement> Query(string key, object value)
        {
            if (typeof(IVertex).IsAssignableFrom(GetIndexClass()))
                return new WrappedVertexIterable((IEnumerable<IVertex>)RawIndex.Query(key, value));
            return new WrappedEdgeIterable((IEnumerable<IEdge>)RawIndex.Query(key, value));
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
