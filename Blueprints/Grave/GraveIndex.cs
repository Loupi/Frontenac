using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Grave.Indexing;

namespace Frontenac.Grave
{
    public class GraveIndex : IIndex
    {
        public GraveIndex(string indexName, Type indexType, GraveGraph graph, IndexingService indexingService)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(IndexingService.IsValidIndexType(indexType));
            Contract.Requires(graph != null);
            Contract.Requires(indexingService != null);

            IndexName = indexName;
            IndexType = indexType;
            Graph = graph;
            IndexingService = indexingService;
        }

        public GraveGraph Graph { get; private set; }
        public string IndexName { get; private set; }
        public Type IndexType { get; private set; }
        public IndexingService IndexingService { get; private set; }

        public virtual long Count(string key, object value)
        {
            Graph.WaitForGeneration();

            return IndexingService.Get(IndexType, IndexName, key, value, true).Count();
        }

        public virtual IEnumerable<IElement> Get(string key, object value)
        {
            Graph.WaitForGeneration();

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
            var id = (int) element.Id;
            var generation = IndexType == typeof (IVertex)
                                 ? IndexingService.UserVertexIndices.Set(id, Name, key, value)
                                 : IndexingService.UserEdgeIndices.Set(id, Name, key, value);
            Graph.UpdateGeneration(generation);
        }

        public IEnumerable<IElement> Query(string key, object query)
        {
            Graph.WaitForGeneration();

            throw new NotImplementedException();
        }

        public virtual void Remove(string key, object value, IElement element)
        {
            var id = (int) element.Id;
            var generation = IndexingService.DeleteUserDocuments(IndexType, id, key, value);
            Graph.UpdateGeneration(generation);
        }

        internal IEnumerable<IElement> ElementsFromHits(IEnumerable<int> hits)
        {
            Contract.Requires(hits != null);
            Contract.Ensures(Contract.Result<IEnumerable<IElement>>() != null);

            IEnumerable<IElement> elements;

            if (IndexType == typeof (IVertex))
                elements = hits.Select(hit => Graph.GetVertex(hit));
            else
                elements = hits.Select(hit => Graph.GetEdge(hit));

            return elements.Where(element => element != null);
        }
    }
}