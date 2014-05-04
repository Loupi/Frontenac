using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Grave.Indexing
{
    public class TransactionalIndexCollection : IIndexCollection
    {
        private readonly IndexCollection _indexCollection;

        readonly List<string> _newIndices = new List<string>();
        readonly List<string> _droppedIndices = new List<string>();
        readonly List<string> _deletedIndices = new List<string>();
        internal readonly List<int> DeletedDocuments = new List<int>();
        readonly Dictionary<string, Tuple<int, string, string, object>> _setIndices
            = new Dictionary<string, Tuple<int, string, string, object>>();

        public TransactionalIndexCollection(IndexCollection indexCollection)
        {
            Contract.Requires(indexCollection != null);

            _indexCollection = indexCollection;
        }

        public void CreateIndex(string indexName)
        {
            if (_droppedIndices.Contains(indexName))
                _droppedIndices.Remove(indexName);

            if(!HasIndex(indexName))
                _newIndices.Add(indexName);
        }

        public long DropIndex(string indexName)
        {
            if (_newIndices.Contains(indexName))
                _newIndices.Remove(indexName);
            else if(_indexCollection.HasIndex(indexName)) 
                _droppedIndices.Add(indexName);

            foreach (var setToRemove in _setIndices.Where(t => t.Value.Item2 == indexName))
                _setIndices.Remove(setToRemove.Key);

            return 0;
        }

        public IEnumerable<string> GetIndices()
        {
            return _indexCollection.GetIndices()
                .Concat(_newIndices)
                .Except(_droppedIndices);
        }

        public bool HasIndex(string indexName)
        {
            return (_indexCollection.HasIndex(indexName) || 
                    _newIndices.Contains(indexName)) && 
                    !_droppedIndices.Contains(indexName);
        }

        public long Set(int id, string indexName, string key, object value)
        {
            var setKey = string.Concat(id, indexName);
            var setValue = new Tuple<int, string, string, object>(id, indexName, key, value);
            if(!_setIndices.ContainsKey(setKey))
                _setIndices.Add(setKey, setValue);
            else
                _setIndices[setKey] = setValue;
            return 0;
        }

        public void WaitForGeneration(long generation)
        {
            
        }

        public IEnumerable<int> Get(string term, object value, int hitsLimit = 1000)
        {
            var excepted = _droppedIndices
                .Concat(_deletedIndices)
                .SelectMany(t => _indexCollection.Get(t, term, value, hitsLimit))
                .Concat(DeletedDocuments)
                .Distinct();
            
            return _setIndices
                .Where(t => t.Value.Item3 == term && t.Value.Item4 == value)
                .Select(t => t.Value.Item1)
                .Concat(_indexCollection.Get(term, value, hitsLimit))
                .Except(excepted)
                .Distinct()
                .ToArray();
        }

        public IEnumerable<int> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            if (_deletedIndices.Contains(indexName) || _droppedIndices.Contains(indexName))
                return Enumerable.Empty<int>();

            return _setIndices.Where(t => t.Value.Item2 == indexName && t.Value.Item3 == key && t.Value.Item4 == value)
                .Select(t => t.Value.Item1)
                .Concat(_indexCollection.Get(indexName, key, value, hitsLimit).Except(DeletedDocuments))
                .Distinct();
        }

        public long DeleteDocuments(int id)
        {
            if(!DeletedDocuments.Contains(id))
                DeletedDocuments.Add(id);
            return 0;
        }

        public long DeleteIndex(string indexName)
        {
            foreach (var setToRemove in _setIndices.Where(t => t.Value.Item2 == indexName))
                _setIndices.Remove(setToRemove.Key);

            if (!_deletedIndices.Contains(indexName))
                _deletedIndices.Add(indexName);
            return 0;
        }

        public void Commit()
        {
            foreach (var deletedDocument in DeletedDocuments)
                _indexCollection.DeleteDocuments(deletedDocument);

            foreach (var droppedIndex in _droppedIndices)
                _indexCollection.DropIndex(droppedIndex);

            foreach (var deletedIndex in _deletedIndices)
                _indexCollection.DeleteIndex(deletedIndex);

            foreach (var newIndex in _newIndices)
                _indexCollection.CreateIndex(newIndex);

            foreach (var setIndex in _setIndices)
                _indexCollection.Set(setIndex.Value.Item1, setIndex.Value.Item2, 
                                     setIndex.Value.Item3, setIndex.Value.Item4);
        }

        public void Rollback()
        {
            DeletedDocuments.Clear();
            _droppedIndices.Clear();
            _deletedIndices.Clear();
            _newIndices.Clear();
            _setIndices.Clear();

            _indexCollection.Rollback();
        }
    }
}
