using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// A KeyIndexableGraph is a graph that supports basic index functionality around the key/value pairs of the elements of the graph.
    /// By creating key indices for a particular property key, that key is indexed on all the elements of the graph.
    /// This has ramifications for quick lookups on methods like GetVertices(string, object) and GetEdges(string, object).
    /// </summary>
    public interface KeyIndexableGraph : Graph
    {
        /// <summary>
        /// Remove an automatic indexing structure associated with indexing provided key for element class.
        /// </summary>
        /// <typeparam name="T">the element class specification</typeparam>
        /// <param name="key">the key to drop the index for</param>
        /// <param name="elementClass">the element class that the index is for</param>
        void DropKeyIndex(string key, Type elementClass);

        /// <summary>
        /// Create an automatic indexing structure for indexing provided key for element class.
        /// </summary>
        /// <typeparam name="T">the element class specification</typeparam>
        /// <param name="key">the key to create the index for</param>
        /// <param name="elementClass">the element class that the index is for</param>
        /// <param name="indexParameters">a collection of parameters for the underlying index implementation</param>
        void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters);

        /// <summary>
        /// Return all the index keys associated with a particular element class.
        /// </summary>
        /// <typeparam name="T">the element class specification</typeparam>
        /// <param name="elementClass">the element class that the index is for</param>
        /// <returns>keys as a Set</returns>
        IEnumerable<string> GetIndexedKeys(Type elementClass);
    }
}
