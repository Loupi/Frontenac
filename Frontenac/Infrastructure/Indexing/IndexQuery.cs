using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    public class IndexQuery : IQuery
    {
        private readonly IGraph _graph;
        private readonly IndexingService _indexingService;
        private readonly List<QueryElement> _queryElements = new List<QueryElement>();
        private long _limit = 1000;

        public IndexQuery(IGraph graph, IndexingService indexingService)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));

            _graph = graph;
            _indexingService = indexingService;
        }

        public IQuery Has<T>(string key, Compare compare, T value)
        {
            QueryContract.ValidateHas(key, compare, value);

            _queryElements.Add(new ComparableQueryElement(key, compare, value));
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue)
        {
            QueryContract.ValidateInterval(key, startValue, endValue);

            _queryElements.Add(new IntervalQueryElement(key, startValue, endValue));
            return this;
        }

        public IQuery Limit(long max)
        {
            QueryContract.ValidateLimit(max);

            _limit = max;
            return this;
        }

        public IQuery Has(string key, object value)
        {
            QueryContract.ValidateHas(key, value);

            return Has(key, Compare.Equal, value as IComparable<object>);
        }

        public IEnumerable<IEdge> Edges()
        {
            var ids = _indexingService.Query(typeof(IEdge), _queryElements, (int)_limit);
            return ids.Select(id => _graph.GetEdge(id));
        }

        public IEnumerable<IVertex> Vertices()
        {
            return Edges().Select(edge => edge.GetVertex(Direction.Out));
        }
    }
}
