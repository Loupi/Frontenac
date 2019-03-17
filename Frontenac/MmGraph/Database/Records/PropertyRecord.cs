namespace MmGraph.Database.Records
{
    public class PropertyRecord : BaseRecord
    {
        public PropertyType PropertyType { get; set; }
        public int KeyIndexId { get; set; }
        public long PropertyBlockId { get; set; }
        public int PreviousPropertyId { get; set; }
        public int NextPropertyId { get; set; }
        public object Value { get; set; }
    }
}