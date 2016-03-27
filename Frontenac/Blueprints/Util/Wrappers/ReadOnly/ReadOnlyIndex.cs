using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndex : IIndex
    {
        private readonly ReadOnlyGraph _graph;
        protected IIndex RawIndex;

        public ReadOnlyIndex(ReadOnlyGraph graph, IIndex rawIndex)
        {
            Contract.Requires(graph != null);
            Contract.Requires(rawIndex != null);

            _graph = graph;
            RawIndex = rawIndex;
        }

        public void Remove(string key, object value, IElement element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public void Put(string key, object value, IElement element)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new ReadOnlyVertexIterable(_graph, (IEnumerable<IVertex>)RawIndex.Get(key, value));
            return new ReadOnlyEdgeIterable(_graph, (IEnumerable<IEdge>) RawIndex.Get(key, value));
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            if (typeof (IVertex).IsAssignableFrom(Type))
                return new ReadOnlyVertexIterable(_graph, (IEnumerable<IVertex>) RawIndex.Query(key, value));
            return new ReadOnlyEdgeIterable(_graph, (IEnumerable<IEdge>) RawIndex.Query(key, value));
        }

        public long Count(string key, object value)
        {
            return RawIndex.Count(key, value);
        }

        public string Name => RawIndex.Name;

        public Type Type => RawIndex.Type;

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}