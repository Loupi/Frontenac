using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndex : Index
    {
        protected Index _RawIndex;

        public WrappedIndex(Index rawIndex)
        {
            _RawIndex = rawIndex;
        }

        public string GetIndexName()
        {
            return _RawIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return _RawIndex.GetIndexClass();
        }

        public long Count(string key, object value)
        {
            return _RawIndex.Count(key, value);
        }

        public void Remove(string key, object value, Element element)
        {
            _RawIndex.Remove(key, value, (element as WrappedElement).GetBaseElement());
        }

        public void Put(string key, object value, Element element)
        {
            _RawIndex.Put(key, value, (element as WrappedElement).GetBaseElement());
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new WrappedVertexIterable((IEnumerable<Vertex>)_RawIndex.Get(key, value));
            else
                return (CloseableIterable<Element>)new WrappedEdgeIterable((IEnumerable<Edge>)_RawIndex.Get(key, value));
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new WrappedVertexIterable((IEnumerable<Vertex>)_RawIndex.Query(key, value));
            else
                return (CloseableIterable<Element>)new WrappedEdgeIterable((IEnumerable<Edge>)_RawIndex.Query(key, value));
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
