using System.Collections.Generic;
using Frontenac.Blueprints;
using System;
using System.Linq;
using Frontenac.Blueprints.Util;
using Grave.Indexing;

namespace Grave
{
    public class GraveIndex : IIndex
    {
        readonly string _indexName;
        readonly Type _indexType;
        readonly GraveGraph _graph;
        readonly IndexingService _indexingService;

        public GraveIndex(string indexName, Type indexType, GraveGraph graph, IndexingService indexingService)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("indexName");

            if (indexType == null)
                throw new ArgumentNullException("indexType");

            if (graph == null)
                throw new ArgumentNullException("graph");

            if (indexingService == null)
                throw new ArgumentNullException("indexingService");

            _indexName = indexName;
            _indexType = indexType;
            _graph = graph;
            _indexingService = indexingService;
        }

        public long Count(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            _graph.WaitForGeneration();

            return _indexingService.Get(_indexType, _indexName, key, value, true).Count();
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            _graph.WaitForGeneration();

            var hits = _indexingService.Get(_indexType,_indexName, key, value, true);
            var elements = ElementsFromHits(hits);
            return new WrappingCloseableIterable<IElement>(elements);
        }

        IEnumerable<IElement> ElementsFromHits(IEnumerable<int> hits)
        {
            IEnumerable<IElement> elements;

            if(_indexType == typeof(IVertex))
                elements = hits.Select(hit => _graph.GetVertex(hit));
            else
                elements = hits.Select(hit => _graph.GetEdge(hit));

            return elements.Where(element => element != null);
        }

        public Type Type
        {
            get { return _indexType; }
        }

        public string Name
        {
            get { return _indexName; }
        }

        public void Put(string key, object value, IElement element)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (element == null)
                throw new ArgumentNullException("element");

            var id = (int)element.Id;
            long generation;
            if (_indexType == typeof (IVertex))
                generation = _indexingService.UserVertexIndices.Set(id, Name, key, value);
            else
                generation = _indexingService.UserEdgeIndices.Set(id, Name, key, value);

            _graph.UpdateGeneration(generation);
        }

        public ICloseableIterable<IElement> Query(string key, object query)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (query == null)
                throw new ArgumentNullException("query");

            _graph.WaitForGeneration();

            throw new NotImplementedException();
        }

        public void Remove(string key, object value, IElement element)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (element == null)
                throw new ArgumentNullException("element");

            var id = (int)element.Id;
            var generation = _indexingService.DeleteUserDocuments(_indexType, id, key, value);
            _graph.UpdateGeneration(generation);
        }
    }
}
