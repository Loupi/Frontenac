using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Frontenac.Grave.Indexing.Lucene
{
    // http://www.lybecker.com/blog/2009/12/03/lucene-net-and-transactions/

    public class TransactionalIndexWriter : IndexWriter, IEnlistmentNotification
    {
        #region ctor
        public TransactionalIndexWriter(Directory d, Analyzer a, bool create, MaxFieldLength mfl)
            : base(d, a, create, mfl)
        {
            EnlistTransaction();
        }
        /* More constructors */
        #endregion

        public void EnlistTransaction()
        {
            // Enlist in transaction if ambient transaction exists
            var tx = Transaction.Current;
            if (tx != null)
                tx.EnlistVolatile(this, EnlistmentOptions.None);
        }

        #region IEnlistmentNotification Members
        public void Commit(Enlistment enlistment)
        {
            Commit();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            // Do nothing.
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            PrepareCommit();
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            base.Rollback();
            enlistment.Done();
        }
        #endregion
    }
}
