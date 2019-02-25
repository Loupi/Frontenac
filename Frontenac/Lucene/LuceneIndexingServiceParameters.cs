using System;

namespace Frontenac.Lucene
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
                return _default;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _default = value;
            }
        }

        public string VertexIdColumnName
        {
            get
            {
                return _vertexIdColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _vertexIdColumnName = value;
            }
        }

        public string VertexKeyColumnName
        {
            get
            {
                return _vertexKeyColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _vertexKeyColumnName = value;
            }
        }

        public string VertexIndexColumnName
        {
            get
            {
                return _vertexIndexColumnName;
            }
            set { _vertexIndexColumnName = value; }
        }

        public string EdgeIdColumnName
        {
            get
            {
                return _edgeIdColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _edgeIdColumnName = value;
            }
        }

        public string EdgeKeyColumnName
        {
            get
            {
                return _edgeKeyColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _edgeKeyColumnName = value;
            }
        }

        public string EdgeIndexColumnName
        {
            get
            {
                return _edgeIndexColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _edgeIndexColumnName = value;
            }
        }

        public string NullColumnName
        {
            get
            {
                return _nullColumnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));
                _nullColumnName = value;
            }
        }

        public int CloseTimeoutSeconds { get; set; }
        public int MaxStaleSeconds { get; set; }
        public int MinStaleMilliseconds { get; set; }
    }
}