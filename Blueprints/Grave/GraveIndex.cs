using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(IndexingService.IsValidIndexType(indexType));
            Contract.Requires(graph != null);
            Contract.Requires(indexingService != null);
            
            _indexName = indexName;
            _indexType = indexType;
            _graph = graph;
            _indexingService = indexingService;
        }

        public long Count(string key, object value)
        {
            _graph.WaitForGeneration();

            return _indexingService.Get(_indexType, _indexName, key, value, true).Count();
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            _graph.WaitForGeneration();

            var hits = _indexingService.Get(_indexType,_indexName, key, value, true);
            var elements = ElementsFromHits(hits);
            return new WrappingCloseableIterable<IElement>(elements);
        }

        IEnumerable<IElement> ElementsFromHits(IEnumerable<int> hits)
        {
            Contract.Requires(hits != null);
            Contract.Ensures(Contract.Result<IEnumerable<IElement>>() != null);

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
            var id = (int)element.Id;
            var generation = _indexType == typeof (IVertex) ? _indexingService.UserVertexIndices.Set(id, Name, key, value) : 
                                                              _indexingService.UserEdgeIndices.Set(id, Name, key, value);
            _graph.UpdateGeneration(generation);
        }

        public ICloseableIterable<IElement> Query(string key, object query)
        {
            _graph.WaitForGeneration();

            throw new NotImplementedException();
        }

        public void Remove(string key, object value, IElement element)
        {
            var id = (int)element.Id;
            var generation = _indexingService.DeleteUserDocuments(_indexType, id, key, value);
            _graph.UpdateGeneration(generation);
        }
    }
}
