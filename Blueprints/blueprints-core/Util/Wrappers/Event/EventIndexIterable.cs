using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// A sequence of indices that applies the list of listeners into each element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class EventIndexIterable : IEnumerable<Index>
    {
        readonly IEnumerable<Index> _Iterable;
        readonly EventGraph _EventGraph;

        public EventIndexIterable(IEnumerable<Index> iterable, EventGraph eventGraph)
        {
            _Iterable = iterable;
            _EventGraph = eventGraph;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _Iterable)
                yield return new EventIndex(index, _EventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}
