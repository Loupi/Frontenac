using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdgeIndex : IIndex
    {
        protected readonly IIndex BaseIndex;
        protected readonly IdGraph IdGraph;

        public IdEdgeIndex(IIndex baseIndex, IdGraph idGraph)
        {
            Contract.Requires(baseIndex != null);
            Contract.Requires(idGraph != null);

            IdGraph = idGraph;
            BaseIndex = baseIndex;
        }

        public string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return BaseIndex.Name;
            }
        }

        public Type Type => BaseIndex.Type;

        public void Put(string key, object value, IElement element)
        {
            BaseIndex.Put(key, value, GetBaseElement(element));
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            return new IdEdgeIterable(BaseIndex.Get(key, value), IdGraph);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            return new IdEdgeIterable(BaseIndex.Query(key, value), IdGraph);
        }

        public long Count(string key, object value)
        {
            return BaseIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            BaseIndex.Remove(key, value, GetBaseElement(element));
        }

        public override string ToString()
        {
            return this.IndexString();
        }

        private static IEdge GetBaseElement(IElement e)
        {
            Contract.Requires(e is IdEdge);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return ((IdEdge) e).GetBaseEdge();
        }
    }
}