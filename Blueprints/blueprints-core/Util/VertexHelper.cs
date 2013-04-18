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
        public static bool haveEqualNeighborhood(Vertex a, Vertex b, bool checkIdEquality)
        {
            if (checkIdEquality && !ElementHelper.haveEqualIds(a, b))
                return false;

            return ElementHelper.haveEqualProperties(a, b) && haveEqualEdges(a, b, checkIdEquality);
        }

        /// <summary>
        /// Test whether the two vertices have equal edge sets
        /// </summary>
        /// <param name="a">the first vertex</param>
        /// <param name="b">the second vertex</param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids</param>
        /// <returns>whether the two vertices have the same edge sets</returns>
        public static bool haveEqualEdges(Vertex a, Vertex b, bool checkIdEquality)
        {
            HashSet<Edge> aEdgeSet = new HashSet<Edge>(a.getEdges(Direction.OUT));
            HashSet<Edge> bEdgeSet = new HashSet<Edge>(b.getEdges(Direction.OUT));

            if (!hasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality))
                return false;

            aEdgeSet.Clear();
            bEdgeSet.Clear();

            foreach (Edge edge in a.getEdges(Direction.IN))
                aEdgeSet.Add(edge);

            foreach (Edge edge in b.getEdges(Direction.IN))
                bEdgeSet.Add(edge);

            return hasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality);
        }

        static bool hasEqualEdgeSets(HashSet<Edge> aEdgeSet, HashSet<Edge> bEdgeSet, bool checkIdEquality)
        {
            if (aEdgeSet.Count != bEdgeSet.Count)
                return false;

            foreach (Edge aEdge in aEdgeSet)
            {
                Edge tempEdge = null;
                foreach (Edge bEdge in bEdgeSet)
                {
                    if (bEdge.getLabel() == aEdge.getLabel())
                    {
                        if (checkIdEquality)
                        {
                            if (ElementHelper.haveEqualIds(aEdge, bEdge) &&
                                ElementHelper.haveEqualIds(aEdge.getVertex(Direction.IN), bEdge.getVertex(Direction.IN)) &&
                                ElementHelper.haveEqualIds(aEdge.getVertex(Direction.OUT), bEdge.getVertex(Direction.OUT)) &&
                                ElementHelper.haveEqualProperties(aEdge, bEdge))
                            {
                                tempEdge = bEdge;
                                break;
                            }
                        }
                        else if (ElementHelper.haveEqualProperties(aEdge, bEdge))
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
