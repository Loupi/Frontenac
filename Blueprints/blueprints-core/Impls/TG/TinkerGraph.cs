using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     An in-memory, reference implementation of the property graph interfaces provided by Blueprints.
    /// </summary>
    [Serializable]
    public class TinkerGraph : IIndexableGraph, IKeyIndexableGraph
    {
        public enum FileType
        {
            DotNet,
            Gml,
            Graphml,
            Graphson
        }

        private static readonly Features TinkerGraphFeatures = new Features();
        private static readonly Features PersistentFeatures;
        private readonly string _directory;
        private readonly FileType _fileType;

        internal long CurrentId = 0;
        internal TinkerKeyIndex EdgeKeyIndex;
        protected Dictionary<string, IEdge> Edges = new Dictionary<string, IEdge>();
        internal Dictionary<string, TinkerIndex> Indices = new Dictionary<string, TinkerIndex>();
        protected Dictionary<string, IVertex> InnerVertices = new Dictionary<string, IVertex>();

        internal TinkerKeyIndex VertexKeyIndex;

        static TinkerGraph()
        {
            TinkerGraphFeatures.SupportsDuplicateEdges = true;
            TinkerGraphFeatures.SupportsSelfLoops = true;
            TinkerGraphFeatures.SupportsSerializableObjectProperty = true;
            TinkerGraphFeatures.SupportsBooleanProperty = true;
            TinkerGraphFeatures.SupportsDoubleProperty = true;
            TinkerGraphFeatures.SupportsFloatProperty = true;
            TinkerGraphFeatures.SupportsIntegerProperty = true;
            TinkerGraphFeatures.SupportsPrimitiveArrayProperty = true;
            TinkerGraphFeatures.SupportsUniformListProperty = true;
            TinkerGraphFeatures.SupportsMixedListProperty = true;
            TinkerGraphFeatures.SupportsLongProperty = true;
            TinkerGraphFeatures.SupportsMapProperty = true;
            TinkerGraphFeatures.SupportsStringProperty = true;

            TinkerGraphFeatures.IgnoresSuppliedIds = false;
            TinkerGraphFeatures.IsPersistent = false;
            TinkerGraphFeatures.IsRdfModel = false;
            TinkerGraphFeatures.IsWrapper = false;

            TinkerGraphFeatures.SupportsIndices = true;
            TinkerGraphFeatures.SupportsKeyIndices = true;
            TinkerGraphFeatures.SupportsVertexKeyIndex = true;
            TinkerGraphFeatures.SupportsEdgeKeyIndex = true;
            TinkerGraphFeatures.SupportsVertexIndex = true;
            TinkerGraphFeatures.SupportsEdgeIndex = true;
            TinkerGraphFeatures.SupportsTransactions = false;
            TinkerGraphFeatures.SupportsVertexIteration = true;
            TinkerGraphFeatures.SupportsEdgeIteration = true;
            TinkerGraphFeatures.SupportsEdgeRetrieval = true;
            TinkerGraphFeatures.SupportsVertexProperties = true;
            TinkerGraphFeatures.SupportsEdgeProperties = true;
            TinkerGraphFeatures.SupportsThreadedTransactions = false;
            TinkerGraphFeatures.SupportsIdProperty = false;
            TinkerGraphFeatures.SupportsLabelProperty = false;

            PersistentFeatures = TinkerGraphFeatures.CopyFeatures();
            PersistentFeatures.IsPersistent = true;
        }

        public TinkerGraph(string directory, FileType fileType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(directory));

            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);

            _directory = directory;
            _fileType = fileType;

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            else
            {
                var tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(fileType);
                var graph = tinkerStorage.Load(directory);

                InnerVertices = graph.InnerVertices;
                Edges = graph.Edges;
                CurrentId = graph.CurrentId;
                Indices = graph.Indices;
                VertexKeyIndex = graph.VertexKeyIndex;
                EdgeKeyIndex = graph.EdgeKeyIndex;
            }
        }

        public TinkerGraph(string directory)
            : this(directory, FileType.DotNet)
        {
        }

        public TinkerGraph()
        {
            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);
            _directory = null;
            _fileType = FileType.DotNet;
        }

        public virtual IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return VertexKeyIndex.GetIndexedKeys().Contains(key)
                       ? VertexKeyIndex.Get(key, value).Cast<IVertex>()
                       : new PropertyFilteredIterable<IVertex>(key, value, GetVertices());
        }

        public virtual IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (EdgeKeyIndex.GetIndexedKeys().Contains(key))
                return EdgeKeyIndex.Get(key, value).Cast<IEdge>();
            return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (Indices.ContainsKey(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            var index = new TinkerIndex(indexName, indexClass);
            Indices.Put(index.Name, index);
            return index;
        }

        public virtual IIndex GetIndex(string indexName, Type indexClass)
        {
            var index = Indices.Get(indexName);
            if (null == index)
                return null;
            if (!indexClass.IsAssignableFrom(index.Type))
                throw ExceptionFactory.IndexDoesNotSupportClass(indexName, indexClass);
            return index;
        }

        public virtual IEnumerable<IIndex> GetIndices()
        {
            return Indices.Values.ToList();
        }

        public virtual void DropIndex(string indexName)
        {
            Indices.Remove(indexName);
        }

        public virtual IVertex AddVertex(object id)
        {
            string idString = null;
            IVertex vertex;
            if (null != id)
            {
                idString = id.ToString();
                vertex = InnerVertices.Get(idString);
                if (null != vertex)
                    throw ExceptionFactory.VertexWithIdAlreadyExists(id);
            }
            else
            {
                var done = false;
                while (!done)
                {
                    idString = GetNextId();
                    vertex = InnerVertices.Get(idString);
                    if (null == vertex)
                        done = true;
                }
            }

            vertex = new TinkerVertex(idString, this);
            InnerVertices.Put(vertex.Id.ToString(), vertex);
            return vertex;
        }

        public virtual IVertex GetVertex(object id)
        {
            var idString = id.ToString();
            return InnerVertices.Get(idString);
        }

        public virtual IEdge GetEdge(object id)
        {
            var idString = id.ToString();
            return Edges.Get(idString);
        }

        public virtual IEnumerable<IVertex> GetVertices()
        {
            return new List<IVertex>(InnerVertices.Values);
        }

        public virtual IEnumerable<IEdge> GetEdges()
        {
            return new List<IEdge>(Edges.Values);
        }

        public virtual void RemoveVertex(IVertex vertex)
        {
            foreach (var edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            VertexKeyIndex.RemoveElement(vertex);
            foreach (var idx in GetIndices().Where(t => t.Type == typeof (IVertex)).Cast<TinkerIndex>())
            {
                idx.RemoveElement(vertex);
            }

            InnerVertices.Remove(vertex.Id.ToString());
        }

        public virtual IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            string idString = null;
            IEdge edge;
            if (null != id)
            {
                idString = id.ToString();
                edge = Edges.Get(idString);
                if (null != edge)
                {
                    throw ExceptionFactory.EdgeWithIdAlreadyExist(id);
                }
            }
            else
            {
                var done = false;
                while (!done)
                {
                    idString = GetNextId();
                    edge = Edges.Get(idString);
                    if (null == edge)
                        done = true;
                }
            }

            edge = new TinkerEdge(idString, outVertex, inVertex, label, this);
            Edges.Put(edge.Id.ToString(), edge);
            var out_ = (TinkerVertex) outVertex;
            var in_ = (TinkerVertex) inVertex;
            out_.AddOutEdge(label, edge);
            in_.AddInEdge(label, edge);
            return edge;
        }

        public virtual void RemoveEdge(IEdge edge)
        {
            var outVertex = (TinkerVertex) edge.GetVertex(Direction.Out);
            var inVertex = (TinkerVertex) edge.GetVertex(Direction.In);
            if (null != outVertex && null != outVertex.OutEdges)
            {
                var e = outVertex.OutEdges.Get(edge.Label);
                if (null != e)
                    e.Remove(edge);
            }
            if (null != inVertex && null != inVertex.InEdges)
            {
                var e = inVertex.InEdges.Get(edge.Label);
                if (null != e)
                    e.Remove(edge);
            }


            EdgeKeyIndex.RemoveElement(edge);
            foreach (var idx in GetIndices().Where(t => t.Type == typeof (IEdge)).Cast<TinkerIndex>())
            {
                idx.RemoveElement(edge);
            }

            Edges.Remove(edge.Id.ToString());
        }

        public virtual IQuery Query()
        {
            return new DefaultGraphQuery(this);
        }

        public void Shutdown()
        {
            if (null == _directory) return;
            var tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(_fileType);
            tinkerStorage.Save(this, _directory);
        }

        public virtual Features Features
        {
            get { return null == _directory ? TinkerGraphFeatures : PersistentFeatures; }
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (typeof (IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.CreateKeyIndex(key);
            else
                EdgeKeyIndex.CreateKeyIndex(key);
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            if (typeof (IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.DropKeyIndex(key);
            else
                EdgeKeyIndex.DropKeyIndex(key);
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            return typeof (IVertex).IsAssignableFrom(elementClass)
                       ? VertexKeyIndex.GetIndexedKeys()
                       : EdgeKeyIndex.GetIndexedKeys();
        }

        public override string ToString()
        {
            if (null == _directory)
                return this.GraphString(string.Concat("vertices:",
                                               InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture),
                                               " edges:",
                                               Edges.LongCount().ToString(CultureInfo.InvariantCulture)));

            return this.GraphString(string.Concat("vertices:",
                                           InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture),
                                           " edges:",
                                           Edges.LongCount().ToString(CultureInfo.InvariantCulture),
                                           " directory:", _directory));
        }

        public void Clear()
        {
            InnerVertices.Clear();
            Edges.Clear();
            Indices.Clear();
            CurrentId = 0;
            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);
        }

        private string GetNextId()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            string idString;
            while (true)
            {
                idString = CurrentId.ToString(CultureInfo.InvariantCulture);
                CurrentId++;
                if (null == InnerVertices.Get(idString) || null == Edges.Get(idString) || CurrentId == long.MaxValue)
                    break;
            }
            return idString;
        }

        [Serializable]
        internal class TinkerKeyIndex : TinkerIndex
        {
            private readonly TinkerGraph _graph;
            private readonly HashSet<string> _indexedKeys = new HashSet<string>();

            public TinkerKeyIndex(Type indexClass, TinkerGraph graph)
                : base(null, indexClass)
            {
                Contract.Requires(graph != null);

                _graph = graph;
            }

            public void AutoUpdate(string key, object newValue, object oldValue, TinkerElement element)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(key));
                Contract.Requires(element != null);

                if (!_indexedKeys.Contains(key)) return;
                if (oldValue != null)
                    Remove(key, oldValue, element);
                Put(key, newValue, element);
            }

            public void AutoRemove(string key, object oldValue, TinkerElement element)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(key));
                Contract.Requires(element != null);

                if (_indexedKeys.Contains(key))
                    Remove(key, oldValue, element);
            }

            public void CreateKeyIndex(string key)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(key));

                if (_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Add(key);

                if (typeof (TinkerVertex) == IndexClass)
                    _graph.ReIndexElements(_graph.GetVertices(), new HashSet<string>(new[] {key}));
                else
                    _graph.ReIndexElements(_graph.GetEdges(), new HashSet<string>(new[] {key}));
            }

            public void DropKeyIndex(string key)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(key));

                if (!_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Remove(key);
                Index.Remove(key);
            }

            public IEnumerable<string> GetIndexedKeys()
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

                return null != _indexedKeys ? new HashSet<string>(_indexedKeys) : Enumerable.Empty<string>();
            }
        }
    }
}