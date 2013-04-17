using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers
{
    public class WrappedGraphQuery : GraphQuery
    {
        protected GraphQuery _Query;
        protected Func<GraphQuery, IEnumerable<Edge>> _EdgesSelector;
        protected Func<GraphQuery, IEnumerable<Vertex>> _VerticesSelector;

        public WrappedGraphQuery(GraphQuery query, Func<GraphQuery, IEnumerable<Edge>> edgesSelector, Func<GraphQuery, IEnumerable<Vertex>> verticesSelector)
        {
            _Query = query;
            _EdgesSelector = edgesSelector;
            _VerticesSelector = verticesSelector;
        }

        public GraphQuery Has(string key, object value)
        {
            _Query = _Query.Has(key, value);
            return this;
        }

        public GraphQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            _Query = _Query.Has(key, compare, value);
            return this;
        }

        public GraphQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            _Query = _Query.Interval(key, startValue, endValue);
            return this;
        }

        public GraphQuery Limit(long max)
        {
            _Query = _Query.Limit(max);
            return this;
        }

        Query Query.Has(string key, object value)
        {
            return this.Has(key, value);
        }

        Query Query.Has<T>(string key, Compare compare, T value)
        {
            return this.Has(key, compare, value);
        }

        Query Query.Interval<T>(string key, T startValue, T endValue)
        {
            return this.Interval(key, startValue, endValue);
        }

        Query Query.Limit(long max)
        {
            return this.Limit(max);
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
