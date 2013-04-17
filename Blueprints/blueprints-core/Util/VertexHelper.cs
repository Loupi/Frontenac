using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static bool HaveEqualNeighborhood(Vertex a, Vertex b, bool checkIdEquality)
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
        public static bool HaveEqualEdges(Vertex a, Vertex b, bool checkIdEquality)
        {
            HashSet<Edge> aEdgeSet = new HashSet<Edge>(a.GetEdges(Direction.OUT));
            HashSet<Edge> bEdgeSet = new HashSet<Edge>(b.GetEdges(Direction.OUT));

            if (!HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality))
                return false;

            aEdgeSet.Clear();
            bEdgeSet.Clear();

            foreach (Edge edge in a.GetEdges(Direction.IN))
                aEdgeSet.Add(edge);

            foreach (Edge edge in b.GetEdges(Direction.IN))
                bEdgeSet.Add(edge);

            return HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality);
        }

        static bool HasEqualEdgeSets(HashSet<Edge> aEdgeSet, HashSet<Edge> bEdgeSet, bool checkIdEquality)
        {
            if (aEdgeSet.Count != bEdgeSet.Count)
                return false;

            foreach (Edge aEdge in aEdgeSet)
            {
                Edge tempEdge = null;
                foreach (Edge bEdge in bEdgeSet)
                {
                    if (bEdge.GetLabel() == aEdge.GetLabel())
                    {
                        if (checkIdEquality)
                        {
                            if (ElementHelper.HaveEqualIds(aEdge, bEdge) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.IN), bEdge.GetVertex(Direction.IN)) &&
                                ElementHelper.HaveEqualIds(aEdge.GetVertex(Direction.OUT), bEdge.GetVertex(Direction.OUT)) &&
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
                else
                    bEdgeSet.Remove(tempEdge);
            }
            return bEdgeSet.Count == 0;
        }
    }
}
