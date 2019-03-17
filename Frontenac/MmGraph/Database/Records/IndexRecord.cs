using System.Threading;

namespace MmGraph.Database.Records
{
    public class IndexRecord : BaseRecord
    {
        private int _count;
        
        public int Count
        {
            get => _count;
            set => _count = value;
        }

        public int KeyBlockId { get; set; }
        public string Key { get; set; }

        public void IncrementCount()
        {
            Interlocked.Increment(ref _count);
        }

        public void DecrementCount()
        {
            Interlocked.Decrement(ref _count);
        }
    }
}