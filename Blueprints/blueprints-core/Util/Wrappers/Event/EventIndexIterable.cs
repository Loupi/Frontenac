using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// A sequence of indices that applies the list of listeners into each element.
    /// </summary>
    class EventIndexIterable : IEnumerable<IIndex>
    {
        readonly IEnumerable<IIndex> _iterable;
        readonly EventGraph _eventGraph;

        public EventIndexIterable(IEnumerable<IIndex> iterable, EventGraph eventGraph)
        {
            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new EventIndex(index, _eventGraph)).Cast<IIndex>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}
