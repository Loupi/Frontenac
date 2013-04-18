using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// ThreadedTransactionalGraph provides more fine grained control over the transactional context.
    /// While TransactionalGraph binds each transaction to the executing thread, ThreadedTransactionalGraph's newTransaction 
    /// returns a TransactionalGraph that represents its own transactional context independent of the executing thread.
    /// Hence, one can have multiple threads operating against a single transaction represented by the returned TransactionalGraph object. 
    /// This is useful for parallelizing graph algorithms.
    /// 
    /// Note, that one needs to call TransactionalGraph.Commit() or TransactionalGraph.Rollback() to close the transactions returned
    /// </summary>
    public interface ThreadedTransactionalGraph : TransactionalGraph
    {
        /// <summary>
        /// Returns a TransactionalGraph that represents a transactional context independent of the executing transaction.
        /// </summary>
        /// <returns>A transactional context. Invoking TransactionalGraph.shutdown() successfully commits the transaction.</returns>
        TransactionalGraph newTransaction();
    }
}
