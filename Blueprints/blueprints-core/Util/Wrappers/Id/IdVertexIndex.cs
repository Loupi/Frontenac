using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertexIndex : Index
    {
        readonly Index _baseIndex;
        readonly IdGraph _idGraph;

        public IdVertexIndex(Index baseIndex, IdGraph idGraph)
        {
            if (null == baseIndex)
                throw new ArgumentException("null base index");

            _idGraph = idGraph;
            _baseIndex = baseIndex;
        }

        public string getIndexName()
        {
            return _baseIndex.getIndexName();
        }

        public Type getIndexClass()
        {
            return _baseIndex.getIndexClass();
        }

        public void put(string key, object value, Element element)
        {
            _baseIndex.put(key, value, GetBaseElement(element));
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            return (CloseableIterable<Element>)new IdVertexIterable(_baseIndex.get(key, value), _idGraph);
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            return (CloseableIterable<Element>)new IdVertexIterable(_baseIndex.query(key, value), _idGraph);
        }

        public long count(string key, object value)
        {
            return _baseIndex.count(key, value);
        }

        public void remove(string key, object value, Element element)
        {
            _baseIndex.remove(key, value, GetBaseElement(element));
        }

        Vertex GetBaseElement(Element e)
        {
            return ((IdVertex)e).getBaseVertex();
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
