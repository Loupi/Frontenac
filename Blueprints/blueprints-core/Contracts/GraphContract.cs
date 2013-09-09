using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof(IGraph))]
    public abstract class GraphContract : IGraph
    {
        public Features Features
        {
            get
            {
                Contract.Ensures(Contract.Result<Features>() != null); 
                return null;
            }
        }

        public IVertex AddVertex(object id)
        {
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return null;
        }

        public IVertex GetVertex(object id)
        {
            Contract.Requires(id != null);
            return null;
        }

        public void RemoveVertex(IVertex vertex)
        {
            Contract.Requires(vertex != null);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);
            return null;
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);
            return null;
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Ensures(Contract.Result<IEdge>() != null);
            return null;
        }

        public IEdge GetEdge(object id)
        {
            Contract.Requires(id != null);
            return null;
        }

        public void RemoveEdge(IEdge edge)
        {
            Contract.Requires(edge != null);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);
            return null;
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);
            return null;
        }

        public IQuery Query()
        {
            Contract.Ensures(Contract.Result<IQuery>() != null);
            return null;
        }

        public void Shutdown()
        {
            
        }
    }
}
