using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndex : Index
    {
        protected Index rawIndex;

        public ReadOnlyIndex(Index rawIndex)
        {
            this.rawIndex = rawIndex;
        }

        public void remove(string key, object value, Element element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public void put(string key, object value, Element element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new ReadOnlyVertexIterable((IEnumerable<Vertex>)rawIndex.get(key, value));
            else
                return (CloseableIterable<Element>)new ReadOnlyEdgeIterable((IEnumerable<Edge>)rawIndex.get(key, value));
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new ReadOnlyVertexIterable((IEnumerable<Vertex>)rawIndex.query(key, value));
            else
                return (CloseableIterable<Element>)new ReadOnlyEdgeIterable((IEnumerable<Edge>)rawIndex.query(key, value));
        }

        public long count(string key, object value)
        {
            return rawIndex.count(key, value);
        }

        public string getIndexName()
        {
            return rawIndex.getIndexName();
        }

        public Type getIndexClass()
        {
            return rawIndex.getIndexClass();
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
