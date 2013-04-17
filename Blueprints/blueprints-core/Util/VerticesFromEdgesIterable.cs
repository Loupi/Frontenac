using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// VerticesFromEdgesIterable is a helper class that returns vertices that meet the direction/label criteria of the incident edges.
    /// </summary>
    public class VerticesFromEdgesIterable : IEnumerable<Vertex>
    {
        readonly IEnumerable<Edge> _Iterable;
        readonly Direction _Direction;
        readonly Vertex _Vertex;

        public VerticesFromEdgesIterable(Vertex vertex, Direction direction, params string[] labels)
        {
            _Direction = direction;
            _Vertex = vertex;
            _Iterable = vertex.GetEdges(direction, labels);
        }

        public IEnumerator<Vertex> GetEnumerator()
        {
            foreach (Edge edge in _Iterable)
            {
                if (_Direction == Direction.OUT)
                    yield return edge.GetVertex(Direction.IN);
                else if (_Direction == Direction.IN)
                {
                    yield return edge.GetVertex(Direction.OUT);
                }
                else
                {
                    if (edge.GetVertex(Direction.IN).Equals(_Vertex))
                        yield return edge.GetVertex(Direction.OUT);
                    else
                        yield return edge.GetVertex(Direction.IN);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
