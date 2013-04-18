using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndexableGraph : WrappedGraph, IndexableGraph, WrapperGraph
    {
        readonly IndexableGraph _baseIndexableGraph;

        public WrappedIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _baseIndexableGraph = baseIndexableGraph;
        }

        public void dropIndex(string indexName)
        {
            _baseIndexableGraph.dropIndex(indexName);
        }

        public IEnumerable<Index> getIndices()
        {
            return new WrappedIndexIterable(_baseIndexableGraph.getIndices());
        }

        public Index getIndex(string indexName, Type indexClass)
        {
            Index index = _baseIndexableGraph.getIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new WrappedIndex(index);
        }

        public Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new WrappedIndex(_baseIndexableGraph.createIndex(indexName, indexClass, indexParameters));
        }
    }
}
