﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    internal class PartitionIndexIterable : IEnumerable<IIndex>
    {
        private readonly PartitionGraph _graph;
        private readonly IEnumerable<IIndex> _iterable;

        public PartitionIndexIterable(IEnumerable<IIndex> iterable, PartitionGraph graph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(graph != null);

            _iterable = iterable;
            _graph = graph;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new PartitionIndex(index, _graph)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}