using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionIndexIterable : IEnumerable<IIndex>
    {
        readonly IEnumerable<IIndex> _iterable;
        readonly PartitionGraph _graph;

        public PartitionIndexIterable(IEnumerable<IIndex> iterable, PartitionGraph graph)
        {
            _iterable = iterable;
            _graph = graph;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new PartitionIndex(index, _graph)).Cast<IIndex>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}
