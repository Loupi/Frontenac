﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionIndexableGraph : PartitionGraph, IIndexableGraph
    {
        protected IIndexableGraph BaseIndexableGraph;

        public PartitionIndexableGraph(IIndexableGraph baseIndexableGraph, string writeGraphKey, string writeGraph,
                                       IEnumerable<string> readGraphs)
            : base(baseIndexableGraph, writeGraphKey, writeGraph, readGraphs)
        {
            Contract.Requires(baseIndexableGraph != null);
            Contract.Requires(readGraphs != null);

            BaseIndexableGraph = baseIndexableGraph;
        }

        public PartitionIndexableGraph(IIndexableGraph baseIndexableGraph, string writeGraphKey, string readWriteGraph)
            : base(baseIndexableGraph, writeGraphKey, readWriteGraph)
        {
            Contract.Requires(baseIndexableGraph != null);
        }

        public void DropIndex(string indexName)
        {
            BaseIndexableGraph.DropIndex(indexName);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new PartitionIndexIterable(BaseIndexableGraph.GetIndices(), this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            var index = BaseIndexableGraph.GetIndex(indexName, indexClass);
            return null == index ? null : new PartitionIndex(index, this);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new PartitionIndex(BaseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters), this);
        }
    }
}