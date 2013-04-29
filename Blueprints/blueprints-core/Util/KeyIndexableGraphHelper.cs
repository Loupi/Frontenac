using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    public static class KeyIndexableGraphHelper
    {
        /// <summary>
        /// For those graphs that do no support automatic reindexing of elements when a key is provided for indexing, this method can be used to simulate that behavior.
        /// The elements in the graph are iterated and their properties (for the provided keys) are removed and then added.
        /// Be sure that the key indices have been created prior to calling this method so that they can pick up the property mutations calls.
        /// Finally, if the graph is a TransactionalGraph, then a 1000 mutation buffer is used for each commit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph">the graph containing the provided elements</param>
        /// <param name="elements">the elements to index into the key indices</param>
        /// <param name="keys">the keys of the key indices</param>
        /// <returns>the number of element properties that were indexed</returns>
        public static long ReIndexElements<T>(IGraph graph, IEnumerable<T> elements, IEnumerable<string> keys) where T : IElement
        {
            bool isTransactional = graph is ITransactionalGraph;
            long counter = 0;
            foreach (T element in elements)
            {
                foreach (string key in keys)
                {
                    object value = element.RemoveProperty(key);
                    if (null != value)
                    {
                        counter++;
                        element.SetProperty(key, value);

                        if (isTransactional && (counter % 1000 == 0))
                            ((ITransactionalGraph)graph).Commit();
                    }
                }
            }
            return counter;
        }
    }
}
