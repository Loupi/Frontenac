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
        readonly Vertex _Vertex;

        public DefaultVertexQuery(Vertex vertex)
        {
            _Vertex = vertex;
        }

        public override Query Has(string key, object value)
        {
            this.HasContainers.Add(new HasContainer(key, value, Compare.EQUAL));
            return this;
        }

        public override Query Has<T>(string key, Compare compare, T value)
        {
            this.HasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        public override Query Interval<T>(string key, T startValue, T endValue)
        {
            this.HasContainers.Add(new HasContainer(key, startValue, Compare.GREATER_THAN_EQUAL));
            this.HasContainers.Add(new HasContainer(key, endValue, Compare.LESS_THAN));
            return this;
        }

        public override IEnumerable<Edge> Edges()
        {
            return new DefaultVertexQueryIterable<Edge>(this, false);
        }

        public override IEnumerable<Vertex> Vertices()
        {
            return new DefaultVertexQueryIterable<Vertex>(this, true);
        }

        public override Query Limit(long max)
        {
            base._Limit = max;
            return this;
        }

        public VertexQuery Direction(Direction direction)
        {
            base._Direction = direction;
            return this;
        }

        public VertexQuery Labels(params string[] labels)
        {
            base._Labels = labels;
            return this;
        }

        public long Count()
        {
            return this.Edges().LongCount();
        }

        public object VertexIds()
        {
            List<object> list = new List<object>();
            foreach (Vertex vertex in this.Vertices())
                list.Add(vertex.GetId());
            return list;
        }

        class DefaultVertexQueryIterable<T> : IEnumerable<T> where T : Element
        {
            DefaultVertexQuery _DefaultVertexQuery;
            IEnumerator<Edge> _Itty;
            bool _ForVertex;
            Edge _NextEdge = null;
            long _Count = 0;

            public DefaultVertexQueryIterable(DefaultVertexQuery defaultVertexQuery, bool forVertex)
            {
                _DefaultVertexQuery = defaultVertexQuery;
                _ForVertex = forVertex;
                _Itty = _DefaultVertexQuery._Vertex.GetEdges(_DefaultVertexQuery._Direction, _DefaultVertexQuery._Labels).GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (this.LoadNext())
                {
                    Edge temp = _NextEdge;
                    _NextEdge = null;
                    if (_ForVertex)
                    {
                        if (_DefaultVertexQuery._Direction == Frontenac.Blueprints.Direction.OUT)
                            yield return (T)temp.GetVertex(Frontenac.Blueprints.Direction.IN);
                        else if (_DefaultVertexQuery._Direction == Frontenac.Blueprints.Direction.IN)
                            yield return (T)temp.GetVertex(Frontenac.Blueprints.Direction.OUT);
                        else
                        {
                            if (temp.GetVertex(Frontenac.Blueprints.Direction.OUT).Equals(_DefaultVertexQuery._Vertex))
                                yield return (T)temp.GetVertex(Frontenac.Blueprints.Direction.IN);
                            else
                                yield return (T)temp.GetVertex(Frontenac.Blueprints.Direction.OUT);
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

            private bool LoadNext()
            {
                _NextEdge = null;
                if (_Count >= _DefaultVertexQuery._Limit) return false;
                while (_Itty.MoveNext())
                {
                    Edge edge = _Itty.Current;
                    bool filter = false;
                    foreach (HasContainer hasContainer in _DefaultVertexQuery.HasContainers)
                    {
                        if (!hasContainer.IsLegal(edge))
                        {
                            filter = true;
                            break;
                        }
                    }
                    if (!filter)
                    {
                        _NextEdge = edge;
                        _Count++;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
