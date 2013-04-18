using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndexableGraph : PartitionGraph, IndexableGraph
    {
        protected IndexableGraph baseIndexableGraph;

        public PartitionIndexableGraph(IndexableGraph baseIndexableGraph, string writeGraphKey, string writeGraph, IEnumerable<string> readGraphs)
            : base(baseIndexableGraph, writeGraphKey, writeGraph, readGraphs)
        {
            this.baseIndexableGraph = baseIndexableGraph;
        }

        public PartitionIndexableGraph(IndexableGraph baseIndexableGraph, string writeGraphKey, string readWriteGraph)
            : base(baseIndexableGraph, writeGraphKey, readWriteGraph)
        {
        }

        public void dropIndex(string indexName)
        {
            baseIndexableGraph.dropIndex(indexName);
        }

        public IEnumerable<Index> getIndices()
        {
            return new PartitionIndexIterable(baseIndexableGraph.getIndices(), this);
        }

        public Index getIndex(string indexName, Type indexClass)
        {
            Index index = baseIndexableGraph.getIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new PartitionIndex(index, this);
        }

        public Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new PartitionIndex(baseIndexableGraph.createIndex(indexName, indexClass, indexParameters), this);
        }
    }
}
