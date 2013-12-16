using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndex : IIndex
    {
        protected PartitionGraph Graph;
        protected IIndex RawIndex;

        public PartitionIndex(IIndex rawIndex, PartitionGraph graph)
        {
            Contract.Requires(rawIndex != null);
            Contract.Requires(graph != null);

            RawIndex = rawIndex;
            Graph = graph;
        }

        public string Name
        {
            get { return RawIndex.Name; }
        }

        public Type Type
        {
            get { return RawIndex.Type; }
        }

        public long Count(string key, object value)
        {
            return Get(key, value).LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                RawIndex.Remove(key, value, partitionElement.GetBaseElement());
        }

        public void Put(string key, object value, IElement element)
        {
            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                RawIndex.Put(key, value, partitionElement.GetBaseElement());
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new PartitionVertexIterable((IEnumerable<IVertex>) RawIndex.Get(key, value), Graph);
            return new PartitionEdgeIterable((IEnumerable<IEdge>) RawIndex.Get(key, value), Graph);
        }

        public ICloseableIterable<IElement> Query(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new PartitionVertexIterable((IEnumerable<IVertex>) RawIndex.Query(key, value), Graph);
            return new PartitionEdgeIterable((IEnumerable<IEdge>) RawIndex.Query(key, value), Graph);
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}