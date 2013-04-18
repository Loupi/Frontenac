using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// For those graph engines that do not support the low-level querying of the vertices or edges, then DefaultGraphQuery can be used.
    /// DefaultGraphQuery assumes, at minimum, that Graph.getVertices() and Graph.getEdges() is implemented by the respective Graph.
    /// </summary>
    public class DefaultGraphQuery : DefaultQuery, GraphQuery
    {
        readonly Graph _graph;

        public DefaultGraphQuery(Graph graph)
        {
            _graph = graph;
        }

        GraphQuery GraphQuery.has(string key, object value)
        {
            this.hasContainers.Add(new HasContainer(key, value, Compare.EQUAL));
            return this;
        }

        GraphQuery GraphQuery.has<T>(string key, Compare compare, T value)
        {
            this.hasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        GraphQuery GraphQuery.interval<T>(string key, T startValue, T endValue)
        {
            this.hasContainers.Add(new HasContainer(key, startValue, Compare.GREATER_THAN_EQUAL));
            this.hasContainers.Add(new HasContainer(key, endValue, Compare.LESS_THAN));
            return this;
        }

        GraphQuery GraphQuery.limit(long max)
        {
            _limit = max;
            return this;
        }

        public override Query has(string key, object value)
        {
            return (this as GraphQuery).has(key, value);
        }

        public override Query has<T>(string key, Compare compare, T value)
        {
            return (this as GraphQuery).has<T>(key, compare, value);
        }

        public override Query interval<T>(string key, T startValue, T endValue)
        {
            return (this as GraphQuery).interval(key, startValue, endValue);
        }

        public override IEnumerable<Edge> edges()
        {
            return new DefaultGraphQueryIterable<Edge>(this, getElementIterable<Edge>(typeof(Edge)));
        }

        public override IEnumerable<Vertex> vertices()
        {
            return new DefaultGraphQueryIterable<Vertex>(this, getElementIterable<Vertex>(typeof(Vertex)));
        }

        public override Query limit(long max)
        {
            return (this as GraphQuery).limit(max);
        }

        private class DefaultGraphQueryIterable<T> : IEnumerable<T> where T : Element
        {
            readonly DefaultGraphQuery _defaultGraphQuery;
            readonly IEnumerable<T> _iterable = null;
            T _nextElement = default(T);
            long _count = 0;

            public DefaultGraphQueryIterable(DefaultGraphQuery defaultGraphQuery, IEnumerable<T> iterable)
            {
                _defaultGraphQuery = defaultGraphQuery;
                _iterable = iterable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (this.loadNext()) yield return _nextElement;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<T>).GetEnumerator();
            }

            private bool loadNext()
            {
                _nextElement = default(T);
                if (_count >= _defaultGraphQuery._limit)
                    return false;

                foreach (T element in _iterable)
                {
                    bool filter = false;

                    foreach (HasContainer hasContainer in _defaultGraphQuery.hasContainers)
                    {
                        if (!hasContainer.isLegal(element))
                        {
                            filter = true;
                            break;
                        }
                    }

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

        private IEnumerable<T> getElementIterable<T>(Type elementClass) where T : Element
        {
            if (_graph is KeyIndexableGraph)
            {
                IEnumerable<string> keys = (_graph as KeyIndexableGraph).getIndexedKeys(elementClass);
                foreach (HasContainer hasContainer in hasContainers)
                {
                    if (hasContainer.compare == Compare.EQUAL && hasContainer.value != null && keys.Contains(hasContainer.key))
                    {
                        if (typeof(Vertex).IsAssignableFrom(elementClass))
                            return (IEnumerable<T>)_graph.getVertices(hasContainer.key, hasContainer.value);
                        else
                            return (IEnumerable<T>)_graph.getEdges(hasContainer.key, hasContainer.value);
                    }
                }
            }

            foreach (HasContainer hasContainer in hasContainers)
            {
                if (hasContainer.compare == Compare.EQUAL)
                {
                    if (typeof(Vertex).IsAssignableFrom(elementClass))
                        return (IEnumerable<T>)_graph.getVertices(hasContainer.key, hasContainer.value);
                    else
                        return (IEnumerable<T>)_graph.getEdges(hasContainer.key, hasContainer.value);
                }
            }

            if (typeof(Vertex).IsAssignableFrom(elementClass))
                return (IEnumerable<T>)_graph.getVertices();
            else
                return (IEnumerable<T>)_graph.getEdges();
        }
    }
}
