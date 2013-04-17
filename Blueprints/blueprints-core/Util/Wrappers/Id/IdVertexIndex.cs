using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertexIndex : Index
    {
        readonly Index _BaseIndex;
        readonly IdGraph _IdGraph;

        public IdVertexIndex(Index baseIndex, IdGraph idGraph)
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
            return (CloseableIterable<Element>)new IdVertexIterable(_BaseIndex.Get(key, value), _IdGraph);
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            return (CloseableIterable<Element>)new IdVertexIterable(_BaseIndex.Query(key, value), _IdGraph);
        }

        public long Count(string key, object value)
        {
            return _BaseIndex.Count(key, value);
        }

        public void Remove(string key, object value, Element element)
        {
            _BaseIndex.Remove(key, value, GetBaseElement(element));
        }

        Vertex GetBaseElement(Element e)
        {
            return ((IdVertex)e).GetBaseVertex();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
