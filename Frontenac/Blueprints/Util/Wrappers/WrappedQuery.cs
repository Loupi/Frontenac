using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers
{
    public class WrappedQuery : IQuery
    {
        protected Func<IQuery, IEnumerable<IEdge>> EdgesSelector;
        protected IQuery Query;
        protected Func<IQuery, IEnumerable<IVertex>> VerticesSelector;

        public WrappedQuery(IQuery query, Func<IQuery, IEnumerable<IEdge>> edgesSelector,
                            Func<IQuery, IEnumerable<IVertex>> verticesSelector)
        {
            Contract.Requires(query != null);
            Contract.Requires(edgesSelector != null);
            Contract.Requires(verticesSelector != null);

            Query = query;
            EdgesSelector = edgesSelector;
            VerticesSelector = verticesSelector;
        }

        public IQuery Has(string key, object value)
        {
            Query = Query.Has(key, value);
            return this;
        }

        public IQuery Has<T>(string key, Compare compare, T value)
        {
            Query = Query.Has(key, compare, value);
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue)
        {
            Query = Query.Interval(key, startValue, endValue);
            return this;
        }

        public IQuery Limit(long max)
        {
            Query = Query.Limit(max);
            return this;
        }

        public IEnumerable<IEdge> Edges()
        {
            return EdgesSelector(Query);
        }

        public IEnumerable<IVertex> Vertices()
        {
            return VerticesSelector(Query);
        }
    }
}