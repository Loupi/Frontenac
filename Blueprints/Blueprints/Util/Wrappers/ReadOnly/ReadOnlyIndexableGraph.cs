using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndexableGraph : ReadOnlyGraph, IIndexableGraph
    {
        private readonly IIndexableGraph _baseIndexableGraph;

        public ReadOnlyIndexableGraph(IIndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            Contract.Requires(baseIndexableGraph != null);

            _baseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            var index = _baseIndexableGraph.GetIndex(indexName, indexClass);
            return new ReadOnlyIndex(this, index);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new ReadOnlyIndexIterable(this, _baseIndexableGraph.GetIndices());
        }
    }
}