using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionIndexIterable : IEnumerable<IIndex>
    {
        readonly IEnumerable<IIndex> _iterable;
        readonly PartitionGraph _graph;

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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}
