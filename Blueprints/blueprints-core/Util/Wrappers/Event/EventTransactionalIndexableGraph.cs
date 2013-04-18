using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    /// The transactional and indexable implementation of EventGraph where events are raised in batch in the order they
    /// changes occured to the graph, but only after a successful commit to the underlying graph.
    /// </summary>
    public class EventTransactionalIndexableGraph : EventIndexableGraph, TransactionalGraph, IndexableGraph, WrapperGraph
    {
        protected readonly TransactionalGraph transactionalGraph;

        public EventTransactionalIndexableGraph(IndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            transactionalGraph = baseIndexableGraph as TransactionalGraph;
            if (transactionalGraph == null)
                throw new ArgumentException("baseIndexableGraph must also implement TransactionalGraph");

            trigger = new EventTrigger(this, true);
        }

        /// <summary>
        /// A commit only fires the event queue on successful operation.  If the commit operation to the underlying
        /// graph fails, the event queue will not fire and the queue will not be reset.
        /// </summary>
        public void commit()
        {
            bool transactionFailure = false;
            try
            {
                transactionalGraph.commit();
            }
            catch (Exception)
            {
                transactionFailure = true;
                throw;
            }
            finally
            {
                if (!transactionFailure)
                {
                    trigger.fireEventQueue();
                    trigger.resetEventQueue();
                }
            }
        }

        /// <summary>
        /// A rollback only resets the event queue on successful operation.  If the rollback operation to the underlying
        /// graph fails, the event queue will not be reset.
        /// </summary>
        public void rollback()
        {
            bool transactionFailure = false;
            try
            {
                transactionalGraph.rollback();
            }
            catch (Exception re)
            {
                transactionFailure = true;
                throw re;
            }
            finally
            {
                if (!transactionFailure)
                {
                    trigger.resetEventQueue();
                }
            }
        }
    }
}
