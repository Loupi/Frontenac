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
        readonly ThreadLocal<ConcurrentQueue<Listener.Event>> _EventQueue =
            new ThreadLocal<ConcurrentQueue<Listener.Event>>(() => new ConcurrentQueue<Listener.Event>());

        /// <summary>
        /// When set to true, events in the event queue will only be fired when a transaction
        /// is committed.
        /// </summary>
        readonly bool _EnqueEvents;

        readonly EventGraph _Graph;

        public EventTrigger(EventGraph graph, bool enqueEvents)
        {
            _EnqueEvents = enqueEvents;
            _Graph = graph;
        }

        /// <summary>
        /// Add an event to the event queue.
        /// If the enqueEvents is false, then the queue fires and resets after each event
        /// </summary>
        /// <param name="evt">The event to add to the event queue</param>
        public void AddEvent(Listener.Event evt)
        {
            _EventQueue.Value.Enqueue(evt);

            if (!_EnqueEvents)
            {
                this.FireEventQueue();
                this.ResetEventQueue();
            }
        }

        public void ResetEventQueue()
        {
            _EventQueue.Value = new ConcurrentQueue<Listener.Event>();
        }

        public void FireEventQueue()
        {
            ConcurrentQueue<Listener.Event> concurrentQueue = _EventQueue.Value;
            Listener.Event event_;
            while (concurrentQueue.TryDequeue(out event_))
            {
                event_.FireEvent(_Graph.GetListenerIterator());
            }
        }
    }
}
