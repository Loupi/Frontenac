using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndexableGraph : WrappedGraph, IIndexableGraph
    {
        private readonly IIndexableGraph _baseIndexableGraph;

        public WrappedIndexableGraph(IIndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _baseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            _baseIndexableGraph.DropIndex(indexName);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new WrappedIndexIterable(_baseIndexableGraph.GetIndices());
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            var index = _baseIndexableGraph.GetIndex(indexName, indexClass);
            return null == index ? null : new WrappedIndex(index);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new WrappedIndex(_baseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters));
        }
    }
}