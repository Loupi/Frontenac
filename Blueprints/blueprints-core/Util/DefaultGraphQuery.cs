using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// For those graph engines that do not support the low-level querying of the vertices or edges, then DefaultGraphQuery can be used.
    /// DefaultGraphQuery assumes, at minimum, that Graph.getVertices() and Graph.getEdges() is implemented by the respective Graph.
    /// </summary>
    public class DefaultGraphQuery : DefaultQuery, IGraphQuery
    {
        readonly IGraph _graph;

        public DefaultGraphQuery(IGraph graph)
        {
            _graph = graph;
        }

        IGraphQuery IGraphQuery.Has(string key, object value)
        {
            HasContainers.Add(new HasContainer(key, value, Compare.Equal));
            return this;
        }

        IGraphQuery IGraphQuery.Has<T>(string key, Compare compare, T value)
        {
            HasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        IGraphQuery IGraphQuery.Interval<T>(string key, T startValue, T endValue)
        {
            HasContainers.Add(new HasContainer(key, startValue, Compare.GreaterThanEqual));
            HasContainers.Add(new HasContainer(key, endValue, Compare.LessThan));
            return this;
        }

        IGraphQuery IGraphQuery.Limit(long max)
        {
            Innerlimit = max;
            return this;
        }

        public override IQuery Has(string key, object value)
        {
            return (this as IGraphQuery).Has(key, value);
        }

        public override IQuery Has<T>(string key, Compare compare, T value)
        {
            return (this as IGraphQuery).Has(key, compare, value);
        }

        public override IQuery Interval<T>(string key, T startValue, T endValue)
        {
            return (this as IGraphQuery).Interval(key, startValue, endValue);
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultGraphQueryIterable<IEdge>(this, GetElementIterable<IEdge>(typeof(IEdge)));
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultGraphQueryIterable<IVertex>(this, GetElementIterable<IVertex>(typeof(IVertex)));
        }

        public override IQuery Limit(long max)
        {
            return (this as IGraphQuery).Limit(max);
        }

        private class DefaultGraphQueryIterable<T> : IEnumerable<T> where T : IElement
        {
            readonly DefaultGraphQuery _defaultGraphQuery;
            readonly IEnumerable<T> _iterable;
            T _nextElement;
            long _count;

            public DefaultGraphQueryIterable(DefaultGraphQuery defaultGraphQuery, IEnumerable<T> iterable)
            {
                _defaultGraphQuery = defaultGraphQuery;
                _iterable = iterable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext()) yield return _nextElement;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool LoadNext()
            {
                _nextElement = default(T);
                if (_count >= _defaultGraphQuery.Innerlimit)
                    return false;

                foreach (T element in _iterable)
                {
                    bool filter = _defaultGraphQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(element));

                    if (!filter)
                    {
                        _nextElement = element;
                        _count++;
                        return true;
                    }
                }
                return false;
            }
        }

        private IEnumerable<T> GetElementIterable<T>(Type elementClass) where T : IElement
        {
            if (_graph is IKeyIndexableGraph)
            {
                IEnumerable<string> keys = (_graph as IKeyIndexableGraph).GetIndexedKeys(elementClass);
                foreach (HasContainer hasContainer in HasContainers)
                {
                    if (hasContainer.Compare == Compare.Equal && hasContainer.Value != null && keys.Contains(hasContainer.Key))
                    {
                        if (typeof(IVertex).IsAssignableFrom(elementClass))
                            return (IEnumerable<T>)_graph.GetVertices(hasContainer.Key, hasContainer.Value);
                        return (IEnumerable<T>)_graph.GetEdges(hasContainer.Key, hasContainer.Value);
                    }
                }
            }

            foreach (HasContainer hasContainer in HasContainers)
            {
                if (hasContainer.Compare == Compare.Equal)
                {
                    if (typeof(IVertex).IsAssignableFrom(elementClass))
                        return (IEnumerable<T>)_graph.GetVertices(hasContainer.Key, hasContainer.Value);
                    return (IEnumerable<T>)_graph.GetEdges(hasContainer.Key, hasContainer.Value);
                }
            }

            if (typeof(IVertex).IsAssignableFrom(elementClass))
                return (IEnumerable<T>)_graph.GetVertices();
            return (IEnumerable<T>)_graph.GetEdges();
        }
    }
}
