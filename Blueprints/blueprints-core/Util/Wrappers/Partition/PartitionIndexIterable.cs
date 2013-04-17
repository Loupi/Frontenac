﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionIndexIterable : IEnumerable<Index>
    {
        readonly IEnumerable<Index> _Iterable;
        readonly PartitionGraph _Graph;

        public PartitionIndexIterable(IEnumerable<Index> iterable, PartitionGraph graph)
        {
            _Iterable = iterable;
            _Graph = graph;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _Iterable)
                yield return new PartitionIndex(index, _Graph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}