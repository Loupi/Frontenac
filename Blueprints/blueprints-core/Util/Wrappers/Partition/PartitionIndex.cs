using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndex : Index
    {
        protected Index _RawIndex;
        protected PartitionGraph _Graph;

        public PartitionIndex(Index rawIndex, PartitionGraph graph)
        {
            _RawIndex = rawIndex;
            _Graph = graph;
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
            return this.Get(key, value).LongCount();
        }

        public void Remove(string key, object value, Element element)
        {
            _RawIndex.Remove(key, value, (element as PartitionElement).GetBaseElement());
        }

        public void Put(string key, object value, Element element)
        {
            _RawIndex.Put(key, value, (element as PartitionElement).GetBaseElement());
        }

        public CloseableIterable<Element> Get(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new PartitionVertexIterable((IEnumerable<Vertex>)_RawIndex.Get(key, value), _Graph);
            else
                return (CloseableIterable<Element>)new PartitionEdgeIterable((IEnumerable<Edge>)_RawIndex.Get(key, value), _Graph);
        }

        public CloseableIterable<Element> Query(string key, object value)
        {
            if (typeof(Vertex).IsAssignableFrom(this.GetIndexClass()))
                return (CloseableIterable<Element>)new PartitionVertexIterable((IEnumerable<Vertex>)_RawIndex.Query(key, value), _Graph);
            else
                return (CloseableIterable<Element>)new PartitionEdgeIterable((IEnumerable<Edge>)_RawIndex.Query(key, value), _Graph);
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
