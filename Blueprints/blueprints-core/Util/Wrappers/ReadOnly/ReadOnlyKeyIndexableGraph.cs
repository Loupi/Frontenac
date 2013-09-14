using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    /// <summary>
    ///     A ReadOnlyKeyIndexableGraph wraps a KeyIndexableGraph and overrides the underlying graph's mutating methods.
    ///     In this way, a ReadOnlyKeyIndexableGraph can only be read from, not written to.
    /// </summary>
    public class ReadOnlyKeyIndexableGraph : ReadOnlyIndexableGraph, IKeyIndexableGraph
    {
        public ReadOnlyKeyIndexableGraph(IKeyIndexableGraph baseKiGraph)
            : base((IIndexableGraph) baseKiGraph)
        {
        }

        public void DropKeyIndex(string key, Type elementClass)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            return ((IKeyIndexableGraph) BaseGraph).GetIndexedKeys(elementClass);
        }
    }
}