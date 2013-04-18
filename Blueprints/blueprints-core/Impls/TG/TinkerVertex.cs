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
        public Dictionary<string, HashSet<Edge>> outEdges = new Dictionary<string, HashSet<Edge>>();
        public Dictionary<string, HashSet<Edge>> inEdges = new Dictionary<string, HashSet<Edge>>();

        public TinkerVertex(string id, TinkerGraph graph)
            : base(id, graph)
        {

        }

        public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
        {
            if (direction == Direction.OUT)
                return getOutEdges(labels);
            else if (direction == Direction.IN)
                return getInEdges(labels);
            else
                return new MultiIterable<Edge>(new List<IEnumerable<Edge>>() { getInEdges(labels), getOutEdges(labels) });
        }

        public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        IEnumerable<Edge> getInEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (var edges in inEdges.Values)
                    totalEdges.AddRange(edges);

                return totalEdges;
            }
            else if (labels.Length == 1)
            {
                HashSet<Edge> edges = inEdges.get(labels[0]);
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
                    HashSet<Edge> edges = inEdges.get(label);
                    if (null != edges)
                        totalEdges.AddRange(edges);
                }
                return totalEdges;
            }
        }

        IEnumerable<Edge> getOutEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                List<Edge> totalEdges = new List<Edge>();
                foreach (var edges in outEdges.Values)
                    totalEdges.AddRange(edges);

                return totalEdges;
            }
            else if (labels.Length == 1)
            {
                HashSet<Edge> edges = outEdges.get(labels[0]);
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
                    HashSet<Edge> edges = outEdges.get(label);
                    if (null != edges)
                        totalEdges.AddRange(edges);
                }
                return totalEdges;
            }
        }

        public VertexQuery query()
        {
            return new DefaultVertexQuery(this);
        }

        public override string ToString()
        {
            return StringFactory.vertexString(this);
        }

        public Edge addEdge(string label, Vertex inVertex)
        {
            return graph.addEdge(null, this, inVertex, label);
        }

        public void addOutEdge(string label, Edge edge)
        {
            HashSet<Edge> edges = outEdges.get(label);
            if (null == edges)
            {
                edges = new HashSet<Edge>();
                outEdges.put(label, edges);
            }
            edges.Add(edge);
        }

        public void addInEdge(string label, Edge edge)
        {
            HashSet<Edge> edges = inEdges.get(label);
            if (null == edges)
            {
                edges = new HashSet<Edge>();
                inEdges.put(label, edges);
            }
            edges.Add(edge);
        }
    }
}
