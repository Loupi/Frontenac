namespace MmGraph.Database.Records
{
    public class BlockRecord : BaseRecord
    {
        public int PreviousBlockId { get; set; }
        public int NextBlockId { get; set; }
        public int Length { get; set; }
    }
}