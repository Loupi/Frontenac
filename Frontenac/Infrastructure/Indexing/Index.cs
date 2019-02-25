using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    public class Index : IIndex
    {
        public Index(string indexName, Type indexType, IGraph graph, IGenerationBasedIndex genBasedIndex, IndexingService indexingService)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (genBasedIndex == null)
                throw new ArgumentNullException(nameof(genBasedIndex));
            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));

            IndexName = indexName;
            IndexType = indexType;
            Graph = graph;
            GenBasedIndex = genBasedIndex;
            IndexingService = indexingService;
        }

        public IGenerationBasedIndex GenBasedIndex { get; private set; }
        public IGraph Graph { get; private set; }
        public string IndexName { get; private set; }
        public Type IndexType { get; private set; }
        public IndexingService IndexingService { get; private set; }

        public virtual long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            GenBasedIndex.WaitForGeneration();

            return IndexingService.Get(IndexType, IndexName, key, value, true).Count();
        }

        public virtual IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            GenBasedIndex.WaitForGeneration();

            var hits = IndexingService.Get(IndexType, IndexName, key, value, true);
            return ElementsFromHits(hits);
        }

        public Type Type
        {
            get { return IndexType; }
        }

        public string Name
        {
            get { return IndexName; }
        }

        public virtual void Put(string key, object value, IElement element)
        {
            IndexContract.ValidatePut(key, value, element);

            var id = Convert.ToInt64(element.Id);
            var generation = IndexType == typeof(IVertex)
                                 ? IndexingService.UserVertexIndices.Set(id, Name, key, value)
                                 : IndexingService.UserEdgeIndices.Set(id, Name, key, value);
            GenBasedIndex.UpdateGeneration(generation);
        }

        public IEnumerable<IElement> Query(string key, object query)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            var id = Convert.ToInt64(element.Id);
            var generation = IndexingService.DeleteUserDocuments(IndexType, id, key, value);
            GenBasedIndex.UpdateGeneration(generation);
        }

        public IEnumerable<IElement> ElementsFromHits(IEnumerable<long> hits)
        {
            if (hits == null)
                throw new ArgumentNullException(nameof(hits));

            IEnumerable<IElement> elements;

            if (IndexType == typeof(IVertex))
                elements = hits.Select(hit => Graph.GetVertex(hit));
            else
                elements = hits.Select(hit => Graph.GetEdge(hit));

            return elements.Where(element => element != null);
        }
    }
}
