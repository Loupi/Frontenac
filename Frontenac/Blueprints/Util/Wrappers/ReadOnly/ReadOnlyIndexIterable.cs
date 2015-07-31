﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    internal class ReadOnlyIndexIterable : IEnumerable<IIndex>
    {
        private readonly ReadOnlyGraph _graph;
        private readonly IEnumerable<IIndex> _iterable;

        public ReadOnlyIndexIterable(ReadOnlyGraph graph, IEnumerable<IIndex> iterable)
        {
            Contract.Requires(iterable != null);

            _graph = graph;
            _iterable = iterable;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new ReadOnlyIndex(_graph, index)).Cast<IIndex>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}