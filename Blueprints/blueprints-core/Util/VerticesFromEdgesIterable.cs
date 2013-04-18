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
        readonly IEnumerable<Edge> _iterable;
        readonly Direction _direction;
        readonly Vertex _vertex;

        public VerticesFromEdgesIterable(Vertex vertex, Direction direction, params string[] labels)
        {
            _direction = direction;
            _vertex = vertex;
            _iterable = vertex.getEdges(direction, labels);
        }

        public IEnumerator<Vertex> GetEnumerator()
        {
            foreach (Edge edge in _iterable)
            {
                if (_direction == Direction.OUT)
                    yield return edge.getVertex(Direction.IN);
                else if (_direction == Direction.IN)
                {
                    yield return edge.getVertex(Direction.OUT);
                }
                else
                {
                    if (edge.getVertex(Direction.IN).Equals(_vertex))
                        yield return edge.getVertex(Direction.OUT);
                    else
                        yield return edge.getVertex(Direction.IN);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
