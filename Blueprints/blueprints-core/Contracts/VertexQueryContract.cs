using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IVertexQuery))]
    public abstract class VertexQueryContract : IVertexQuery
    {
        public IVertexQuery Direction(Direction direction)
        {
            Contract.Ensures(Contract.Result<IVertexQuery>() != null);
            return null;
        }

        public IVertexQuery Labels(params string[] labels)
        {
            Contract.Ensures(Contract.Result<IVertexQuery>() != null);
            return null;
        }

        public long Count()
        {
            Contract.Ensures(Contract.Result<long>() >= 0);
            return default(long);
        }

        public IEnumerable<object> VertexIds()
        {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return null;
        }

        public abstract IQuery Has(string key, object value);
        public abstract IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>;
        public abstract IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;
        public abstract IEnumerable<IEdge> Edges();
        public abstract IEnumerable<IVertex> Vertices();
        public abstract IQuery Limit(long max);
    }
}