using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Infrastructure
{
    public abstract class IndexedGraph : IKeyIndexableGraph, IIndexableGraph, IGenerationBasedIndex
    {
        protected readonly ThreadLocal<ThreadContext> Contexts;

        protected IndexedGraph(IIndexingServiceFactory indexingServiceFactory)
        {
            Contract.Requires(indexingServiceFactory != null);

            Contexts = new ThreadLocal<ThreadContext>(() => CreateThreadContext(indexingServiceFactory), true);
        }

        protected abstract ThreadContext CreateThreadContext(IIndexingServiceFactory indexingServiceFactory);
        protected abstract IVertex GetVertexInstance(long vertexId);

        public abstract Features Features { get; }
        public abstract IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label);
        public abstract IVertex AddVertex(object id);
        public abstract IEdge GetEdge(object id);
        public abstract IEnumerable<IEdge> GetEdges();
        public abstract IVertex GetVertex(object id);
        public abstract IEnumerable<IVertex> GetVertices();
        public abstract void Shutdown();

        public virtual IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (!Context.IndexingService.EdgeIndices.HasIndex(key))
                return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());

            WaitForGeneration();
            return IterateEdges(key, value);
        }

        public virtual IEnumerable<IVertex> GetVertices(string key, object value)
        {
            if (!Context.IndexingService.VertexIndices.HasIndex(key))
                return new PropertyFilteredIterable<IVertex>(key, value, GetVertices());

            WaitForGeneration();
            return Context.IndexingService.VertexIndices.Get(key, key, value, int.MaxValue)
                .Select(GetVertexInstance);
        }

        public virtual void RemoveEdge(IEdge edge)
        {
            var id = edge.Id.TryToInt64();
            if(!id.HasValue)throw new InvalidOperationException();
            var generation = Context.IndexingService.EdgeIndices.DeleteDocuments(id.Value);
            UpdateGeneration(generation);
        }

        public virtual void RemoveVertex(IVertex vertex)
        {
            var id = vertex.Id.TryToInt64();
            if (!id.HasValue) throw new InvalidOperationException();
            var generation = Context.IndexingService.VertexIndices.DeleteDocuments(id.Value);
            UpdateGeneration(generation);
        }

        public ThreadContext Context
        {
            get { return Contexts.Value; }
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (GetIndices(typeof(IVertex), true).HasIndex(indexName) ||
                GetIndices(typeof(IEdge), true).HasIndex(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            var indexCollection = GetIndices(indexClass, true);
            var userIndexCollection = GetIndices(indexClass, false);
            indexCollection.CreateIndex(indexName);
            return CreateIndexObject(indexName, indexClass, indexCollection, userIndexCollection);
        }

        public virtual IIndex GetIndex(string indexName, Type indexClass)
        {
            var indexCollection = GetIndices(indexClass, true);
            var userIndexCollection = GetIndices(indexClass, false);
            return indexCollection.HasIndex(indexName)
                       ? CreateIndexObject(indexName, indexClass, indexCollection, userIndexCollection)
                       : null;
        }

        public virtual IEnumerable<IIndex> GetIndices()
        {
            var vertexIndexCollection = GetIndices(typeof(IVertex), true);
            var edgeIndexCollection = GetIndices(typeof(IEdge), true);

            var userVertexIndexCollection = GetIndices(typeof(IVertex), false);
            var userEdgeIndexCollection = GetIndices(typeof(IEdge), false);

            return vertexIndexCollection.GetIndices()
                .Select(t => CreateIndexObject(t, typeof(IVertex), vertexIndexCollection, userVertexIndexCollection))
                .Concat(edgeIndexCollection.GetIndices()
                        .Select(t => CreateIndexObject(t, typeof(IEdge), edgeIndexCollection, userEdgeIndexCollection)));
        }

        protected virtual IIndex CreateIndexObject(string indexName, Type indexType, IIndexCollection indexCollection, IIndexCollection userIndexCollection)
        {
            var key = string.Concat(indexName, indexType);
            Index index;
            if (!Context.Indices.TryGetValue(key, out index))
            {
                index = new Index(indexName, indexType, this, this, Context.IndexingService);
                Context.Indices.Add(key, index);
            }
            return index;
        }

        public virtual void DropIndex(string indexName)
        {
            long generation = -1;
            if (GetIndices(typeof(IVertex), true).HasIndex(indexName))
                generation = GetIndices(typeof(IVertex), true).DropIndex(indexName);
            else if (GetIndices(typeof(IEdge), true).HasIndex(indexName))
                generation = GetIndices(typeof(IEdge), true).DropIndex(indexName);

            if (generation != -1)
                UpdateGeneration(generation);
        }

        internal IIndexCollection GetIndices(Type indexType, bool isUserIndex)
        {
            Contract.Requires(indexType != null);
            Contract.Requires(indexType.IsAssignableFrom(typeof(IVertex)) || indexType.IsAssignableFrom(typeof(IEdge)));

            if (isUserIndex)
                return indexType == typeof(IVertex)
                           ? Context.IndexingService.UserVertexIndices
                           : Context.IndexingService.UserEdgeIndices;

            return indexType == typeof(IVertex) ? Context.IndexingService.VertexIndices : Context.IndexingService.EdgeIndices;
        }

        public void UpdateGeneration(long generation)
        {
            Context.Generation = generation;
        }

        public void WaitForGeneration()
        {
            Context.IndexingService.WaitForGeneration(Context.Generation);
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            var generation = GetIndices(elementClass, false).DropIndex(key);
            if (generation != -1)
                UpdateGeneration(generation);
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            var indices = GetIndices(elementClass, false);
            if (indices.HasIndex(key)) return;

            indices.CreateIndex(key);

            if (elementClass == typeof(IVertex))
                this.ReIndexElements(GetVertices(), new[] { key });
            else
                this.ReIndexElements(GetEdges(), new[] { key });
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            return GetIndices(elementClass, false).GetIndices();
        }

        protected virtual IEnumerable<IEdge> IterateEdges(string key, object value)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(key));

            var edgeIds = Context.IndexingService.EdgeIndices.Get(key, key, value, int.MaxValue);
            return edgeIds.Select(edgeId => GetEdge(edgeId)).Where(edge => edge != null);
        }

        public virtual IQuery Query()
        {
            WaitForGeneration();
            return new IndexQuery(this, Context.IndexingService);
        }

        public void SetIndexedKeyValue(IElement element, string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var type = element is IVertex ? typeof(IVertex) : typeof(IEdge);
            var indices = GetIndices(type, false);
            if (!indices.HasIndex(key)) return;

            var id = element.Id.TryToInt64();
            if (!id.HasValue) throw new InvalidOperationException();
            
            var generation = indices.Set(id.Value, key, key, value);
            UpdateGeneration(generation);
        }
    }
}
