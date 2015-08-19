namespace Frontenac.Redis
{
    public enum RedisTransactionMode
    {
        SingleTransaction,
        SingleBatch,
        BatchTransaction,
        Batch
    }
}