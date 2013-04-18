using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdgeIndex : Index
    {
        protected readonly Index baseIndex;
        protected readonly IdGraph idGraph;

        public IdEdgeIndex(Index baseIndex, IdGraph idGraph)
        {
            if (null == baseIndex)
                throw new ArgumentException("null base index");

            this.idGraph = idGraph;
            this.baseIndex = baseIndex;
        }

        public string getIndexName()
        {
            return baseIndex.getIndexName();
        }

        public Type getIndexClass()
        {
            return baseIndex.getIndexClass();
        }

        public void put(string key, object value, Element element)
        {
            baseIndex.put(key, value, GetBaseElement(element));
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            return (CloseableIterable<Element>)new IdEdgeIterable(baseIndex.get(key, value), idGraph);
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            return (CloseableIterable<Element>)new IdEdgeIterable(baseIndex.query(key, value), idGraph);
        }

        public long count(string key, object value)
        {
            return baseIndex.count(key, value);
        }

        public void remove(string key, object value, Element element)
        {
            baseIndex.remove(key, value, GetBaseElement(element));
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }

        Edge GetBaseElement(Element e)
        {
            return ((IdEdge)e).getBaseEdge();
        }
    }
}
