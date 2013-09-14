using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IIndexableGraph))]
    public abstract class IndexableGraphContract : IIndexableGraph
    {
        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(indexClass != null);
            Contract.Requires(indexClass.IsAssignableFrom(typeof (IVertex)) ||
                              indexClass.IsAssignableFrom(typeof (IEdge)));
            return null;
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(indexClass != null);
            Contract.Requires(indexClass.IsAssignableFrom(typeof (IVertex)) ||
                              indexClass.IsAssignableFrom(typeof (IEdge)));
            return null;
        }

        public IEnumerable<IIndex> GetIndices()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IIndex>>() != null);
            return null;
        }

        public void DropIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
        }

        public abstract Features Features { get; }
        public abstract IVertex AddVertex(object id);
        public abstract IVertex GetVertex(object id);
        public abstract void RemoveVertex(IVertex vertex);
        public abstract IEnumerable<IVertex> GetVertices();
        public abstract IEnumerable<IVertex> GetVertices(string key, object value);
        public abstract IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label);
        public abstract IEdge GetEdge(object id);
        public abstract void RemoveEdge(IEdge edge);
        public abstract IEnumerable<IEdge> GetEdges();
        public abstract IEnumerable<IEdge> GetEdges(string key, object value);
        public abstract IQuery Query();
        public abstract void Shutdown();
    }
}