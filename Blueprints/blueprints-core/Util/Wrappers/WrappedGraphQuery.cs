using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers
{
    public class WrappedGraphQuery : GraphQuery
    {
        protected GraphQuery query;
        protected Func<GraphQuery, IEnumerable<Edge>> edgesSelector;
        protected Func<GraphQuery, IEnumerable<Vertex>> verticesSelector;

        public WrappedGraphQuery(GraphQuery query, Func<GraphQuery, IEnumerable<Edge>> edgesSelector, Func<GraphQuery, IEnumerable<Vertex>> verticesSelector)
        {
            this.query = query;
            this.edgesSelector = edgesSelector;
            this.verticesSelector = verticesSelector;
        }

        public GraphQuery has(string key, object value)
        {
            query = query.has(key, value);
            return this;
        }

        public GraphQuery has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            query = query.has(key, compare, value);
            return this;
        }

        public GraphQuery interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            query = query.interval(key, startValue, endValue);
            return this;
        }

        public GraphQuery limit(long max)
        {
            query = query.limit(max);
            return this;
        }

        Query Query.has(string key, object value)
        {
            return this.has(key, value);
        }

        Query Query.has<T>(string key, Compare compare, T value)
        {
            return this.has(key, compare, value);
        }

        Query Query.interval<T>(string key, T startValue, T endValue)
        {
            return this.interval(key, startValue, endValue);
        }

        Query Query.limit(long max)
        {
            return this.limit(max);
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
