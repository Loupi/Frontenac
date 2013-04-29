using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// For those graph engines that do not support the low-level querying of the edges of a vertex, then DefaultVertexQuery can be used.
    /// DefaultVertexQuery assumes, at minimum, that Vertex.getOutEdges() and Vertex.getInEdges() is implemented by the respective Vertex.
    /// </summary>
    public class DefaultVertexQuery : DefaultQuery, IVertexQuery
    {
        readonly IVertex _vertex;

        public DefaultVertexQuery(IVertex vertex)
        {
            _vertex = vertex;
        }

        public override IQuery Has(string key, object value)
        {
            HasContainers.Add(new HasContainer(key, value, Compare.Equal));
            return this;
        }

        public override IQuery Has<T>(string key, Compare compare, T value)
        {
            HasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        public override IQuery Interval<T>(string key, T startValue, T endValue)
        {
            HasContainers.Add(new HasContainer(key, startValue, Compare.GreaterThanEqual));
            HasContainers.Add(new HasContainer(key, endValue, Compare.LessThan));
            return this;
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultVertexQueryIterable<IEdge>(this, false);
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultVertexQueryIterable<IVertex>(this, true);
        }

        public override IQuery Limit(long max)
        {
            Innerlimit = max;
            return this;
        }

        public new IVertexQuery Direction(Direction direction)
        {
            base.Direction = direction;
            return this;
        }

        public new IVertexQuery Labels(params string[] labels)
        {
            base.Labels = labels;
            return this;
        }

        public long Count()
        {
            return Edges().LongCount();
        }

        public object VertexIds()
        {
            return Vertices().Select(vertex => vertex.GetId()).ToList();
        }

        class DefaultVertexQueryIterable<T> : IEnumerable<T> where T : IElement
        {
            readonly DefaultVertexQuery _defaultVertexQuery;
            readonly IEnumerator<IEdge> _itty;
            readonly bool _forVertex;
            IEdge _nextEdge;
            long _count;

            public DefaultVertexQueryIterable(DefaultVertexQuery defaultVertexQuery, bool forVertex)
            {
                _defaultVertexQuery = defaultVertexQuery;
                _forVertex = forVertex;
                _itty = _defaultVertexQuery._vertex.GetEdges(((DefaultQuery) _defaultVertexQuery).Direction, ((DefaultQuery) _defaultVertexQuery).Labels).GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext())
                {
                    IEdge temp = _nextEdge;
                    _nextEdge = null;
                    if (_forVertex)
                    {
                        if (((DefaultQuery) _defaultVertexQuery).Direction == Blueprints.Direction.Out)
                            yield return (T)temp.GetVertex(Blueprints.Direction.In);
                        else if (((DefaultQuery) _defaultVertexQuery).Direction == Blueprints.Direction.In)
                            yield return (T)temp.GetVertex(Blueprints.Direction.Out);
                        else
                        {
                            if (temp.GetVertex(Blueprints.Direction.Out).Equals(_defaultVertexQuery._vertex))
                                yield return (T)temp.GetVertex(Blueprints.Direction.In);
                            else
                                yield return (T)temp.GetVertex(Blueprints.Direction.Out);
                        }
                    }
                    else
                        yield return (T)temp;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool LoadNext()
            {
                _nextEdge = null;
                if (_count >= _defaultVertexQuery.Innerlimit) return false;
                while (_itty.MoveNext())
                {
                    IEdge edge = _itty.Current;
                    bool filter = _defaultVertexQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(edge));
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
