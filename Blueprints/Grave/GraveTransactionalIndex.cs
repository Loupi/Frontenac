using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Grave.Indexing;

namespace Frontenac.Grave
{
    public class GraveTransactionalIndex : IIndex
    {
        readonly Dictionary<string, List<KeyValuePair<object, IElement>>> _putBuffer = 
            new Dictionary<string, List<KeyValuePair<object, IElement>>>();

        readonly Dictionary<string, List<KeyValuePair<object, IElement>>> _removeBuffer =
            new Dictionary<string, List<KeyValuePair<object, IElement>>>();

        private readonly GraveIndex _index;
        private readonly TransactionalIndexCollection _indexCollection;
        private readonly TransactionalIndexCollection _userIndexCollection;

        public GraveTransactionalIndex(GraveIndex index, TransactionalIndexCollection indexCollection, 
                                       TransactionalIndexCollection userIndexCollection)
        {
            Contract.Requires(index != null);
            Contract.Requires(indexCollection != null);

            _index = index;
            _indexCollection = indexCollection;
            _userIndexCollection = userIndexCollection;
        }

        public virtual void Put(string key, object value, IElement element)
        {
            List<KeyValuePair<object, IElement>> putBuffer;
            if (!_putBuffer.TryGetValue(key, out putBuffer))
            {
                putBuffer = new List<KeyValuePair<object, IElement>>();
                _putBuffer.Add(key, putBuffer);
            }
            putBuffer.Add(new KeyValuePair<object, IElement>(value, element));
        }

        public virtual long Count(string key, object value)
        {
            List<KeyValuePair<object, IElement>> putBuffer;
            var result = _putBuffer.TryGetValue(key, out putBuffer) ?
                putBuffer.Where(t => t.Key == value).Select(t => t.Value) :
                Enumerable.Empty<IElement>();

            var lookInOriginal = true;
            List<KeyValuePair<object, IElement>> removeBuffer;
            if (_removeBuffer.TryGetValue(key, out removeBuffer))
                lookInOriginal = removeBuffer.Any(t => t.Key == value);

            if (!lookInOriginal)
                return result.Count(t => !_indexCollection.DeletedDocuments.Contains((int)t.Id) &&
                                         !_userIndexCollection.DeletedDocuments.Contains((int)t.Id));

            var hits = _index.IndexingService.Get(_index.IndexType, _index.IndexName, key, value, true)
                .Except(_indexCollection.DeletedDocuments);
            return hits.Count() + result.Count();
        }

        public virtual IEnumerable<IElement> Get(string key, object value)
        {
            List<KeyValuePair<object, IElement>> putBuffer;
            var result = _putBuffer.TryGetValue(key, out putBuffer) ? 
                putBuffer.Where(t => t.Key == value)
                         .Select(t => t.Value)
                         .Where(t => !_indexCollection.DeletedDocuments.Contains((int)t.Id) &&
                                     !_userIndexCollection.DeletedDocuments.Contains((int)t.Id)) :
                Enumerable.Empty<IElement>();

            var lookInOriginal = true;
            List<KeyValuePair<object, IElement>> removeBuffer;
            if (_removeBuffer.TryGetValue(key, out removeBuffer))
                lookInOriginal = removeBuffer.Any(t => t.Key == value);

            if (!lookInOriginal)
                return result;

            var hits = _index.IndexingService.Get(_index.IndexType, _index.IndexName, key, value, true)
                .Except(_indexCollection.DeletedDocuments)
                .Except(_userIndexCollection.DeletedDocuments);

            return result.Concat(_index.ElementsFromHits(hits));
        }

        public virtual void Remove(string key, object value, IElement element)
        {
            List<KeyValuePair<object, IElement>> putBuffer;
            var nbPutRemoved = 0;
            if (_putBuffer.TryGetValue(key, out putBuffer))
                nbPutRemoved = putBuffer.RemoveAll(t => t.Key == value && ((int)t.Value.Id == (int)element.Id));

            List<KeyValuePair<object, IElement>> removeBuffer = null;
            if (nbPutRemoved == 0 && !_removeBuffer.TryGetValue(key, out removeBuffer))
            {
                removeBuffer = new List<KeyValuePair<object, IElement>>();
                _removeBuffer.Add(key, removeBuffer);
            }
            if(removeBuffer != null)
                removeBuffer.Add(new KeyValuePair<object, IElement>(value, element));
        }

        public void Commit()
        {
            foreach (var putBuffer in _putBuffer)
            {
                foreach (var toPut in putBuffer.Value)
                {
                    _index.Put(putBuffer.Key, toPut.Key, toPut.Value);
                }
            }

            foreach (var removeBuffer in _removeBuffer)
            {
                foreach (var toRemove in removeBuffer.Value)
                {
                    _index.Remove(removeBuffer.Key, toRemove.Key, toRemove.Value);
                }
            }

            _index.Graph.WaitForGeneration();
        }

        public void Rollback()
        {
            _putBuffer.Clear();
            _removeBuffer.Clear();
        }

        public Type Type
        {
            get { return _index.IndexType; }
        }

        public string Name
        {
            get { return _index.IndexName; }
        }

        public IEnumerable<IElement> Query(string key, object query)
        {
            throw new NotImplementedException();
        }
    }
}