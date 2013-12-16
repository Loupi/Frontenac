using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerVertex : TinkerElement, IVertex
    {
        public Dictionary<string, HashSet<IEdge>> InEdges = new Dictionary<string, HashSet<IEdge>>();
        public Dictionary<string, HashSet<IEdge>> OutEdges = new Dictionary<string, HashSet<IEdge>>();

        public TinkerVertex(string id, TinkerGraph graph)
            : base(id, graph)
        {
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            if (direction == Direction.Out)
                return FilterEdgesByLabel(OutEdges, labels);
            if (direction == Direction.In)
                return FilterEdgesByLabel(InEdges, labels);
            return
                new MultiIterable<IEdge>(new List<IEnumerable<IEdge>>
                    {
                        FilterEdgesByLabel(InEdges, labels),
                        FilterEdgesByLabel(OutEdges, labels)
                    });
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            return new DefaultVertexQuery(this);
        }

        public IEdge AddEdge(string label, IVertex inVertex)
        {
            return Graph.AddEdge(null, this, inVertex, label);
        }

        private static IEnumerable<IEdge> FilterEdgesByLabel(IDictionary<string, HashSet<IEdge>> edgesToGet,
                                                             params string[] labels)
        {
            Contract.Requires(edgesToGet != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            if (labels.Length == 0)
            {
                var totalEdgesList = new List<IEdge>();
                foreach (var edges in edgesToGet.Values)
                    totalEdgesList.AddRange(edges);

                return totalEdgesList;
            }
            if (labels.Length == 1)
            {
                var edges = edgesToGet.Get(labels[0]);
                return null == edges ? Enumerable.Empty<IEdge>() : new List<IEdge>(edges);
            }
            var totalEdges = new List<IEdge>();
            foreach (var edges in labels.Select(edgesToGet.Get).Where(edges => null != edges))
            {
                totalEdges.AddRange(edges);
            }
            return totalEdges;
        }

        public override string ToString()
        {
            return this.VertexString();
        }

        public void AddOutEdge(string label, IEdge edge)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(edge != null);

            var edges = OutEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<IEdge>();
                OutEdges.Put(label, edges);
            }
            edges.Add(edge);
        }

        public void AddInEdge(string label, IEdge edge)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(edge != null);

            var edges = InEdges.Get(label);
            if (null == edges)
            {
                edges = new HashSet<IEdge>();
                InEdges.Put(label, edges);
            }
            edges.Add(edge);
        }
    }
}