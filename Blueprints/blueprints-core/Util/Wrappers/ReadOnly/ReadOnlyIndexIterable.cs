using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    class ReadOnlyIndexIterable : IEnumerable<IIndex>
    {
        readonly IEnumerable<IIndex> _iterable;

        public ReadOnlyIndexIterable(IEnumerable<IIndex> iterable)
        {
            _iterable = iterable;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new ReadOnlyIndex(index)).Cast<IIndex>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}
