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

        private readonly ThreadLocal<TransactionContext> _transactionContexts 
            = new ThreadLocal<TransactionContext>(() => new TransactionContext(), true);

        public class TransactionContext
        {
            public TransactionContext()
            {
                TransactionalIndices = new Dictionary<string, GraveTransactionalIndex>();
            }

            public EsentTransaction Transaction { get; set; }
            public TransactionScope TransactionScope { get; set; }
            public Dictionary<string, GraveTransactionalIndex> TransactionalIndices { get; private set; } 
        }
        
        public TransactionContext Transaction
        {
            get { return _transactionContexts.Value; }
        }

        public GraveTransactionalGraph(IGraveGraphFactory factory, 
                                       EsentContext context, 
                                       IIndexingServiceFactory indexingServiceFactory)
            : base(factory, indexingServiceFactory)
        {
            Contract.Requires(factory != null);
            Contract.Requires(context != null);
            Contract.Requires(indexingServiceFactory != null);
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

            if (disposing)
            {
                foreach (var transaction in _transactionContexts.Values)
                {
                    if (transaction.TransactionScope != null)
                        transaction.TransactionScope.Dispose();
                    else
                    {
                        Rollback();
                        if (transaction.Transaction != null)
                        {
                            transaction.Transaction.Dispose();
                        }
                    }
                }
            }

            _disposed = true;
        }

        #endregion

        public override void Shutdown()
        {
            foreach (var transaction in _transactionContexts.Values)
            {
                if (transaction != null)
                {
                    Commit();
                    if (transaction.Transaction != null)
                    {
                        transaction.Transaction.Dispose();
                        transaction.Transaction = null;
                    }
                }
            }
            
            //Commit();
            base.Shutdown();
        }

        public void Commit()
        {
            if (Transaction.Transaction != null)
                Transaction.Transaction.Commit();

            foreach (var index in Transaction.TransactionalIndices.Values)
            {
                index.Commit();
                index.Clear();
            }

            Context.IndexingService.Commit();
        }

        public void Rollback()
        {
            if (Transaction.Transaction != null)
                Transaction.Transaction.Rollback();

            foreach (var index in Transaction.TransactionalIndices.Values)
                index.Clear();

            Context.IndexingService.Rollback();
            
            Context.Context.VertexTable.RefreshColumns();
            Context.Context.EdgesTable.RefreshColumns();
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
            if (Transaction.TransactionScope == null)
            {
                if (System.Transactions.Transaction.Current != null)
                {
                    Transaction.TransactionScope = new TransactionScope(System.Transactions.Transaction.Current);
                    System.Transactions.Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                }
            }

            if (Transaction.Transaction == null)
            {
                var transactionNumber = Interlocked.Increment(ref _transactionNumber);
                Transaction.Transaction = new EsentTransaction(Context.Context.Session, transactionNumber);
            }

            return Transaction.Transaction.EnterSessionContext();
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
            Context.IndexingService.Prepare();
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

        protected override IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection, IIndexCollection userIndexCollection)
        {
            var key = string.Concat(indexName, indexType);
            GraveTransactionalIndex index;
            if (!Transaction.TransactionalIndices.TryGetValue(key, out index))
            {
                index = new GraveTransactionalIndex((GraveIndex)base.CreateIndexObject(indexName, indexType, indexCollection, userIndexCollection),
                                                    (TransactionalIndexCollection)indexCollection,
                                                    (TransactionalIndexCollection)userIndexCollection);
                Transaction.TransactionalIndices.Add(key, index);
            }
            return index;
        }
    }
}