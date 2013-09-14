using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof (IQuery))]
    public abstract class QueryContract : IQuery
    {
        public IQuery Has(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IQuery>() != null);
            return null;
        }

        public IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IQuery>() != null);
            return null;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IQuery>() != null);
            return null;
        }

        public IEnumerable<IEdge> Edges()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);
            return null;
        }

        public IEnumerable<IVertex> Vertices()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);
            return null;
        }

        public IQuery Limit(long max)
        {
            Contract.Requires(max > 0);
            Contract.Ensures(Contract.Result<IQuery>() != null);
            return null;
        }
    }
}