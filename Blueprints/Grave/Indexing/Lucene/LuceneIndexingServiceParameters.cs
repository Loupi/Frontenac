using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing.Lucene
{
    public class LuceneIndexingServiceParameters
    {
        private static LuceneIndexingServiceParameters _default;
        private string _edgeIdColumnName;
        private string _edgeIndexColumnName;
        private string _edgeKeyColumnName;
        private string _nullColumnName;
        private string _vertexIdColumnName;
        private string _vertexIndexColumnName;
        private string _vertexKeyColumnName;

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

        public static LuceneIndexingServiceParameters Default
        {
            get
            {
                Contract.Ensures(Contract.Result<LuceneIndexingServiceParameters>() != null);
                return _default;
            }
            private set
            {
                Contract.Requires(value != null);
                _default = value;
            }
        }

        public string VertexIdColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _vertexIdColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _vertexIdColumnName = value;
            }
        }

        public string VertexKeyColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _vertexKeyColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _vertexKeyColumnName = value;
            }
        }

        public string VertexIndexColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _vertexIndexColumnName;
            }
            set { _vertexIndexColumnName = value; }
        }

        public string EdgeIdColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _edgeIdColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _edgeIdColumnName = value;
            }
        }

        public string EdgeKeyColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _edgeKeyColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _edgeKeyColumnName = value;
            }
        }

        public string EdgeIndexColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _edgeIndexColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _edgeIndexColumnName = value;
            }
        }

        public string NullColumnName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _nullColumnName;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _nullColumnName = value;
            }
        }

        public int CloseTimeoutSeconds { get; set; }
        public int MaxStaleSeconds { get; set; }
        public int MinStaleMilliseconds { get; set; }
    }
}