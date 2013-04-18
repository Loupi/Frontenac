using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndex : Index
    {
        protected Index rawIndex;

        public WrappedIndex(Index rawIndex)
        {
            this.rawIndex = rawIndex;
        }

        public string getIndexName()
        {
            return rawIndex.getIndexName();
        }

        public Type getIndexClass()
        {
            return rawIndex.getIndexClass();
        }

        public long count(string key, object value)
        {
            return rawIndex.count(key, value);
        }

        public void remove(string key, object value, Element element)
        {
            rawIndex.remove(key, value, (element as WrappedElement).getBaseElement());
        }

        public void put(string key, object value, Element element)
        {
            rawIndex.put(key, value, (element as WrappedElement).getBaseElement());
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new WrappedVertexIterable((IEnumerable<Vertex>)rawIndex.get(key, value));
            else
                return (CloseableIterable<Element>)new WrappedEdgeIterable((IEnumerable<Edge>)rawIndex.get(key, value));
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new WrappedVertexIterable((IEnumerable<Vertex>)rawIndex.query(key, value));
            else
                return (CloseableIterable<Element>)new WrappedEdgeIterable((IEnumerable<Edge>)rawIndex.query(key, value));
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
