using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    class WrappedIndexIterable : IEnumerable<IIndex>
    {
        readonly IEnumerable<IIndex> _iterable;

        public WrappedIndexIterable(IEnumerable<IIndex> iterable)
        {
            _iterable = iterable;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new WrappedIndex(index)).Cast<IIndex>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}
