using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// EventIndexableGraph is merely a proxy to index methods exposing EventGraph methods in the "evented" way. Like the
    /// EventGraph it extends from, this graph implementations raise notifications to the listeners for the
    /// following events: new vertex/edge, vertex/edge property changed, vertex/edge property removed,
    /// vertex/edge removed.
    /// </summary>
    public class EventIndexableGraph : EventGraph, IIndexableGraph
    {
        readonly IIndexableGraph _baseIndexableGraph;

        public EventIndexableGraph(IIndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            _baseIndexableGraph = baseIndexableGraph;
        }

        public void DropIndex(string indexName)
        {
            _baseIndexableGraph.DropIndex(indexName);
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            return new EventIndex(_baseIndexableGraph.CreateIndex(indexName, indexClass, indexParameters), this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            IIndex index = _baseIndexableGraph.GetIndex(indexName, indexClass);
            if (null == index)
                return null;

            return new EventIndex(index, this);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            return new EventIndexIterable(_baseIndexableGraph.GetIndices(), this);
        }
    }
}
