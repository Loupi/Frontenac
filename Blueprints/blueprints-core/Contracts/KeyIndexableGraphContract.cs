using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof(IKeyIndexableGraph))]
    public abstract class KeyIndexableGraphContract : IKeyIndexableGraph
    {
        public void DropKeyIndex(string key, Type elementClass)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(elementClass != null);
            Contract.Requires(elementClass.IsAssignableFrom(typeof(IVertex)) || elementClass.IsAssignableFrom(typeof(IEdge)));
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(elementClass != null);
            Contract.Requires(elementClass.IsAssignableFrom(typeof(IVertex)) || elementClass.IsAssignableFrom(typeof(IEdge)));
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            Contract.Requires(elementClass != null);
            Contract.Requires(elementClass.IsAssignableFrom(typeof(IVertex)) || elementClass.IsAssignableFrom(typeof(IEdge)));
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
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
    }
}
