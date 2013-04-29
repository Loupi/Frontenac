using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers
{
    public class WrappedGraphQuery : IGraphQuery
    {
        protected IGraphQuery Query;
        protected Func<IGraphQuery, IEnumerable<IEdge>> EdgesSelector;
        protected Func<IGraphQuery, IEnumerable<IVertex>> VerticesSelector;

        public WrappedGraphQuery(IGraphQuery query, Func<IGraphQuery, IEnumerable<IEdge>> edgesSelector, Func<IGraphQuery, IEnumerable<IVertex>> verticesSelector)
        {
            Query = query;
            EdgesSelector = edgesSelector;
            VerticesSelector = verticesSelector;
        }

        public IGraphQuery Has(string key, object value)
        {
            Query = Query.Has(key, value);
            return this;
        }

        public IGraphQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            Query = Query.Has(key, compare, value);
            return this;
        }

        public IGraphQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            Query = Query.Interval(key, startValue, endValue);
            return this;
        }

        public IGraphQuery Limit(long max)
        {
            Query = Query.Limit(max);
            return this;
        }

        IQuery IQuery.Has(string key, object value)
        {
            return Has(key, value);
        }

        IQuery IQuery.Has<T>(string key, Compare compare, T value)
        {
            return Has(key, compare, value);
        }

        IQuery IQuery.Interval<T>(string key, T startValue, T endValue)
        {
            return Interval(key, startValue, endValue);
        }

        IQuery IQuery.Limit(long max)
        {
            return Limit(max);
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
