namespace MmGraph.Database.Records
{
    public class EdgeRecord : BaseRecord
    {
        public bool IsDirected { get; set; }
        public int OutNodeId { get; set; }
        public int InNodeId { get; set; }
        public int LabelId { get; set; }
        public int OutNodePreviousEdgeId { get; set; }
        public int OutNodeNextEdgeId { get; set; }
        public int InNodePreviousEdgeId { get; set; }
        public int InNodeNextEdgeId { get; set; }
        public int NextPropertyId { get; set; }
    }
}