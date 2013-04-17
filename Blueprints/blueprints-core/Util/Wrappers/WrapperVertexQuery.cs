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
        protected VertexQuery _Query;
        protected Func<VertexQuery, IEnumerable<Edge>> _EdgesSelector;
        protected Func<VertexQuery, IEnumerable<Vertex>> _VerticesSelector;

        public WrapperVertexQuery(VertexQuery query, Func<VertexQuery, IEnumerable<Edge>> edgesSelector, Func<VertexQuery, IEnumerable<Vertex>> verticesSelector)
        {
            _Query = query;
            _EdgesSelector = edgesSelector;
            _VerticesSelector = verticesSelector;
        }

        public VertexQuery Direction(Direction direction)
        {
            _Query = _Query.Direction(direction);
            return this;
        }

        public VertexQuery Labels(params string[] labels)
        {
            _Query = _Query.Labels(labels);
            return this;
        }

        public long Count()
        {
            return _Query.Count();
        }

        public object VertexIds()
        {
            return _Query.VertexIds();
        }

        public Query Has(string key, object value)
        {
            _Query = _Query.Has(key, value) as VertexQuery;
            return this;
        }

        public Query Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            _Query = _Query.Has(key, compare, value) as VertexQuery;
            return this;
        }

        public Query Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            _Query = _Query.Interval(key, startValue, endValue) as VertexQuery;
            return this;
        }

        public Query Limit(long max)
        {
            _Query = _Query.Limit(max) as VertexQuery;
            return this;
        }

        public IEnumerable<Edge> Edges()
        {
            return _EdgesSelector(_Query);
        }

        public IEnumerable<Vertex> Vertices()
        {
            return _VerticesSelector(_Query);
        }
    }
}
