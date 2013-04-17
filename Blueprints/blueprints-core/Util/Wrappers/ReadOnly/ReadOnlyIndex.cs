using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndex : Index
    {
        protected Index _RawIndex;

        public ReadOnlyIndex(Index rawIndex)
        {
            _RawIndex = rawIndex;
        }

        public void Remove(string key, object value, Element element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public void Put(string key, object value, Element element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new ReadOnlyVertexIterable((IEnumerable<Vertex>)_RawIndex.Get(key, value));
            else
                return (CloseableIterable<Element>)new ReadOnlyEdgeIterable((IEnumerable<Edge>)_RawIndex.Get(key, value));
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new ReadOnlyVertexIterable((IEnumerable<Vertex>)_RawIndex.Query(key, value));
            else
                return (CloseableIterable<Element>)new ReadOnlyEdgeIterable((IEnumerable<Edge>)_RawIndex.Query(key, value));
        }

        public long Count(string key, object value)
        {
            return _RawIndex.Count(key, value);
        }

        public string GetIndexName()
        {
            return _RawIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return _RawIndex.GetIndexClass();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
