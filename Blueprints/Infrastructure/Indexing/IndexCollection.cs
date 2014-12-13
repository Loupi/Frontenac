﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    public class IndexCollection : IIndexCollection
    {
        private readonly Type _indexType;
        private readonly IndexingService _indexingService;
        private readonly List<string> _indices;
        private readonly string _indicesColumnName;
        private readonly bool _isUserIndex;

        public IndexCollection(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indicesColumnName));
            Contract.Requires(indexingService != null);

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
            return _indices.AsReadOnly();
        }

        public bool HasIndex(string indexName)
        {
           return _indices.Contains(indexName);
        }

        public long Set(long id, string indexName, string key, object value)
        {
            return _indexingService.Set(_indexType, id, indexName, key, value, _isUserIndex);
        }

        public void WaitForGeneration(long generation)
        {
            _indexingService.WaitForGeneration(generation);
        }

        public IEnumerable<long> Get(string term, object value, int hitsLimit)
        {
            return _indexingService.Get(_indexType, null, term, value, _isUserIndex, hitsLimit);
        }

        public IEnumerable<long> Get(string indexName, string key, object value, int hitsLimit)
        {
            return _indexingService.Get(_indexType, indexName, key, value, _isUserIndex, hitsLimit);
        }

        public long DeleteDocuments(long id)
        {
            return _indexingService.DeleteDocuments(_indexType, id);
        }

        public long DeleteIndex(string indexName)
        {
            return _indexingService.DeleteIndex(_indexType, indexName, _isUserIndex);
        }

        public void Commit()
        {

        }

        public void Rollback()
        {
            
        }
    }
}