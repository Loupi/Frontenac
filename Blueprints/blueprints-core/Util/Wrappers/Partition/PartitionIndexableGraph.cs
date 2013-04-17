using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndexableGraph : PartitionGraph, IndexableGraph
    {
        protected IndexableGraph _BaseIndexableGraph;

        public PartitionIndexableGraph(IndexableGraph baseIndexableGraph, string writeGraphKey, string writeGraph, IEnumerable<string> readGraphs)
            : base(baseIndexableGraph, writeGraphKey, writeGraph, readGraphs)
        {
            _BaseIndexableGraph = baseIndexableGraph;
        }

        public PartitionIndexableGraph(IndexableGraph baseIndexableGraph, string writeGraphKey, string readWriteGraph)
            : base(baseIndexableGraph, writeGraphKey, readWriteGraph)
        {
        }

        public void DropIndex(string indexName)
        {
            _BaseIndexableGraph.DropIndex(indexName);
        }

        public IEnumerable<Index> GetIndices()
        {
            return new PartitionIndexIterable(_BaseIndexableGraph.GetIndices(), this);
        }

        public Index GetIndex(string indexName, Type indexClass)
        {
            Index index = _BaseIndexableGraph.GetIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new PartitionIndex(index, this);
        }

        public Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new PartitionIndex(_BaseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters), this);
        }
    }
}
