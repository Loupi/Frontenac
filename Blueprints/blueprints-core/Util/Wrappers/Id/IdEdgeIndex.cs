using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdgeIndex : Index
    {
        protected readonly Index _BaseIndex;
        protected readonly IdGraph _IdGraph;

        public IdEdgeIndex(Index baseIndex, IdGraph idGraph)
        {
            if (null == baseIndex)
                throw new ArgumentException("null base index");

            _IdGraph = idGraph;
            _BaseIndex = baseIndex;
        }

        public string GetIndexName()
        {
            return _BaseIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return _BaseIndex.GetIndexClass();
        }

        public void Put(string key, object value, Element element)
        {
            _BaseIndex.Put(key, value, GetBaseElement(element));
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            return (CloseableIterable<Element>)new IdEdgeIterable(_BaseIndex.Get(key, value), _IdGraph);
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            return (CloseableIterable<Element>)new IdEdgeIterable(_BaseIndex.Query(key, value), _IdGraph);
        }

        public long Count(string key, object value)
        {
            return _BaseIndex.Count(key, value);
        }

        public void Remove(string key, object value, Element element)
        {
            _BaseIndex.Remove(key, value, GetBaseElement(element));
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }

        Edge GetBaseElement(Element e)
        {
            return ((IdEdge)e).GetBaseEdge();
        }
    }
}
