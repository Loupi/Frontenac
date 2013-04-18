using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers
{
    /// <summary>
    /// A WrapperQuery is useful for wrapping the construction and results of a Vertex.query().
    /// Any necessary Iterable wrapping must occur when Vertex.vertices() or Vertex.edges() is called.
    /// </summary>
    public class WrapperVertexQuery : VertexQuery
    {
        protected VertexQuery query;
        protected Func<VertexQuery, IEnumerable<Edge>> edgesSelector;
        protected Func<VertexQuery, IEnumerable<Vertex>> verticesSelector;

        public WrapperVertexQuery(VertexQuery query, Func<VertexQuery, IEnumerable<Edge>> edgesSelector, Func<VertexQuery, IEnumerable<Vertex>> verticesSelector)
        {
            this.query = query;
            this.edgesSelector = edgesSelector;
            this.verticesSelector = verticesSelector;
        }

        public VertexQuery direction(Direction direction)
        {
            query = query.direction(direction);
            return this;
        }

        public VertexQuery labels(params string[] labels)
        {
            query = query.labels(labels);
            return this;
        }

        public long count()
        {
            return query.count();
        }

        public object vertexIds()
        {
            return query.vertexIds();
        }

        public Query has(string key, object value)
        {
            query = query.has(key, value) as VertexQuery;
            return this;
        }

        public Query has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            query = query.has(key, compare, value) as VertexQuery;
            return this;
        }

        public Query interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            query = query.interval(key, startValue, endValue) as VertexQuery;
            return this;
        }

        public Query limit(long max)
        {
            query = query.limit(max) as VertexQuery;
            return this;
        }

        public IEnumerable<Edge> edges()
        {
            return edgesSelector(query);
        }

        public IEnumerable<Vertex> vertices()
        {
            return verticesSelector(query);
        }
    }
}
