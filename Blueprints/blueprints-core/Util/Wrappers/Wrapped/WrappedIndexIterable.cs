using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    class WrappedIndexIterable : IEnumerable<Index>
    {
        readonly IEnumerable<Index> _iterable;

        public WrappedIndexIterable(IEnumerable<Index> iterable)
        {
            _iterable = iterable;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _iterable)
                yield return new WrappedIndex(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}
