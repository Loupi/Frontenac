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
        readonly IEnumerable<Index> _iterable;
        readonly EventGraph _eventGraph;

        public EventIndexIterable(IEnumerable<Index> iterable, EventGraph eventGraph)
        {
            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _iterable)
                yield return new EventIndex(index, _eventGraph);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}
