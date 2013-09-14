using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util
{
    public static class VertexHelper
    {
        /// <summary>
        ///     Test whether the two vertices have equal properties and edge sets.
        /// </summary>
        /// <param name="a">the first vertex </param>
        /// <param name="b">the second vertex </param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids </param>
        /// <returns>whether the two vertices are semantically the same </returns>
        public static bool HaveEqualNeighborhood(IVertex a, IVertex b, bool checkIdEquality)
        {
            Contract.Requires(a != null);
            Contract.Requires(b != null);

            if (checkIdEquality && !ElementHelper.HaveEqualIds(a, b))
                return false;

            return ElementHelper.HaveEqualProperties(a, b) && HaveEqualEdges(a, b, checkIdEquality);
        }

        /// <summary>
        ///     Test whether the two vertices have equal edge sets
        /// </summary>
        /// <param name="a">the first vertex</param>
        /// <param name="b">the second vertex</param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids</param>
        /// <returns>whether the two vertices have the same edge sets</returns>
        public static bool HaveEqualEdges(IVertex a, IVertex b, bool checkIdEquality)
        {
            Contract.Requires(a != null);
            Contract.Requires(b != null);

            var aEdgeSet = new HashSet<IEdge>(a.GetEdges(Direction.Out));
            var bEdgeSet = new HashSet<IEdge>(b.GetEdges(Direction.Out));

            if (!HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality))
                return false;

            aEdgeSet.Clear();
            bEdgeSet.Clear();

            foreach (var edge in a.GetEdges(Direction.In))
                aEdgeSet.Add(edge);

            foreach (var edge in b.GetEdges(Direction.In))
                bEdgeSet.Add(edge);

            return HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality);
        }

        private static bool HasEqualEdgeSets(ICollection<IEdge> aEdgeSet, ICollection<IEdge> bEdgeSet,
                                             bool checkIdEquality)
        {
            Contract.Requires(aEdgeSet != null);
            Contract.Requires(bEdgeSet != null);

            if (aEdgeSet.Count != bEdgeSet.Count)
                return false;

            foreach (var aEdge in aEdgeSet)
            {
                IEdge tempEdge = null;
                foreach (var bEdge in bEdgeSet)
                {
                    if (bEdge.Label == aEdge.Label)
                    {
                        if (checkIdEquality)
                        {
                            if (ElementHelper.HaveEqualIds(aEdge, bEdge) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.In), bEdge.GetVertex(Direction.In)) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.Out),
                                                           bEdge.GetVertex(Direction.Out)) &&
                                ElementHelper.HaveEqualProperties(aEdge, bEdge))
                            {
                                tempEdge = bEdge;
                                break;
                            }
                        }
                        else if (ElementHelper.HaveEqualProperties(aEdge, bEdge))
                        {
                            tempEdge = bEdge;
                            break;
                        }
                    }
                }
                if (tempEdge == null)
                    return false;
                bEdgeSet.Remove(tempEdge);
            }
            return bEdgeSet.Count == 0;
        }
    }
}