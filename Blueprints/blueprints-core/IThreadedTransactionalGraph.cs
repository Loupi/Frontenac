using System.Diagnostics.Contracts;
using Frontenac.Blueprints.Contracts;

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
    [ContractClass(typeof(ThreadedTransactionalGraphContract))]
    public interface IThreadedTransactionalGraph : ITransactionalGraph
    {
        /// <summary>
        /// Returns a TransactionalGraph that represents a transactional context independent of the executing transaction.
        /// </summary>
        /// <returns>A transactional context. Invoking TransactionalGraph.shutdown() successfully commits the transaction.</returns>
        ITransactionalGraph NewTransaction();
    }
}
