using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers
{
    /// <summary>
    ///     A WrapperQuery is useful for wrapping the construction and results of a Vertex.query().
    ///     Any necessary Iterable wrapping must occur when Vertex.vertices() or Vertex.edges() is called.
    /// </summary>
    public class WrapperVertexQuery : IVertexQuery
    {
        protected Func<IVertexQuery, IEnumerable<IEdge>> EdgesSelector;
        protected IVertexQuery Query;
        protected Func<IVertexQuery, IEnumerable<IVertex>> VerticesSelector;

        public WrapperVertexQuery(IVertexQuery query, Func<IVertexQuery, IEnumerable<IEdge>> edgesSelector,
                                  Func<IVertexQuery, IEnumerable<IVertex>> verticesSelector)
        {
            Contract.Requires(query != null);
            Contract.Requires(edgesSelector != null);
            Contract.Requires(verticesSelector != null);

            Query = query;
            EdgesSelector = edgesSelector;
            VerticesSelector = verticesSelector;
        }

        public IVertexQuery Direction(Direction direction)
        {
            Query = Query.Direction(direction);
            return this;
        }

        public IVertexQuery Labels(params string[] labels)
        {
            Query = Query.Labels(labels);
            return this;
        }

        public long Count()
        {
            return Query.Count();
        }

        public IEnumerable<object> VertexIds()
        {
            return Query.VertexIds();
        }

        public IQuery Has(string key, object value)
        {
            Query = Query.Has(key, value) as IVertexQuery;
            return this;
        }

        public IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            Query = Query.Has(key, compare, value) as IVertexQuery;
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            Query = Query.Interval(key, startValue, endValue) as IVertexQuery;
            return this;
        }

        public IQuery Limit(long max)
        {
            Query = Query.Limit(max) as IVertexQuery;
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