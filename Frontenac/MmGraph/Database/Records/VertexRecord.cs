namespace MmGraph.Database.Records
{
    public class VertexRecord : BaseRecord
    {
        public int NextEdgeId { get; set; }
        public int NextPropertyId { get; set; }
    }
}