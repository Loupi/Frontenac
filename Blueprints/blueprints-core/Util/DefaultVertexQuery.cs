using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// For those graph engines that do not support the low-level querying of the edges of a vertex, then DefaultVertexQuery can be used.
    /// DefaultVertexQuery assumes, at minimum, that Vertex.getOutEdges() and Vertex.getInEdges() is implemented by the respective Vertex.
    /// </summary>
    public class DefaultVertexQuery : DefaultQuery, VertexQuery
    {
        readonly Vertex _vertex;

        public DefaultVertexQuery(Vertex vertex)
        {
            _vertex = vertex;
        }

        public override Query has(string key, object value)
        {
            this.hasContainers.Add(new HasContainer(key, value, Compare.EQUAL));
            return this;
        }

        public override Query has<T>(string key, Compare compare, T value)
        {
            this.hasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        public override Query interval<T>(string key, T startValue, T endValue)
        {
            this.hasContainers.Add(new HasContainer(key, startValue, Compare.GREATER_THAN_EQUAL));
            this.hasContainers.Add(new HasContainer(key, endValue, Compare.LESS_THAN));
            return this;
        }

        public override IEnumerable<Edge> edges()
        {
            return new DefaultVertexQueryIterable<Edge>(this, false);
        }

        public override IEnumerable<Vertex> vertices()
        {
            return new DefaultVertexQueryIterable<Vertex>(this, true);
        }

        public override Query limit(long max)
        {
            _limit = max;
            return this;
        }

        public new VertexQuery direction(Direction direction)
        {
            base.direction = direction;
            return this;
        }

        public VertexQuery labels(params string[] labels)
        {
            _labels = labels;
            return this;
        }

        public long count()
        {
            return this.edges().LongCount();
        }

        public object vertexIds()
        {
            List<object> list = new List<object>();
            foreach (Vertex vertex in this.vertices())
                list.Add(vertex.getId());
            return list;
        }

        class DefaultVertexQueryIterable<T> : IEnumerable<T> where T : Element
        {
            readonly DefaultVertexQuery _defaultVertexQuery;
            readonly IEnumerator<Edge> _itty;
            readonly bool _forVertex;
            Edge _nextEdge = null;
            long _count = 0;

            public DefaultVertexQueryIterable(DefaultVertexQuery defaultVertexQuery, bool forVertex)
            {
                _defaultVertexQuery = defaultVertexQuery;
                _forVertex = forVertex;
                _itty = _defaultVertexQuery._vertex.getEdges(((DefaultQuery) _defaultVertexQuery).direction, _defaultVertexQuery._labels).GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (this.loadNext())
                {
                    Edge temp = _nextEdge;
                    _nextEdge = null;
                    if (_forVertex)
                    {
                        if (((DefaultQuery) _defaultVertexQuery).direction == Frontenac.Blueprints.Direction.OUT)
                            yield return (T)temp.getVertex(Frontenac.Blueprints.Direction.IN);
                        else if (((DefaultQuery) _defaultVertexQuery).direction == Frontenac.Blueprints.Direction.IN)
                            yield return (T)temp.getVertex(Frontenac.Blueprints.Direction.OUT);
                        else
                        {
                            if (temp.getVertex(Frontenac.Blueprints.Direction.OUT).Equals(_defaultVertexQuery._vertex))
                                yield return (T)temp.getVertex(Frontenac.Blueprints.Direction.IN);
                            else
                                yield return (T)temp.getVertex(Frontenac.Blueprints.Direction.OUT);
                        }
                    }
                    else
                        yield return (T)temp;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<T>).GetEnumerator();
            }

            private bool loadNext()
            {
                _nextEdge = null;
                if (_count >= _defaultVertexQuery._limit) return false;
                while (_itty.MoveNext())
                {
                    Edge edge = _itty.Current;
                    bool filter = false;
                    foreach (HasContainer hasContainer in _defaultVertexQuery.hasContainers)
                    {
                        if (!hasContainer.isLegal(edge))
                        {
                            filter = true;
                            break;
                        }
                    }
                    if (!filter)
                    {
                        _nextEdge = edge;
                        _count++;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
