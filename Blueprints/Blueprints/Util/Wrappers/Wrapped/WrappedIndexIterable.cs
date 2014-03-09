using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    internal class WrappedIndexIterable : IEnumerable<IIndex>
    {
        private readonly IEnumerable<IIndex> _iterable;

        public WrappedIndexIterable(IEnumerable<IIndex> iterable)
        {
            Contract.Requires(iterable != null);

            _iterable = iterable;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new WrappedIndex(index)).Cast<IIndex>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}