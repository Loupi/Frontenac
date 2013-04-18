using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndex : Index
    {
        protected Index rawIndex;
        protected PartitionGraph graph;

        public PartitionIndex(Index rawIndex, PartitionGraph graph)
        {
            this.rawIndex = rawIndex;
            this.graph = graph;
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
            return this.get(key, value).LongCount();
        }

        public void remove(string key, object value, Element element)
        {
            rawIndex.remove(key, value, (element as PartitionElement).getBaseElement());
        }

        public void put(string key, object value, Element element)
        {
            rawIndex.put(key, value, (element as PartitionElement).getBaseElement());
        }

        public CloseableIterable<Element> get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new PartitionVertexIterable((IEnumerable<Vertex>)rawIndex.get(key, value), graph);
            else
                return (CloseableIterable<Element>)new PartitionEdgeIterable((IEnumerable<Edge>)rawIndex.get(key, value), graph);
        }

        public CloseableIterable<Element> query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.getIndexClass()))
                return (CloseableIterable<Element>)new PartitionVertexIterable((IEnumerable<Vertex>)rawIndex.query(key, value), graph);
            else
                return (CloseableIterable<Element>)new PartitionEdgeIterable((IEnumerable<Edge>)rawIndex.query(key, value), graph);
        }

        public override string ToString()
        {
            return StringFactory.indexString(this);
        }
    }
}
