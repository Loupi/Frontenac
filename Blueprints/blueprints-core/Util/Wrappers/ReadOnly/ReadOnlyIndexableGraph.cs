using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndexableGraph : ReadOnlyGraph, IndexableGraph, WrapperGraph
    {
        readonly IndexableGraph _baseIndexableGraph;

        public ReadOnlyIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _baseIndexableGraph = baseIndexableGraph;
        }

        public void dropIndex(string indexName)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public Index getIndex(string indexName, Type indexClass)
        {
            Index index = _baseIndexableGraph.getIndex(indexName, indexClass);
            return new ReadOnlyIndex(index);
        }

        public IEnumerable<Index> getIndices()
        {
            return new ReadOnlyIndexIterable(_baseIndexableGraph.getIndices());
        }
    }
}
