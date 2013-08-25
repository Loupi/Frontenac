namespace Grave.Indexing.Lucene
{
    public class LuceneIndexingServiceParameters
    {
        public string VertexIdColumnName { get; set; }
        public string VertexKeyColumnName { get; set; }
        public string VertexIndexColumnName { get; set; }
        public string EdgeIdColumnName { get; set; }
        public string EdgeKeyColumnName { get; set; }
        public string EdgeIndexColumnName { get; set; }
        public string NullColumnName { get; set; }
        public int CloseTimeoutSeconds { get; set; }
        public int MaxStaleSeconds { get; set; }
        public int MinStaleMilliseconds { get; set; }

        public static LuceneIndexingServiceParameters Default { get; private set; }

        static LuceneIndexingServiceParameters()
        {
            Default = new LuceneIndexingServiceParameters
                {
                    VertexIdColumnName = "$vi",
                    VertexKeyColumnName = "$vk",
                    VertexIndexColumnName = "$vx",
                    EdgeIdColumnName = "$ei",
                    EdgeKeyColumnName = "$ek",
                    EdgeIndexColumnName = "$ex",
                    NullColumnName = "$nv",
                    CloseTimeoutSeconds = 10,
                    MaxStaleSeconds = 5,
                    MinStaleMilliseconds = 25
                };
        }
    }
}
