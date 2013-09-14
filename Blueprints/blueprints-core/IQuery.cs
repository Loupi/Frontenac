using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints
{
    [ContractClass(typeof (QueryContract))]
    public interface IQuery
    {
        /// <summary>
        ///     Filter out the edge if it does not have a property with the specified value.
        /// </summary>
        /// <param name="key">the key of the property</param>
        /// <param name="value">the value to check against</param>
        /// <returns>the modified query object</returns>
        IQuery Has(string key, object value);

        /// <summary>
        ///     Filter out the edge if it does not have a property with a comparable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">the key of the property</param>
        /// ///
        /// <param name="compare">the comparator to use for comparison</param>
        /// <param name="value">the value to check against</param>
        /// <returns>the modified query object</returns>
        IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>;

        /// <summary>
        ///     Filter out the edge of its property value is not within the provided interval.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">the key of the property</param>
        /// <param name="startValue">the inclusive start value of the interval</param>
        /// <param name="endValue">the exclusive end value of the interval</param>
        /// <returns>the modified query object</returns>
        IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;

        /// <summary>
        ///     Execute the query and return the matching edges.
        /// </summary>
        /// <returns>the unfiltered edges</returns>
        IEnumerable<IEdge> Edges();

        /// <summary>
        ///     Execute the query and return the vertices on the other end of the matching edges.
        /// </summary>
        /// <returns>the unfiltered edge's vertices</returns>
        IEnumerable<IVertex> Vertices();

        /// <summary>
        ///     Filter out the edge if the max number of edges to retrieve has already been reached.
        /// </summary>
        /// <param name="max">the max number of edges to return</param>
        /// <returns>the modified query object</returns>
        IQuery Limit(long max);
    }

    public enum Compare
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual
    }

    public static class CompareHelpers
    {
        public static Compare Opposite(this Compare compare)
        {
            if (compare == Compare.Equal)
                return Compare.NotEqual;
            if (compare == Compare.NotEqual)
                return Compare.Equal;
            if (compare == Compare.GreaterThan)
                return Compare.LessThanEqual;
            if (compare == Compare.GreaterThanEqual)
                return Compare.LessThan;
            if (compare == Compare.LessThan)
                return Compare.GreaterThanEqual;
            return Compare.GreaterThan;
        }
    }
}