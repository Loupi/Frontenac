using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     A sequence of indices that applies the list of listeners into each element.
    /// </summary>
    internal class EventIndexIterable : IEnumerable<IIndex>
    {
        private readonly EventGraph _eventGraph;
        private readonly IEnumerable<IIndex> _iterable;

        public EventIndexIterable(IEnumerable<IIndex> iterable, EventGraph eventGraph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(eventGraph != null);

            _iterable = iterable;
            _eventGraph = eventGraph;
        }

        public IEnumerator<IIndex> GetEnumerator()
        {
            return _iterable.Select(index => new EventIndex(index, _eventGraph)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IIndex>).GetEnumerator();
        }
    }
}