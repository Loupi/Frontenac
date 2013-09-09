using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(vertex != null);

            _vertex = vertex;
        }

        public override IEnumerable<IEdge> Edges()
        {
            return new DefaultVertexQueryIterable<IEdge>(this, false);
        }

        public override IEnumerable<IVertex> Vertices()
        {
            return new DefaultVertexQueryIterable<IVertex>(this, true);
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

        public IEnumerable<object> VertexIds()
        {
            return Vertices().Select(vertex => vertex.Id);
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
                Contract.Requires(defaultVertexQuery != null);

                _defaultVertexQuery = defaultVertexQuery;
                _forVertex = forVertex;
                _itty = _defaultVertexQuery._vertex.GetEdges(((DefaultQuery) _defaultVertexQuery).Direction, ((DefaultQuery) _defaultVertexQuery).Labels).GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (LoadNext())
                {
                    var temp = _nextEdge;
                    _nextEdge = null;
                    if (_forVertex && temp != null)
                    {
                        switch (((DefaultQuery) _defaultVertexQuery).Direction)
                        {
                            case Blueprints.Direction.Out:
                                yield return (T) temp.GetVertex(Blueprints.Direction.In);
                                break;
                            case Blueprints.Direction.In:
                                yield return (T) temp.GetVertex(Blueprints.Direction.Out);
                                break;
                            default:
                                if (temp.GetVertex(Blueprints.Direction.Out).Equals(_defaultVertexQuery._vertex))
                                    yield return (T) temp.GetVertex(Blueprints.Direction.In);
                                else
                                    yield return (T) temp.GetVertex(Blueprints.Direction.Out);
                                break;
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
                    var edge = _itty.Current;
                    var filter = _defaultVertexQuery.HasContainers.Any(hasContainer => !hasContainer.IsLegal(edge));
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
