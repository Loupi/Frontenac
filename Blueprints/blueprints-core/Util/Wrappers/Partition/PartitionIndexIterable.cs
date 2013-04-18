using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionIndexIterable : IEnumerable<Index>
    {
        readonly IEnumerable<Index> _iterable;
        readonly PartitionGraph _graph;

        public PartitionIndexIterable(IEnumerable<Index> iterable, PartitionGraph graph)
        {
            _iterable = iterable;
            _graph = graph;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _iterable)
                yield return new PartitionIndex(index, _graph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}
