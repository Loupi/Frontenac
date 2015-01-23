using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Transactions;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Grave
{
    public class GraveTransactionalGraph : GraveGraph, ITransactionalGraph, ISinglePhaseNotification, IPromotableSinglePhaseNotification, IDisposable
    {
        private const int TransactionTimeout = 60;
        private static readonly Mutex TransactionMutex = new Mutex(false);

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

        private static readonly Features GraveTransactionalGraphFeatures = new Features
        {
            SupportsDuplicateEdges = true,
            SupportsSelfLoops = true,
            SupportsSerializableObjectProperty = true,
            SupportsBooleanProperty = true,
            SupportsDoubleProperty = true,
            SupportsFloatProperty = true,
            SupportsIntegerProperty = true,
            SupportsPrimitiveArrayProperty = true,
            SupportsUniformListProperty = true,
            SupportsMixedListProperty = true,
            SupportsLongProperty = true,
            SupportsMapProperty = true,
            SupportsStringProperty = true,
            IgnoresSuppliedIds = true,
            IsPersistent = true,
            IsRdfModel = false,
            IsWrapper = false,
            SupportsIndices = true,
            SupportsKeyIndices = true,
            SupportsVertexKeyIndex = true,
            SupportsEdgeKeyIndex = true,
            SupportsVertexIndex = true,
            SupportsEdgeIndex = true,
            SupportsTransactions = true,
            SupportsVertexIteration = true,
            SupportsEdgeIteration = true,
            SupportsEdgeRetrieval = true,
            SupportsVertexProperties = true,
            SupportsEdgeProperties = true,
            SupportsThreadedTransactions = false,
            SupportsIdProperty = true,
            SupportsLabelProperty = true
        };

        public override Features Features
        {
            get { return GraveTransactionalGraphFeatures; }
        }

        public GraveTransactionalGraph(IGraphFactory factory, 
                                       EsentInstance instance, 
                                       IndexingService indexingService, 
                                       IGraphConfiguration configuration)
            : base(factory, instance, indexingService, configuration)
        {
            Contract.Requires(factory != null);
            Contract.Requires(instance != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(configuration != null);
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
                    {
                        transaction.TransactionScope.Dispose();
                        Transaction.TransactionScope = null;
                    }
                    else
                    {
                        Rollback();
                        if (transaction.Transaction != null)
                        {
                            transaction.Transaction.Dispose();
                        }
                    }
                }

                _transactionContexts.Dispose();
            }

            _disposed = true;
        }

        #endregion

        public override void Shutdown()
        {
            foreach (var transaction in _transactionContexts.Values.Where(transaction => transaction != null))
            {
                Commit();
                if (transaction.Transaction == null) continue;
                transaction.Transaction.Dispose();
                transaction.Transaction = null;
            }

            base.Shutdown();
        }

        public void Commit()
        {
            if (Transaction.TransactionScope == null) return;
            Transaction.TransactionScope.Complete();
            Transaction.TransactionScope.Dispose();
            Transaction.TransactionScope = null;
        }

        public void Rollback()
        {
            if (Transaction.TransactionScope == null) return;
            Transaction.TransactionScope.Dispose();
            Transaction.TransactionScope = null;
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
            if (Transaction.TransactionScope != null)
                return Transaction.Transaction.EnterSessionContext();

            var scope = System.Transactions.Transaction.Current != null
                ? new TransactionScope(System.Transactions.Transaction.Current, TransactionScopeAsyncFlowOption.Enabled)
                : new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

            if (System.Transactions.Transaction.Current != null)
            {
                Transaction.TransactionScope = scope;
                System.Transactions.Transaction.Current.EnlistPromotableSinglePhase(this);
            }
            
            Transaction.Transaction = EsentContext.CreateTransaction();

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

        protected override IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection, IIndexCollection userIndexCollection)
        {
            var key = string.Concat(indexName, indexType);
            GraveTransactionalIndex index;
            if (!Transaction.TransactionalIndices.TryGetValue(key, out index))
            {
                index = new GraveTransactionalIndex((Index)base.CreateIndexObject(indexName, indexType, indexCollection, userIndexCollection),
                                                    (TransactionalIndexCollection)indexCollection,
                                                    (TransactionalIndexCollection)userIndexCollection);
                Transaction.TransactionalIndices.Add(key, index);
            }
            return index;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
                PrepareInternal();
                preparingEnlistment.Prepared();
            }
            catch (Exception x)
            {
                preparingEnlistment.ForceRollback(x);
            }
        }

        public void Commit(Enlistment enlistment)
        {
            CommitInternal();
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            RollbackInternal();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public byte[] Promote()
        {
            return TransactionInterop.GetTransmitterPropagationToken(System.Transactions.Transaction.Current);
        }

        public void Initialize()
        {
            
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            try
            {
                PrepareInternal();
                CommitInternal();
                singlePhaseEnlistment.Committed();
            }
            catch (Exception x)
            {
                singlePhaseEnlistment.Aborted(x);
            }
        }

        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            try
            {
                RollbackInternal();
                singlePhaseEnlistment.Aborted();
            }
            catch (Exception x)
            {
                singlePhaseEnlistment.Aborted(x);
            }
        }

        private void PrepareInternal()
        {
            if (!TransactionMutex.WaitOne(TimeSpan.FromSeconds(TransactionTimeout)))
                throw new TimeoutException("Could not acquire transaction mutex.");

            using (EnterTransactionContext())
            {
                foreach (var index in Transaction.TransactionalIndices.Values)
                {
                    index.Commit();
                    index.Clear();
                }

                Context.IndexingService.Prepare();
            }
        }

        private void CommitInternal()
        {
            if (TransactionMutex.WaitOne(0))
            {
                try
                {
                    using (EnterTransactionContext())
                    {
                        if (Transaction.Transaction != null)
                        {
                            Transaction.Transaction.Commit();
                            Transaction.TransactionScope.Dispose();
                            Transaction.TransactionScope = null;
                            Transaction.Transaction = null;
                        }
                        Context.IndexingService.Commit();
                    }
                }
                finally
                {
                    TransactionMutex.ReleaseMutex();
                }
            }

            System.Transactions.Transaction.Current = null;
        }

        private void RollbackInternal()
        {
            if (TransactionMutex.WaitOne(0))
            {
                try
                {
                    Context.IndexingService.Rollback();
                }
                finally
                {
                    TransactionMutex.ReleaseMutex();
                }
            }

            if (Transaction.Transaction != null)
            {
                Transaction.Transaction.Rollback();
                Transaction.TransactionScope.Dispose();
                Transaction.TransactionScope = null;
                Transaction.Transaction = null;
            }
            
            foreach (var index in Transaction.TransactionalIndices.Values)
                index.Clear();

            var esentContext = EsentContext;
            esentContext.VertexTable.RefreshColumns();
            esentContext.EdgesTable.RefreshColumns();

            System.Transactions.Transaction.Current = null;
        }
    }
}