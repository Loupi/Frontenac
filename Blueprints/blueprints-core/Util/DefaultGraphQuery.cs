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
        readonly Graph _Graph;

        public DefaultGraphQuery(Graph graph)
        {
            _Graph = graph;
        }

        GraphQuery GraphQuery.Has(string key, object value)
        {
            this.HasContainers.Add(new HasContainer(key, value, Compare.EQUAL));
            return this;
        }

        GraphQuery GraphQuery.Has<T>(string key, Compare compare, T value)
        {
            this.HasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        GraphQuery GraphQuery.Interval<T>(string key, T startValue, T endValue)
        {
            this.HasContainers.Add(new HasContainer(key, startValue, Compare.GREATER_THAN_EQUAL));
            this.HasContainers.Add(new HasContainer(key, endValue, Compare.LESS_THAN));
            return this;
        }

        GraphQuery GraphQuery.Limit(long max)
        {
            _Limit = max;
            return this;
        }

        public override Query Has(string key, object value)
        {
            return (this as GraphQuery).Has(key, value);
        }

        public override Query Has<T>(string key, Compare compare, T value)
        {
            return (this as GraphQuery).Has<T>(key, compare, value);
        }

        public override Query Interval<T>(string key, T startValue, T endValue)
        {
            return (this as GraphQuery).Interval(key, startValue, endValue);
        }

        public override IEnumerable<Edge> Edges()
        {
            return new DefaultGraphQueryIterable<Edge>(this, GetElementIterable<Edge>(typeof(Edge)));
        }

        public override IEnumerable<Vertex> Vertices()
        {
            return new DefaultGraphQueryIterable<Vertex>(this, GetElementIterable<Vertex>(typeof(Vertex)));
        }

        public override Query Limit(long max)
        {
            return (this as GraphQuery).Limit(max);
        }

        private class DefaultGraphQueryIterable<T> : IEnumerable<T> where T : Element
        {
            DefaultGraphQuery _DefaultGraphQuery;
            IEnumerable<T> _Iterable = null;
            T _NextElement = default(T);
            long _Count = 0;

            public DefaultGraphQueryIterable(DefaultGraphQuery defaultGraphQuery, IEnumerable<T> iterable)
            {
                _DefaultGraphQuery = defaultGraphQuery;
                _Iterable = iterable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (this.LoadNext()) yield return _NextElement;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<T>).GetEnumerator();
            }

            private bool LoadNext()
            {
                _NextElement = default(T);
                if (_Count >= _DefaultGraphQuery._Limit)
                    return false;

                foreach (T element in _Iterable)
                {
                    bool filter = false;

                    foreach (HasContainer hasContainer in _DefaultGraphQuery.HasContainers)
                    {
                        if (!hasContainer.IsLegal(element))
                        {
                            filter = true;
                            break;
                        }
                    }

                    if (!filter)
                    {
                        _NextElement = element;
                        _Count++;
                        return true;
                    }
                }
                return false;
            }
        }

        private IEnumerable<T> GetElementIterable<T>(Type elementClass) where T : Element
        {
            if (_Graph is KeyIndexableGraph)
            {
                IEnumerable<string> keys = (_Graph as KeyIndexableGraph).GetIndexedKeys(elementClass);
                foreach (HasContainer hasContainer in HasContainers)
                {
                    if (hasContainer.Compare == Compare.EQUAL && hasContainer.Value != null && keys.Contains(hasContainer.Key))
                    {
                        if (typeof(Vertex).IsAssignableFrom(elementClass))
                            return (IEnumerable<T>)_Graph.GetVertices(hasContainer.Key, hasContainer.Value);
                        else
                            return (IEnumerable<T>)_Graph.GetEdges(hasContainer.Key, hasContainer.Value);
                    }
                }
            }

            foreach (HasContainer hasContainer in HasContainers)
            {
                if (hasContainer.Compare == Compare.EQUAL)
                {
                    if (typeof(Vertex).IsAssignableFrom(elementClass))
                        return (IEnumerable<T>)_Graph.GetVertices(hasContainer.Key, hasContainer.Value);
                    else
                        return (IEnumerable<T>)_Graph.GetEdges(hasContainer.Key, hasContainer.Value);
                }
            }

            if (typeof(Vertex).IsAssignableFrom(elementClass))
                return (IEnumerable<T>)_Graph.GetVertices();
            else
                return (IEnumerable<T>)_Graph.GetEdges();
        }
    }
}
