using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Grave.Indexing;

namespace Grave
{
    public class GraveQuery : IQuery
    {
        private readonly GraveGraph _graph;
        private readonly IndexingService _indexingService;
        private readonly List<GraveQueryElement> _queryElements = new List<GraveQueryElement>();
        private long _limit = 1000;

        public GraveQuery(GraveGraph graph, IndexingService indexingService)
        {
            Contract.Requires(graph != null);
            Contract.Requires(indexingService != null);

            _graph = graph;
            _indexingService = indexingService;
        }

        public IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            _queryElements.Add(new GraveComparableQueryElement(key, compare, value));
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            _queryElements.Add(new GraveIntervalQueryElement(key, startValue, endValue));
            return this;
        }

        public IQuery Limit(long max)
        {
            _limit = max;
            return this;
        }

        public IQuery Has(string key, object value)
        {
            return Has(key, Compare.Equal, value as IComparable<object>);
        }

        public IEnumerable<IEdge> Edges()
        {
            var ids = _indexingService.Query(typeof (IEdge), _queryElements, (int) _limit);
            return ids.Select(id => _graph.GetEdge(id)).Where(edge => edge != null);
        }

        public IEnumerable<IVertex> Vertices()
        {
            return Edges().Select(edge => edge.GetVertex(Direction.Out));
        }
    }

    public abstract class GraveQueryElement
    {
        private string _key;

        protected GraveQueryElement(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            Key = key;
        }

        public string Key
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _key;
            }
            private set
            {
                Contract.Requires(value != null);
                _key = value;
            }
        }
    }

    internal class GraveComparableQueryElement : GraveQueryElement
    {
        public GraveComparableQueryElement(string key, Compare comparison, object value) : base(key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            Comparison = comparison;
            Value = value;
        }

        public Compare Comparison { get; private set; }
        public object Value { get; private set; }
    }

    internal class GraveIntervalQueryElement : GraveQueryElement
    {
        private object _endValue;
        private object _startValue;

        public GraveIntervalQueryElement(string key, object startValue, object endValue) : base(key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(startValue != null);
            Contract.Requires(endValue != null);

            EndValue = endValue;
            StartValue = startValue;
        }

        public object StartValue
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return _startValue;
            }
            private set
            {
                Contract.Requires(value != null);
                _startValue = value;
            }
        }

        public object EndValue
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return _endValue;
            }
            private set
            {
                Contract.Requires(value != null);
                _endValue = value;
            }
        }
    }
}