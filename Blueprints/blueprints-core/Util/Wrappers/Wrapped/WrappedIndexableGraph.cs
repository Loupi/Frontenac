using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndexableGraph : WrappedGraph, IndexableGraph, WrapperGraph
    {
        readonly IndexableGraph _BaseIndexableGraph;

        public WrappedIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _BaseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            _BaseIndexableGraph.DropIndex(indexName);
        }

        public IEnumerable<Index> GetIndices()
        {
            return new WrappedIndexIterable(_BaseIndexableGraph.GetIndices());
        }

        public Index GetIndex(string indexName, Type indexClass)
        {
            Index index = _BaseIndexableGraph.GetIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new WrappedIndex(index);
        }

        public Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new WrappedIndex(_BaseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters));
        }
    }
}
