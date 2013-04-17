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
            _Trigger = new EventTrigger(this, true);
        }

        /// <summary>
        /// A commit only fires the event queue on successful operation.  If the commit operation to the underlying
        /// graph fails, the event queue will not fire and the queue will not be reset.
        /// </summary>
        public void Commit()
        {
            bool transactionFailure = false;
            try
            {
                _TransactionalGraph.Commit();
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
                    _Trigger.FireEventQueue();
                    _Trigger.ResetEventQueue();
                }
            }
        }

        /// <summary>
        /// A rollback only resets the event queue on successful operation.  If the rollback operation to the underlying
        /// graph fails, the event queue will not be reset.
        /// </summary>
        public void Rollback()
        {
            bool transactionFailure = false;
            try
            {
                _TransactionalGraph.Rollback();
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
                    _Trigger.ResetEventQueue();
                }
            }
        }
    }
}
