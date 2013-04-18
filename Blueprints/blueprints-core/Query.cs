using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public interface Query
    {
        /// <summary>
        /// Filter out the edge if it does not have a property with the specified value.
        /// </summary>
        /// <param name="key">the key of the property</param>
        /// <param name="value">the value to check against</param>
        /// <returns>the modified query object</returns>
        Query has(string key, object value);

        /// <summary>
        /// Filter out the edge if it does not have a property with a comparable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">the key of the property</param>
        /// /// <param name="compare">the comparator to use for comparison</param>
        /// <param name="value">the value to check against</param>
        /// <returns>the modified query object</returns>
        Query has<T>(string key, Compare compare, T value) where T : IComparable<T>;

        /// <summary>
        /// Filter out the edge of its property value is not within the provided interval.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">the key of the property</param>
        /// <param name="startValue">the inclusive start value of the interval</param>
        /// <param name="endValue">the exclusive end value of the interval</param>
        /// <returns>the modified query object</returns>
        Query interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;

        /// <summary>
        /// Execute the query and return the matching edges.
        /// </summary>
        /// <returns>the unfiltered edges</returns>
        IEnumerable<Edge> edges();

        /// <summary>
        /// Execute the query and return the vertices on the other end of the matching edges.
        /// </summary>
        /// <returns>the unfiltered edge's vertices</returns>
        IEnumerable<Vertex> vertices();

        /// <summary>
        /// Filter out the edge if the max number of edges to retrieve has already been reached.
        /// </summary>
        /// <param name="max">the max number of edges to return</param>
        /// <returns>the modified query object</returns>
        Query limit(long max);
    }

    public enum Compare
    {
        EQUAL,
        NOT_EQUAL,
        GREATER_THAN,
        GREATER_THAN_EQUAL,
        LESS_THAN,
        LESS_THAN_EQUAL
    }

    public static class CompareHelpers
    {
        public static Compare opposite(this Compare compare)
        {
            if (compare == Compare.EQUAL)
                return Compare.NOT_EQUAL;
            else if (compare == Compare.NOT_EQUAL)
                return Compare.EQUAL;
            else if (compare == Compare.GREATER_THAN)
                return Compare.LESS_THAN_EQUAL;
            else if (compare == Compare.GREATER_THAN_EQUAL)
                return Compare.LESS_THAN;
            else if (compare == Compare.LESS_THAN)
                return Compare.GREATER_THAN_EQUAL;
            else
                return Compare.GREATER_THAN;
        }
    }
}
