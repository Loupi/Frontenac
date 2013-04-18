using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    public class EventTransactionalGraph : EventGraph, TransactionalGraph, WrapperGraph
    {
        protected readonly TransactionalGraph _TransactionalGraph;

        public EventTransactionalGraph(TransactionalGraph baseGraph)
            : base(baseGraph)
        {
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
                _TransactionalGraph.commit();
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
                _TransactionalGraph.rollback();
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
                    trigger.resetEventQueue();
                }
            }
        }
    }
}
