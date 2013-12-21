using System;
using System.Collections.Generic;
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
        TransactionScope _transactionScope;

        public GraveTransactionalGraph(IGraveGraphFactory factory, EsentContext context, IndexingService indexingService)
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
            if (_transaction != null && _transaction.IsInTransaction)
                _transaction.Commit(CommitTransactionGrbit.None);
        }

        public void Rollback()
        {
            if (_transaction != null && _transaction.IsInTransaction)
                _transaction.Rollback();
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

        public override IEdge GetEdge(object id)
        {
            BeginTransaction();
            return base.GetEdge(id);
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            BeginTransaction();
            return base.GetEdges();
        }

        public override IEnumerable<IEdge> GetEdges(string key, object value)
        {
            BeginTransaction();
            return base.GetEdges(key, value);
        }

        public override IVertex GetVertex(object id)
        {
            BeginTransaction();
            return base.GetVertex(id);
        }

        public override IEnumerable<IVertex> GetVertices()
        {
            BeginTransaction();
            return base.GetVertices();
        }

        public override IEnumerable<IVertex> GetVertices(string key, object value)
        {
            BeginTransaction();
            return base.GetVertices(key, value);
        }

        public override IQuery Query()
        {
            BeginTransaction();
            return base.Query();
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

        public override IIndex GetIndex(string indexName, Type indexClass)
        {
            BeginTransaction();
            return base.GetIndex(indexName, indexClass);
        }

        public override IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            BeginTransaction();
            return base.GetIndexedKeys(elementClass);
        }

        public override IEnumerable<IIndex> GetIndices()
        {
            BeginTransaction();
            return base.GetIndices();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            Commit();
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            Rollback();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}