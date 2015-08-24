using System.Threading;
using Frontenac.Infrastructure.Indexing;
using StackExchange.Redis;

namespace Frontenac.Redis
{
    public class TransactionManager
    {
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly IndexingService _indexingService;
        private IBatch _batch;

        public RedisTransactionMode Mode { get; private set; }

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
            if (Mode == RedisTransactionMode.SingleBatch || Mode == RedisTransactionMode.SingleTransaction)
            {
                if (_batch != null)
                {
                    _batch.Execute();
                    _batch = null;
                }

                _indexingService.Commit();
            }
        }

        public void Commit()
        {
            if (_batch != null)
            {
                _batch.Execute();
                _indexingService.Commit();
                _batch = null;
                Thread.Sleep(3000);
            }
        }

        public void Rollback()
        {
            _indexingService.Rollback();
            _batch = null;
        }
    }
}