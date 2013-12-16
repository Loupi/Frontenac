using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    public class IndexCollection
    {
        private readonly Type _indexType;
        private readonly IndexingService _indexingService;
        private readonly List<string> _indices;
        private readonly string _indicesColumnName;
        private readonly bool _isUserIndex;

        public IndexCollection(string indicesColumnName, Type indexType, bool isUserIndex,
                               IndexingService indexingService)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indicesColumnName));
            Contract.Requires(IndexingService.IsValidIndexType(indexType));
            Contract.Requires(indexingService != null);

            _indicesColumnName = indicesColumnName;
            _indexingService = indexingService;
            _indexType = indexType;
            _isUserIndex = isUserIndex;
            _indices = _indexingService.GetIndicesOfType(_indicesColumnName);
        }

        public void CreateIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));

            _indexingService.CreateIndexOfType(indexName, _indicesColumnName, _indices);
        }

        public long DropIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));

            return _indexingService.DropIndexOfType(indexName, _indicesColumnName, _indexType, _indices, _isUserIndex);
        }

        public IEnumerable<string> GetIndices()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            IEnumerable<string> result;

            _indexingService.IndicesLock.EnterReadLock();
            try
            {
                result = _indices.AsReadOnly();
            }
            finally
            {
                _indexingService.IndicesLock.ExitReadLock();
            }

            return result;
        }

        public bool HasIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));

            bool result;

            _indexingService.IndicesLock.EnterReadLock();
            try
            {
                result = _indices.Contains(indexName);
            }
            finally
            {
                _indexingService.IndicesLock.ExitReadLock();
            }

            return result;
        }

        public long Set(int id, string indexName, string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            return _indexingService.Set(_indexType, id, indexName, key, value, _isUserIndex);
        }

        public void WaitForGeneration(long generation)
        {
            _indexingService.WaitForGeneration(generation);
        }

        public IEnumerable<int> Get(string term, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(term));
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);

            return _indexingService.Get(_indexType, null, term, value, _isUserIndex, hitsLimit);
        }

        public IEnumerable<int> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);

            return _indexingService.Get(_indexType, indexName, key, value, _isUserIndex, hitsLimit);
        }

        public long DeleteDocuments(int id)
        {
            return _indexingService.DeleteDocuments(_indexType, id);
        }

        public long DeleteIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));

            return _indexingService.DeleteIndex(_indexType, indexName, _isUserIndex);
        }
    }
}