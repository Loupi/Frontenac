using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    class TinkerVertex : TinkerElement, IVertex
    {
        public Dictionary<string, HashSet<IEdge>> OutEdges = new Dictionary<string, HashSet<IEdge>>();
        public Dictionary<string, HashSet<IEdge>> InEdges = new Dictionary<string, HashSet<IEdge>>();

        public TinkerVertex(string id, TinkerGraph graph)
            : base(id, graph)
        {

        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            if (direction == Direction.Out)
                return GetOutEdges(labels);
            if (direction == Direction.In)
                return GetInEdges(labels);
            return new MultiIterable<IEdge>(new List<IEnumerable<IEdge>> { GetInEdges(labels), GetOutEdges(labels) });
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        IEnumerable<IEdge> GetInEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                var totalEdgesList = new List<IEdge>();
                foreach (var edges in InEdges.Values)
                    totalEdgesList.AddRange(edges);

                return totalEdgesList;
            }
            if (labels.Length == 1)
            {
                HashSet<IEdge> edges = InEdges.Get(labels[0]);
                if (null == edges)
                    return Enumerable.Empty<IEdge>();
                return new List<IEdge>(edges);
            }
            var totalEdges = new List<IEdge>();
            foreach (string label in labels)
            {
                HashSet<IEdge> edges = InEdges.Get(label);
                if (null != edges)
                    totalEdges.AddRange(edges);
            }
            return totalEdges;
        }

        IEnumerable<IEdge> GetOutEdges(params string[] labels)
        {
            if (labels.Length == 0)
            {
                var totalEdgesList = new List<IEdge>();
                foreach (var edges in OutEdges.Values)
                    totalEdgesList.AddRange(edges);

                return totalEdgesList;
            }
            if (labels.Length == 1)
            {
                HashSet<IEdge> edges = OutEdges.Get(labels[0]);
                if (null == edges)
                    return Enumerable.Empty<IEdge>();
                return new List<IEdge>(edges);
            }
            var totalEdges = new List<IEdge>();
            foreach (string label in labels)
            {
                HashSet<IEdge> edges = OutEdges.Get(label);
                if (null != edges)
                    totalEdges.AddRange(edges);
            }
            return totalEdges;
        }

        public IVertexQuery Query()
        {
            return new DefaultVertexQuery(this);
        }

        public override string ToString()
        {
            return StringFactory.VertexString(this);
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            return Graph.AddEdge(null, this, inVertex, label);
        }

        public void AddOutEdge(string label, IEdge edge)
        {
            HashSet<IEdge> edges = OutEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<IEdge>();
                OutEdges.Put(label, edges);
            }
            edges.Add(edge);
        }

        public void AddInEdge(string label, IEdge edge)
        {
            HashSet<IEdge> edges = InEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<IEdge>();
                InEdges.Put(label, edges);
            }
            edges.Add(edge);
        }
    }
}
