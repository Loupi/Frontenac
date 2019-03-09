using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> Both(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.In(labels).Concat(vertex.Out(labels));
        }

        public static IEnumerable<IVertex> Both(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.In(branchFactor, labels).Concat(vertex.Out(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IEdge> BothE(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.InE(labels).Concat(vertex.OutE(labels));
        }

        public static IEnumerable<IEdge> BothE(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.InE(branchFactor, labels).Concat(vertex.OutE(branchFactor, labels)).Take(branchFactor);
        }
    }
}
