using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndexableGraph : ReadOnlyGraph, IndexableGraph, WrapperGraph
    {
        readonly IndexableGraph _BaseIndexableGraph;

        public ReadOnlyIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _BaseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Index GetIndex(string indexName, Type indexClass)
        {
            Index index = _BaseIndexableGraph.GetIndex(indexName, indexClass);
            return new ReadOnlyIndex(index);
        }

        public IEnumerable<Index> GetIndices()
        {
            return new ReadOnlyIndexIterable(_BaseIndexableGraph.GetIndices());
        }
    }
}
