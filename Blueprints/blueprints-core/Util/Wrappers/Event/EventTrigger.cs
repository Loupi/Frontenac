using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    public class EventTrigger
    {
        /// <summary>
        /// A queue of events that are triggered by change to the graph.  The queue builds
        /// up until the EventTrigger fires them in the order they were received.
        /// </summary>
        readonly ThreadLocal<ConcurrentQueue<Listener.IEvent>> _eventQueue =
            new ThreadLocal<ConcurrentQueue<Listener.IEvent>>(() => new ConcurrentQueue<Listener.IEvent>());

        /// <summary>
        /// When set to true, events in the event queue will only be fired when a transaction
        /// is committed.
        /// </summary>
        readonly bool _enqueEvents;

        readonly EventGraph _graph;

        public EventTrigger(EventGraph graph, bool enqueEvents)
        {
            Contract.Requires(graph != null);

            _enqueEvents = enqueEvents;
            _graph = graph;
        }

        /// <summary>
        /// Add an event to the event queue.
        /// If the enqueEvents is false, then the queue fires and resets after each event
        /// </summary>
        /// <param name="evt">The event to add to the event queue</param>
        public void AddEvent(Listener.IEvent evt)
        {
            Contract.Requires(evt != null);

            _eventQueue.Value.Enqueue(evt);

            if (!_enqueEvents)
            {
                FireEventQueue();
                ResetEventQueue();
            }
        }

        public void ResetEventQueue()
        {
            _eventQueue.Value = new ConcurrentQueue<Listener.IEvent>();
        }

        public void FireEventQueue()
        {
            var concurrentQueue = _eventQueue.Value;
            Listener.IEvent event_;
            while (concurrentQueue.TryDequeue(out event_))
            {
                event_.FireEvent(_graph.GetListenerIterator());
            }
        }
    }
}
