using System;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    //Based on RavenDB EsentTransactionContext
    public class EsentTransaction : IDisposable
    {
        private readonly IntPtr _context;
        private readonly Session _session;
        private readonly Transaction _transaction;
        private bool _alreadyInContext;

        public EsentTransaction(Session session, int transactionNumber)
        {
            _session = session;
            _context = new IntPtr(transactionNumber);
            using (EnterSessionContext())
            {
                _transaction = new Transaction(_session);
            }
        }

        public IDisposable EnterSessionContext()
        {
            if (_alreadyInContext)
                return new DisposableAction(() => { });

            Api.JetSetSessionContext(_session, _context);
            _alreadyInContext = true;

            if (_transaction != null && !_transaction.IsInTransaction)
                _transaction.Begin();

            return new DisposableAction(() =>
            {
                Api.JetResetSessionContext(_session);
                _alreadyInContext = false;
            });
        }

        public void Commit()
        {
            if (_transaction.IsInTransaction)
            {
                using (EnterSessionContext())
                {
                    _transaction.Commit(CommitTransactionGrbit.None);
                }
            }
        }

        public void Rollback()
        {
            if (_transaction.IsInTransaction)
            {
                using (EnterSessionContext())
                {
                    _transaction.Rollback();
                }
            }
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EsentTransaction()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                using (EnterSessionContext())
                {
                    _transaction.Dispose();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
