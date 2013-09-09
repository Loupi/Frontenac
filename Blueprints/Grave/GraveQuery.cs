using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Grave.Indexing;

namespace Grave
{
    public class GraveQuery : IQuery
    {
        readonly GraveGraph _graph;
        readonly IndexingService _indexingService;
        readonly List<GraveQueryElement> _queryElements = new List<GraveQueryElement>();
        long _limit = 1000;

        public GraveQuery(GraveGraph graph, IndexingService indexingService)
        {
            _graph = graph;
            _indexingService = indexingService;
        }

        public IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            _queryElements.Add(new GraveComparableQueryElement(key, compare, value));
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            _queryElements.Add(new GraveIntervalQueryElement(key, startValue, endValue));
            return this;
        }

        public IQuery Limit(long max)
        {
            if(max <= 0)
                throw new ArgumentException("max");

            _limit = max;
            return this;
        }

        public IQuery Has(string key, object value)
        {
            if(value != null && !(value is IComparable<object>))
                throw new ArgumentException("value");

            return Has(key, Compare.Equal, value as IComparable<object>);
        }

        public IEnumerable<IEdge> Edges()
        {
            var ids = _indexingService.Query(typeof(IEdge), _queryElements, (int)_limit);
            return ids.Select(id => _graph.GetEdge(id)).Where(edge => edge != null);
        }

        public IEnumerable<IVertex> Vertices()
        {
            return Edges().Select(edge => edge.GetVertex(Direction.Out));
        }
    }

    public abstract class GraveQueryElement
    {
        public string Key { get; private set; }

        protected GraveQueryElement(string key)
        {
            Key = key;
        }
    }

    class GraveComparableQueryElement : GraveQueryElement
    {
        public GraveComparableQueryElement(string key, Compare comparison, object value) : base(key)
        {
            Comparison = comparison;
            Value = value;
        }

        public Compare Comparison { get; private set; }
        public object Value { get; private set; }
    }

    class GraveIntervalQueryElement : GraveQueryElement
    {
        public GraveIntervalQueryElement(string key, object startValue, object endValue) : base(key)
        {
            EndValue = endValue;
            StartValue = startValue;
        }

        public object StartValue { get; private set; }
        public object EndValue { get; private set; }
    }
}
