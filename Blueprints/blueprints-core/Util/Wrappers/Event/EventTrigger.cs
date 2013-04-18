using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    public class EventTrigger
    {
        /// <summary>
        /// A queue of events that are triggered by change to the graph.  The queue builds
        /// up until the EventTrigger fires them in the order they were received.
        /// </summary>
        readonly ThreadLocal<ConcurrentQueue<Listener.Event>> _eventQueue =
            new ThreadLocal<ConcurrentQueue<Listener.Event>>(() => new ConcurrentQueue<Listener.Event>());

        /// <summary>
        /// When set to true, events in the event queue will only be fired when a transaction
        /// is committed.
        /// </summary>
        readonly bool _enqueEvents;

        readonly EventGraph _graph;

        public EventTrigger(EventGraph graph, bool enqueEvents)
        {
            _enqueEvents = enqueEvents;
            _graph = graph;
        }

        /// <summary>
        /// Add an event to the event queue.
        /// If the enqueEvents is false, then the queue fires and resets after each event
        /// </summary>
        /// <param name="evt">The event to add to the event queue</param>
        public void addEvent(Listener.Event evt)
        {
            _eventQueue.Value.Enqueue(evt);

            if (!_enqueEvents)
            {
                this.fireEventQueue();
                this.resetEventQueue();
            }
        }

        public void resetEventQueue()
        {
            _eventQueue.Value = new ConcurrentQueue<Listener.Event>();
        }

        public void fireEventQueue()
        {
            ConcurrentQueue<Listener.Event> concurrentQueue = _eventQueue.Value;
            Listener.Event event_;
            while (concurrentQueue.TryDequeue(out event_))
            {
                event_.fireEvent(_graph.getListenerIterator());
            }
        }
    }
}
