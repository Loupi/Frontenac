using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// EventIndexableGraph is merely a proxy to index methods exposing EventGraph methods in the "evented" way. Like the
    /// EventGraph it extends from, this graph implementations raise notifications to the listeners for the
    /// following events: new vertex/edge, vertex/edge property changed, vertex/edge property removed,
    /// vertex/edge removed.
    /// </summary>
    public class EventIndexableGraph : EventGraph, IndexableGraph, WrapperGraph
    {
        readonly IndexableGraph _baseIndexableGraph;

        public EventIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _baseIndexableGraph = baseIndexableGraph;
        }

        public void dropIndex(string indexName)
        {
            _baseIndexableGraph.dropIndex(indexName);
        }

        public Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new EventIndex(_baseIndexableGraph.createIndex(indexName, indexClass, indexParameters), this);
        }

        public Index getIndex(string indexName, Type indexClass)
        {
            Index index = _baseIndexableGraph.getIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new EventIndex(index, this);
        }

        public IEnumerable<Index> getIndices()
        {
            return new EventIndexIterable(_baseIndexableGraph.getIndices(), this);
        }
    }
}
