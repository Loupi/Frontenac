using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    /// <summary>
    /// A ReadOnlyKeyIndexableGraph wraps a KeyIndexableGraph and overrides the underlying graph's mutating methods.
    /// In this way, a ReadOnlyKeyIndexableGraph can only be read from, not written to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlyKeyIndexableGraph : ReadOnlyIndexableGraph, KeyIndexableGraph
    {
        public ReadOnlyKeyIndexableGraph(KeyIndexableGraph baseKIGraph)
            : base((IndexableGraph)baseKIGraph)
        {
        }

        public void dropKeyIndex(string key, Type elementClass)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public void createKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public IEnumerable<string> getIndexedKeys(Type elementClass)
        {
            return ((KeyIndexableGraph)baseGraph).getIndexedKeys(elementClass);
        }
    }
}
