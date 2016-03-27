using System.Threading.Tasks;
using Frontenac.Infrastructure.Indexing;
using StackExchange.Redis;

namespace Frontenac.Redis
{
    public class TransactionManager
    {
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly IndexingService _indexingService;
        private IBatch _batch;

        public RedisTransactionMode Mode { get; }

        public TransactionManager(RedisTransactionMode mode, ConnectionMultiplexer multiplexer, IndexingService indexingService)
        {
            Mode = mode;
            _multiplexer = multiplexer;
            _indexingService = indexingService;
        }

        public IBatch Begin(out IDatabase db)
        {
            db = _multiplexer.GetDatabase();

            if (_batch == null)
            {
                if (Mode == RedisTransactionMode.BatchTransaction || Mode == RedisTransactionMode.SingleTransaction)
                    _batch = db.CreateTransaction();
                else
                    _batch = db.CreateBatch();
            }

            return _batch;
        }

        public void End()
        {
            if (Mode != RedisTransactionMode.SingleBatch && Mode != RedisTransactionMode.SingleTransaction) return;

            if (_batch != null)
            {
                _batch.Execute();
                _batch = null;
            }

            _indexingService.Commit();
        }

        public void Commit()
        {
            if (_batch == null) return;
            var transaction = _batch as ITransaction;
            if(transaction != null)
            {
                var t1 = transaction.ExecuteAsync();
                var t2 = _indexingService.CommitAsync();

                if (t2 != null)
                    Task.WaitAll(t1, t2);
                else
                    t1.Wait();

            }
            else
            {
                _batch.Execute();
                _indexingService.Commit();
            }
                
            _batch = null;
        }

        public void Rollback()
        {
            _indexingService.Rollback();
            _batch = null;
        }
    }
}