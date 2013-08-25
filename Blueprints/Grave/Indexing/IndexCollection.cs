using System;
using System.Collections.Generic;

namespace Grave.Indexing
{
    public class IndexCollection
    {
        readonly Type _indexType;
        readonly bool _isUserIndex;
        readonly string _indicesColumnName;
        readonly IndexingService _indexingService;
        readonly List<string> _indices;
        
        public IndexCollection(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            _indicesColumnName = indicesColumnName;
            _indexingService = indexingService;
            _indexType = indexType;
            _isUserIndex = isUserIndex;
            _indices = _indexingService.GetIndicesOfType(_indicesColumnName);
        }

        public void CreateIndex(string indexName)
        {
            _indexingService.CreateIndexOfType(indexName, _indicesColumnName, _indices);
        }

        public long DropIndex(string indexName)
        {
            return _indexingService.DropIndexOfType(indexName, _indicesColumnName, _indexType, _indices, _isUserIndex);
        }

        public IEnumerable<string> GetIndices()
        {
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
            return _indexingService.Set(_indexType, id, indexName, key, value, _isUserIndex);
        }

        public void WaitForGeneration(long generation)
        {
            _indexingService.WaitForGeneration(generation);
        }

        public IEnumerable<int> Get(string term, object value, int hitsLimit = 1000)
        {
            return _indexingService.Get(_indexType, null, term, value, _isUserIndex, hitsLimit);
        }

        public IEnumerable<int> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            return _indexingService.Get(_indexType, indexName, key, value, _isUserIndex, hitsLimit);
        }

        public long DeleteDocuments(int id)
        {
            return _indexingService.DeleteDocuments(_indexType, id);
        }

        public long DeleteIndex(string indexName)
        {
            return _indexingService.DeleteIndex(_indexType, indexName, _isUserIndex);
        }
    }
}