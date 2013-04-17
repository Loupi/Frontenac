using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// An IndexableGraph is a graph that supports the manual indexing of its elements.
    /// An index is typically some sort of tree structure that allows for the fast lookup of elements by key/value pairs.
    /// Indices have an Index object associated with them and allow the user to specify the putting and getting of elements into the index.
    /// </summary>
    public interface IndexableGraph : Graph
    {
        /// <summary>
        /// Generate an index with a particular name for a particular class.
        /// </summary>
        /// <typeparam name="T">the element class that this index is indexing (can be base class)</typeparam>
        /// <param name="indexName">the name of the manual index</param>
        /// <param name="indexClass">the element class that this index is indexing (can be base class)</param>
        /// <param name="indexParameters">a collection of parameters for the underlying index implementation</param>
        /// <returns>the index created</returns>
        Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters);

        /// <summary>
        /// Get an index from the graph by its name and index class. An index is unique up to name.
        /// </summary>
        /// <typeparam name="T">the class of the elements being indexed (can be base class)</typeparam>
        /// <param name="indexName">the name of the index to retrieve</param>
        /// <param name="indexClass">the class of the elements being indexed (can be base class)</param>
        /// <returns>the retrieved index</returns>
        Index GetIndex(string indexName, Type indexClass);

        /// <summary>
        /// Get all the indices maintained by the graph.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>the indices associated with the graph</returns>
        IEnumerable<Index> GetIndices();

        /// <summary>
        /// Remove an index associated with the graph.
        /// </summary>
        /// <param name="indexName">the name of the index to drop</param>
        void DropIndex(string indexName);
    }
}
