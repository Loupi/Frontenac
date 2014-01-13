using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Transactions;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Indexing;

namespace Frontenac.Grave
{
    public class GraveTransactionalGraph : GraveGraph, ITransactionalGraph, IEnlistmentNotification, IDisposable
    {
        private static int _transactionNumber = 1;
        private EsentTransaction _transaction;
        private TransactionScope _transactionScope;
        private readonly Dictionary<string, GraveTransactionalIndex> _transactionalIndices
            = new Dictionary<string, GraveTransactionalIndex>();

        public GraveTransactionalGraph(IGraveGraphFactory factory, 
                                       EsentContext context, 
                                       IndexingService indexingService)
            : base(factory, indexingService)
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
            base.Shutdown();
        }

        public void Commit()
        {
            if (_transactionScope == null) return;
            _transactionScope.Complete();
            _transactionScope.Dispose();
            _transactionScope = null;
        }

        public void Rollback()
        {
            if (_transactionScope == null) return;
            _transactionScope.Dispose();
            _transactionScope = null;
        }

        public override IEdge AddEdge(object unused, IVertex outVertex, IVertex inVertex, string label)
        {
            using (EnterTransactionContext())
            {
                return base.AddEdge(unused, outVertex, inVertex, label);
            }
        }

        public override IEdge GetEdge(object id)
        {
            using (EnterTransactionContext())
            {
                return base.GetEdge(id);
            }
        }

        public override IEnumerable<IEdge> GetEdges()
        {
            using (EnterTransactionContext())
            {
                foreach (var edge in base.GetEdges())
                {
                    yield return edge;
                }
            }
        }

        public override IEnumerable<IEdge> GetEdges(string key, object value)
        {
            using (EnterTransactionContext())
            {
                foreach (var edge in base.GetEdges(key, value))
                {
                    yield return edge;
                }
            }
        }

        public override void RemoveEdge(IEdge edge)
        {
            using (EnterTransactionContext())
            {
                base.RemoveEdge(edge);
            }
        }

        public override IVertex AddVertex(object unused)
        {
            using (EnterTransactionContext())
            {
                return base.AddVertex(unused);
            }
        }

        public override IVertex GetVertex(object id)
        {
            using (EnterTransactionContext())
            {
                return base.GetVertex(id);
            }
        }

        public override IEnumerable<IVertex> GetVertices()
        {
            using (EnterTransactionContext())
            {
                foreach (var vertex in base.GetVertices())
                {
                    yield return vertex;
                }
            }
        }

        public override IEnumerable<IVertex> GetVertices(string key, object value)
        {
            using (EnterTransactionContext())
            {
                foreach (var vertex in base.GetVertices(key, value))
                {
                    yield return vertex;
                }
            }
        }

        public override void RemoveVertex(IVertex vertex)
        {
            using (EnterTransactionContext())
            {
                base.RemoveVertex(vertex);
            }
        }

        private IDisposable EnterTransactionContext()
        {
            if (_transactionScope == null)
            {
                _transactionScope = Transaction.Current != null
                    ? new TransactionScope(Transaction.Current, TimeSpan.MaxValue)
                    : new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue);

                if (Transaction.Current != null)
                    Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);

                if (_transaction == null)
                {
                    var transactionNumber = Interlocked.Increment(ref _transactionNumber);
                    _transaction = new EsentTransaction(Context.Session, transactionNumber);
                }
            }

            return _transaction.EnterSessionContext();
        }

        public override IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            using (EnterTransactionContext())
            {
                return base.CreateIndex(indexName, indexClass, indexParameters);
            }
        }

        public override IIndex GetIndex(string indexName, Type indexClass)
        {
            using (EnterTransactionContext())
            {
                return base.GetIndex(indexName, indexClass);
            }
        }

        public override IEnumerable<IIndex> GetIndices()
        {
            using (EnterTransactionContext())
            {
                foreach (var index in base.GetIndices())
                {
                    yield return index;
                }
            }
        }

        public override void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            using (EnterTransactionContext())
            {
                base.CreateKeyIndex(key, elementClass, indexParameters);
            }
        }

        public override IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            using (EnterTransactionContext())
            {
                foreach (var key in base.GetIndexedKeys(elementClass))
                {
                    yield return key;
                }
            }
        }

        public override void DropIndex(string indexName)
        {
            using (EnterTransactionContext())
            {
                base.DropIndex(indexName);
            }
        }

        public override void DropKeyIndex(string key, Type elementClass)
        {
            using (EnterTransactionContext())
            {
                base.DropKeyIndex(key, elementClass);
            }
        }

        public override IEnumerable<IEdge> GetEdges(GraveVertex vertex, Direction direction, params string[] labels)
        {
            using (EnterTransactionContext())
            {
                foreach (var edge in base.GetEdges(vertex, direction, labels))
                {
                    yield return edge;
                }
            }
        }

        public override object GetProperty(GraveElement element, string key)
        {
            using (EnterTransactionContext())
            {
                return base.GetProperty(element, key);
            }
        }

        public override IEnumerable<string> GetPropertyKeys(GraveElement element)
        {
            using (EnterTransactionContext())
            {
                foreach (var key in base.GetPropertyKeys(element))
                {
                    yield return key;
                }
            }
        }

        public override object RemoveProperty(GraveElement element, string key)
        {
            using (EnterTransactionContext())
            {
                return base.RemoveProperty(element, key);
            }
        }

        public override void SetProperty(GraveElement element, string key, object value)
        {
            using (EnterTransactionContext())
            {
                base.SetProperty(element, key, value);
            }
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            IndexingService.Prepare();
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            if (_transaction != null)
                _transaction.Commit();

            foreach (var index in _transactionalIndices.Values)
                index.Commit();
            _transactionalIndices.Clear();
            
            IndexingService.Commit();

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            if (_transaction != null)
                _transaction.Rollback();

            foreach (var index in _transactionalIndices.Values)
                index.Rollback();
            _transactionalIndices.Clear();

            IndexingService.Rollback();

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        protected override IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection, IIndexCollection userIndexCollection)
        {
            var key = string.Concat(indexName, indexType);
            GraveTransactionalIndex index;
            if (!_transactionalIndices.TryGetValue(key, out index))
            {
                index = new GraveTransactionalIndex((GraveIndex)base.CreateIndexObject(indexName, indexType, indexCollection, userIndexCollection),
                                                    (TransactionalIndexCollection)indexCollection,
                                                    (TransactionalIndexCollection)userIndexCollection);
                _transactionalIndices.Add(key, index);
            }
            return index;
        }
    }
}