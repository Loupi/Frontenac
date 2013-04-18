using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public interface Index
    {
        /// <summary>
        /// Get the name of the index.
        /// </summary>
        /// <returns>the name of the index</returns>
        string getIndexName();

        /// <summary>
        /// Get the class that this index is indexing.
        /// </summary>
        /// <returns>the class this index is indexing</returns>
        Type getIndexClass();

        /// <summary>
        /// Index an element by a key and a value.
        /// </summary>
        /// <param name="key">the key to index the element by</param>
        /// <param name="value">the value to index the element by</param>
        /// <param name="element">the element to index</param>
        void put(string key, object value, Element element);

        /// <summary>
        /// Get all elements that are indexed by the provided key/value.
        /// </summary>
        /// <param name="key">the key of the indexed elements</param>
        /// <param name="value">the value of the indexed elements</param>
        /// <returns>an IEnumerable of elements that have a particular key/value in the index</returns>
        CloseableIterable<Element> get(string key, object value);

        /// <summary>
        /// Get all the elements that are indexed by the provided key and specified query object.
        /// This is useful for graph implementations that support complex query capabilities.
        /// If querying is not supported, simply throw a NotSupportedException.
        /// </summary>
        /// <param name="key">the key of the indexed elements</param>
        /// <param name="query">the query object for the indexed elements' keys</param>
        /// <returns>an IEnumerable of elements that have a particular key/value in the index that match the query object</returns>
        CloseableIterable<Element> query(string key, object query);

        /// <summary>
        /// Get a count of elements with a particular key/value pair.
        /// The semantics are the same as the get method.
        /// </summary>
        /// <param name="key">denoting the sub-index to search</param>
        /// <param name="value">the value to search for</param>
        /// <returns>the collection of elements that meet that criteria</returns>
        long count(string key, object value);

        /// <summary>
        /// Remove an element indexed by a particular key/value.
        /// </summary>
        /// <param name="key">the key of the indexed element</param>
        /// <param name="value">the value of the indexed element</param>
        /// <param name="element">the element to remove given the key/value pair</param>
        void remove(string key, object value, Element element);
    }
}
