using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    public static class VertexHelper
    {
        /// <summary>
        /// Test whether the two vertices have equal properties and edge sets. 
        /// </summary>
        /// <param name="a">the first vertex </param>
        /// <param name="b">the second vertex </param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids </param>
        /// <returns>whether the two vertices are semantically the same </returns>
        public static bool HaveEqualNeighborhood(IVertex a, IVertex b, bool checkIdEquality)
        {
            if (checkIdEquality && !ElementHelper.HaveEqualIds(a, b))
                return false;

            return ElementHelper.HaveEqualProperties(a, b) && HaveEqualEdges(a, b, checkIdEquality);
        }

        /// <summary>
        /// Test whether the two vertices have equal edge sets
        /// </summary>
        /// <param name="a">the first vertex</param>
        /// <param name="b">the second vertex</param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids</param>
        /// <returns>whether the two vertices have the same edge sets</returns>
        public static bool HaveEqualEdges(IVertex a, IVertex b, bool checkIdEquality)
        {
            var aEdgeSet = new HashSet<IEdge>(a.GetEdges(Direction.Out));
            var bEdgeSet = new HashSet<IEdge>(b.GetEdges(Direction.Out));

            if (!HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality))
                return false;

            aEdgeSet.Clear();
            bEdgeSet.Clear();

            foreach (IEdge edge in a.GetEdges(Direction.In))
                aEdgeSet.Add(edge);

            foreach (IEdge edge in b.GetEdges(Direction.In))
                bEdgeSet.Add(edge);

            return HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality);
        }

        static bool HasEqualEdgeSets(HashSet<IEdge> aEdgeSet, HashSet<IEdge> bEdgeSet, bool checkIdEquality)
        {
            if (aEdgeSet.Count != bEdgeSet.Count)
                return false;

            foreach (IEdge aEdge in aEdgeSet)
            {
                IEdge tempEdge = null;
                foreach (IEdge bEdge in bEdgeSet)
                {
                    if (bEdge.GetLabel() == aEdge.GetLabel())
                    {
                        if (checkIdEquality)
                        {
                            if (ElementHelper.HaveEqualIds(aEdge, bEdge) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.In), bEdge.GetVertex(Direction.In)) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.Out), bEdge.GetVertex(Direction.Out)) &&
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
