using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    class TinkerVertex : TinkerElement, Vertex
    {
        public Dictionary<string, HashSet<Edge>> _OutEdges = new Dictionary<string, HashSet<Edge>>();
        public Dictionary<string, HashSet<Edge>> _InEdges = new Dictionary<string, HashSet<Edge>>();

        public TinkerVertex(string id, TinkerGraph graph)
            : base(id, graph)
        {

        }

        public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
        {
            if (direction == Direction.OUT)
                return GetOutEdges(labels);
            else if (direction == Direction.IN)
                return GetInEdges(labels);
            else
                return new MultiIterable<Edge>(new List<IEnumerable<Edge>>() { GetInEdges(labels), GetOutEdges(labels) });
        }

        public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        IEnumerable<Edge> GetInEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (var edges in _InEdges.Values)
                    totalEdges.AddRange(edges);

                return totalEdges;
            }
            else if (labels.Length == 1)
            {
                HashSet<Edge> edges = _InEdges.Get(labels[0]);
                if (null == edges)
                    return Enumerable.Empty<Edge>();
                else
                    return new List<Edge>(edges);

            }
            else
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (string label in labels)
                {
                    HashSet<Edge> edges = _InEdges.Get(label);
                    if (null != edges)
                        totalEdges.AddRange(edges);
                }
                return totalEdges;
            }
        }

        IEnumerable<Edge> GetOutEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (var edges in _OutEdges.Values)
                    totalEdges.AddRange(edges);

                return totalEdges;
            }
            else if (labels.Length == 1)
            {
                HashSet<Edge> edges = _OutEdges.Get(labels[0]);
                if (null == edges)
                    return Enumerable.Empty<Edge>();
                else
                    return new List<Edge>(edges);
            }
            else
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (string label in labels)
                {
                    HashSet<Edge> edges = _OutEdges.Get(label);
                    if (null != edges)
                        totalEdges.AddRange(edges);
                }
                return totalEdges;
            }
        }

        public VertexQuery Query()
        {
            return new DefaultVertexQuery(this);
        }

        public override string ToString()
        {
            return StringFactory.VertexString(this);
        }

        public Edge AddEdge(string label, Vertex inVertex)
        {
            return _Graph.AddEdge(null, this, inVertex, label);
        }

        public void AddOutEdge(string label, Edge edge)
        {
            HashSet<Edge> edges = _OutEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<Edge>();
                _OutEdges.Put(label, edges);
            }
            edges.Add(edge);
        }

        public void AddInEdge(string label, Edge edge)
        {
            HashSet<Edge> edges = _InEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<Edge>();
                _InEdges.Put(label, edges);
            }
            edges.Add(edge);
        }
    }
}
