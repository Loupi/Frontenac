using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// An in-memory, reference implementation of the property graph interfaces provided by Blueprints.
    /// </summary>
    [Serializable]
    public class TinkerGraph : IIndexableGraph, IKeyIndexableGraph
    {
        internal long CurrentId = 0;
        protected Dictionary<string, IVertex> InnerVertices = new Dictionary<string, IVertex>();
        protected Dictionary<string, IEdge> Edges = new Dictionary<string, IEdge>();
        internal Dictionary<string, TinkerIndex> Indices = new Dictionary<string, TinkerIndex>();

        internal TinkerKeyIndex VertexKeyIndex;
        internal TinkerKeyIndex EdgeKeyIndex;

        readonly string _directory;
        readonly FileType _fileType;

        static readonly Features TinkerGraphFeatures = new Features();
        static readonly Features PersistentFeatures;

        public enum FileType
        {
            Java,
            Gml,
            Graphml,
            Graphson
        }

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
            VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);

            _directory = directory;
            _fileType = fileType;

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            else
            {
                ITinkerStorage tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(fileType);
                TinkerGraph graph = tinkerStorage.Load(directory);

                InnerVertices = graph.InnerVertices;
                Edges = graph.Edges;
                CurrentId = graph.CurrentId;
                Indices = graph.Indices;
                VertexKeyIndex = graph.VertexKeyIndex;
                EdgeKeyIndex = graph.EdgeKeyIndex;
            }
        }

        public TinkerGraph(string directory)
            : this(directory, FileType.Java)
        {

        }

        public TinkerGraph()
        {
            VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
            _directory = null;
            _fileType = FileType.Java;
        }

        public virtual IEnumerable<IVertex> GetVertices(string key, object value)
        {
            if (VertexKeyIndex.GetIndexedKeys().Contains(key))
                return VertexKeyIndex.Get(key, value).Cast<IVertex>();
            return new PropertyFilteredIterable<IVertex>(key, value, GetVertices());
        }

        public virtual IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (EdgeKeyIndex.GetIndexedKeys().Contains(key))
                return EdgeKeyIndex.Get(key, value).Cast<IEdge>();
            return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (typeof(IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.CreateKeyIndex(key);
            else if (typeof(IEdge).IsAssignableFrom(elementClass))
                EdgeKeyIndex.CreateKeyIndex(key);
            else
                throw ExceptionFactory.ClassIsNotIndexable(elementClass);
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            if (typeof(IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.DropKeyIndex(key);
            else if (typeof(IEdge).IsAssignableFrom(elementClass))
                EdgeKeyIndex.DropKeyIndex(key);
            else
                throw ExceptionFactory.ClassIsNotIndexable(elementClass);
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            if (typeof(IVertex).IsAssignableFrom(elementClass))
                return VertexKeyIndex.GetIndexedKeys();
            if (typeof(IEdge).IsAssignableFrom(elementClass))
                return EdgeKeyIndex.GetIndexedKeys();
            throw ExceptionFactory.ClassIsNotIndexable(elementClass);
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
            IIndex index = Indices.Get(indexName);
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
                bool done = false;
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
            if (null == id)
                throw ExceptionFactory.VertexIdCanNotBeNull();

            string idString = id.ToString();
            return InnerVertices.Get(idString);
        }

        public virtual IEdge GetEdge(object id)
        {
            if (null == id)
                throw ExceptionFactory.EdgeIdCanNotBeNull();

            string idString = id.ToString();
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
            foreach (IEdge edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            VertexKeyIndex.RemoveElement(vertex);
            foreach (var index in GetIndices().Where(t => t.Type == typeof(IVertex)))
            {
                var idx = (TinkerIndex)index;
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
                bool done = false;
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
            var out_ = (TinkerVertex)outVertex;
            var in_ = (TinkerVertex)inVertex;
            out_.AddOutEdge(label, edge);
            in_.AddInEdge(label, edge);
            return edge;
        }

        public virtual void RemoveEdge(IEdge edge)
        {
            var outVertex = (TinkerVertex)edge.GetVertex(Direction.Out);
            var inVertex = (TinkerVertex)edge.GetVertex(Direction.In);
            if (null != outVertex && null != outVertex.OutEdges)
            {
                HashSet<IEdge> e = outVertex.OutEdges.Get(edge.Label);
                if (null != e)
                    e.Remove(edge);
            }
            if (null != inVertex && null != inVertex.InEdges)
            {
                HashSet<IEdge> e = inVertex.InEdges.Get(edge.Label);
                if (null != e)
                    e.Remove(edge);
            }


            EdgeKeyIndex.RemoveElement(edge);
            foreach (var index in GetIndices().Where(t => t.Type == typeof(IEdge)))
            {
                var idx = (TinkerIndex)index;
                idx.RemoveElement(edge);
            }

            Edges.Remove(edge.Id.ToString());
        }

        public virtual IGraphQuery Query()
        {
            return new DefaultGraphQuery(this);
        }

        public override string ToString()
        {
            if (null == _directory)
                return StringFactory.GraphString(this, string.Concat("vertices:", InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture), " edges:", Edges.LongCount().ToString(CultureInfo.InvariantCulture)));
            return StringFactory.GraphString(this, string.Concat("vertices:", InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture), " edges:", Edges.LongCount().ToString(CultureInfo.InvariantCulture), " directory:", _directory));
        }

        public void Clear()
        {
            InnerVertices.Clear();
            Edges.Clear();
            Indices.Clear();
            CurrentId = 0;
            VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
        }

        public void Shutdown()
        {
            if (null != _directory)
            {
                ITinkerStorage tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(_fileType);
                tinkerStorage.Save(this, _directory);
            }
        }

        string GetNextId()
        {
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

        public virtual Features Features
        {
            get
            {
                return null == _directory ? TinkerGraphFeatures : PersistentFeatures;
            }
        }

        [Serializable]
        internal class TinkerKeyIndex : TinkerIndex
        {
            readonly HashSet<string> _indexedKeys = new HashSet<string>();
            readonly TinkerGraph _graph;

            public TinkerKeyIndex(Type indexClass, TinkerGraph graph)
                : base(null, indexClass)
            {
                _graph = graph;
            }

            public void AutoUpdate(string key, object newValue, object oldValue, TinkerElement element)
            {
                if (_indexedKeys.Contains(key))
                {
                    if (oldValue != null)
                        Remove(key, oldValue, element);
                    Put(key, newValue, element);
                }
            }

            public void AutoRemove(string key, object oldValue, TinkerElement element)
            {
                if (_indexedKeys.Contains(key))
                    Remove(key, oldValue, element);
            }

            public void CreateKeyIndex(string key)
            {
                if (_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Add(key);

                if (typeof(TinkerVertex) == IndexClass)
                    KeyIndexableGraphHelper.ReIndexElements(_graph, _graph.GetVertices(), new HashSet<string>(new[] { key }));
                else
                    KeyIndexableGraphHelper.ReIndexElements(_graph, _graph.GetEdges(), new HashSet<string>(new[] { key }));
            }

            public void DropKeyIndex(string key)
            {
                if (!_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Remove(key);
                Index.Remove(key);
            }

            public IEnumerable<string> GetIndexedKeys()
            {
                if (null != _indexedKeys)
                    return new HashSet<string>(_indexedKeys);
                return Enumerable.Empty<string>();
            }
        }
    }
}
