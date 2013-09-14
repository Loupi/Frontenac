using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IThreadedTransactionalGraph))]
    public abstract class ThreadedTransactionalGraphContract : IThreadedTransactionalGraph
    {
        public ITransactionalGraph NewTransaction()
        {
            Contract.Ensures(Contract.Result<ITransactionalGraph>() != null);
            return null;
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
        public abstract void Commit();
        public abstract void Rollback();
    }
}