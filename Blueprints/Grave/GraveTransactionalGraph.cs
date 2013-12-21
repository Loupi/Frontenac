using System;
using System.Diagnostics.Contracts;
using System.Transactions;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Indexing;
using Microsoft.Isam.Esent.Interop;
using Transaction = Microsoft.Isam.Esent.Interop.Transaction;

namespace Frontenac.Grave
{
    public class GraveTransactionalGraph : GraveGraph, ITransactionalGraph, IEnlistmentNotification, IDisposable
    {
        private Transaction _transaction;
        private TransactionScope _transactionScope;

        public GraveTransactionalGraph(IGraveGraphFactory factory, 
                                       EsentContext context, 
                                       IndexingService indexingService)
            : base(factory, indexingService, context)
        {
            Contract.Requires(factory != null);
            Contract.Requires(context != null);
            Contract.Requires(indexingService != null);
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GraveTransactionalGraph()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _transaction != null)
            {
                Rollback();
                _transaction.Dispose();
            }

            _disposed = true;
        }

        #endregion

        public override void Shutdown()
        {
            if (_transaction != null)
            {
                Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Commit()
        {
            if (_transactionScope != null)
            {
                _transactionScope.Complete();
                _transactionScope.Dispose();
                _transactionScope = null;
            }
        }

        public void Rollback()
        {
            if (_transactionScope != null)
            {
                _transactionScope.Dispose();
                _transactionScope = null;
            }
        }

        public override IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            BeginTransaction();
            return base.AddEdge(unused, outVertex, inVertex, label);
        }

        public override IVertex AddVertex(object unused)
        {
            BeginTransaction();
            return base.AddVertex(unused);
        }

        public override void RemoveEdge(IEdge edge)
        {
            BeginTransaction();
            base.RemoveEdge(edge);
        }

        public override void RemoveVertex(IVertex vertex)
        {
            BeginTransaction();
            base.RemoveVertex(vertex);
        }

        private void BeginTransaction()
        {
            if (_transactionScope != null) return;

            _transactionScope = System.Transactions.Transaction.Current != null 
                ? new TransactionScope(System.Transactions.Transaction.Current) 
                : new TransactionScope();

            if (System.Transactions.Transaction.Current != null)
            {
                System.Transactions.Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
            }

            if (_transaction == null)
                _transaction = new Transaction(Context.Session);
            else if (!_transaction.IsInTransaction)
                _transaction.Begin();
        }

        public override IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            BeginTransaction();
            return base.CreateIndex(indexName, indexClass, indexParameters);
        }

        public override void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            BeginTransaction();
            base.CreateKeyIndex(key, elementClass, indexParameters);
        }

        public override void DropIndex(string indexName)
        {
            BeginTransaction();
            base.DropIndex(indexName);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            IndexingService.Prepare();
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            if (_transaction != null && _transaction.IsInTransaction)
                _transaction.Commit(CommitTransactionGrbit.None);

            IndexingService.Commit();

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            if (_transaction != null && _transaction.IsInTransaction)
                _transaction.Rollback();

            IndexingService.Rollback();

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}