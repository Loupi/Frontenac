using StackExchange.Redis;

namespace Frontenac.Redis
{
    public class RedisTransaction
    {
        private readonly RedisTransactionMode _mode;
        private readonly ConnectionMultiplexer _multiplexer;
        private IBatch _batch;

        public RedisTransaction(RedisTransactionMode mode, ConnectionMultiplexer multiplexer)
        {
            _mode = mode;
            _multiplexer = multiplexer;
        }

        public IBatch Begin(out IDatabase db)
        {
            db = _multiplexer.GetDatabase();

            if (_batch == null)
            {
                if (_mode == RedisTransactionMode.BatchTransaction || _mode == RedisTransactionMode.SingleTransaction)
                    _batch = db.CreateTransaction();
                else
                    _batch = db.CreateBatch();
            }

            return _batch;
        }

        public void End()
        {
            if ((_mode != RedisTransactionMode.SingleBatch && _mode != RedisTransactionMode.SingleTransaction) ||
                _batch == null) return;
            _batch.Execute();
            _batch = null;
        }

        public void Commit()
        {
            if (_batch != null)
            {
                _batch.Execute();
                _batch = null;
            }
        }

        public void Rollback()
        {
            _batch = null;
        }
    }
}