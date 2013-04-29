﻿using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyIndex : IIndex
    {
        protected IIndex RawIndex;

        public ReadOnlyIndex(IIndex rawIndex)
        {
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

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            if (typeof(IVertex).IsAssignableFrom(GetIndexClass()))
                return new ReadOnlyVertexIterable((IEnumerable<IVertex>)RawIndex.Get(key, value));
            return new ReadOnlyEdgeIterable((IEnumerable<IEdge>)RawIndex.Get(key, value));
        }

        public ICloseableIterable<IElement> Query(string key, object value)
        {
            if (typeof(IVertex).IsAssignableFrom(GetIndexClass()))
                return new ReadOnlyVertexIterable((IEnumerable<IVertex>)RawIndex.Query(key, value));
            return new ReadOnlyEdgeIterable((IEnumerable<IEdge>)RawIndex.Query(key, value));
        }

        public long Count(string key, object value)
        {
            return RawIndex.Count(key, value);
        }

        public string GetIndexName()
        {
            return RawIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return RawIndex.GetIndexClass();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
