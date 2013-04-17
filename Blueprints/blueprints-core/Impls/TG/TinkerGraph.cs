using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// An in-memory, reference implementation of the property graph interfaces provided by Blueprints.
    /// </summary>
    [Serializable]
    public class TinkerGraph : IndexableGraph, KeyIndexableGraph
    {
        internal long _CurrentId = 0;
        protected Dictionary<string, Vertex> _Vertices = new Dictionary<string, Vertex>();
        protected Dictionary<string, Edge> _Edges = new Dictionary<string, Edge>();
        internal Dictionary<string, TinkerIndex> _Indices = new Dictionary<string, TinkerIndex>();

        internal TinkerKeyIndex _VertexKeyIndex;
        internal TinkerKeyIndex _EdgeKeyIndex;

        readonly string _Directory;
        readonly FileType _FileType;

        static readonly Features FEATURES = new Features();
        static readonly Features PERSISTENT_FEATURES;

        public enum FileType
        {
            JAVA,
            GML,
            GRAPHML,
            GRAPHSON
        }

        static TinkerGraph()
        {
            FEATURES.SupportsDuplicateEdges = true;
            FEATURES.SupportsSelfLoops = true;
            FEATURES.SupportsSerializableObjectProperty = true;
            FEATURES.SupportsBooleanProperty = true;
            FEATURES.SupportsDoubleProperty = true;
            FEATURES.SupportsFloatProperty = true;
            FEATURES.SupportsIntegerProperty = true;
            FEATURES.SupportsPrimitiveArrayProperty = true;
            FEATURES.SupportsUniformListProperty = true;
            FEATURES.SupportsMixedListProperty = true;
            FEATURES.SupportsLongProperty = true;
            FEATURES.SupportsMapProperty = true;
            FEATURES.SupportsStringProperty = true;

            FEATURES.IgnoresSuppliedIds = false;
            FEATURES.IsPersistent = false;
            FEATURES.IsRDFModel = false;
            FEATURES.IsWrapper = false;

            FEATURES.SupportsIndices = true;
            FEATURES.SupportsKeyIndices = true;
            FEATURES.SupportsVertexKeyIndex = true;
            FEATURES.SupportsEdgeKeyIndex = true;
            FEATURES.SupportsVertexIndex = true;
            FEATURES.SupportsEdgeIndex = true;
            FEATURES.SupportsTransactions = false;
            FEATURES.SupportsVertexIteration = true;
            FEATURES.SupportsEdgeIteration = true;
            FEATURES.SupportsEdgeRetrieval = true;
            FEATURES.SupportsVertexProperties = true;
            FEATURES.SupportsEdgeProperties = true;
            FEATURES.SupportsThreadedTransactions = false;

            PERSISTENT_FEATURES = FEATURES.CopyFeatures();
            PERSISTENT_FEATURES.IsPersistent = true;
        }

        public TinkerGraph(string directory, FileType fileType)
        {
            _VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            _EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);

            _Directory = directory;
            _FileType = fileType;

            string file = string.Concat(_Directory, "ted");

            if (!Directory.Exists(_Directory))
                Directory.CreateDirectory(_Directory);
            else
            {
                using (var input = File.OpenRead(file))
                {
                    TinkerStorage tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(fileType);
                    TinkerGraph graph = tinkerStorage.Load(directory);

                    _Vertices = graph._Vertices;
                    _Edges = graph._Edges;
                    _CurrentId = graph._CurrentId;
                    _Indices = graph._Indices;
                    _VertexKeyIndex = graph._VertexKeyIndex;
                    _EdgeKeyIndex = graph._EdgeKeyIndex;
                }
            }
        }

        public TinkerGraph(string directory)
            : this(directory, FileType.JAVA)
        {

        }

        public TinkerGraph()
        {
            _VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            _EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
            _Directory = null;
            _FileType = FileType.JAVA;
        }

        public virtual IEnumerable<Vertex> GetVertices(string key, object value)
        {
            if (_VertexKeyIndex.GetIndexedKeys().Contains(key))
                return _VertexKeyIndex.Get(key, value).Cast<Vertex>();
            else
                return new PropertyFilteredIterable<Vertex>(key, value, this.GetVertices());
        }

        public virtual IEnumerable<Edge> GetEdges(string key, object value)
        {
            if (_EdgeKeyIndex.GetIndexedKeys().Contains(key))
                return _EdgeKeyIndex.Get(key, value).Cast<Edge>();
            else
                return new PropertyFilteredIterable<Edge>(key, value, this.GetEdges());
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                _VertexKeyIndex.CreateKeyIndex(key);
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                _EdgeKeyIndex.CreateKeyIndex(key);
            else
                throw ExceptionFactory.ClassIsNotIndexable(elementClass);
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                _VertexKeyIndex.DropKeyIndex(key);
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                _EdgeKeyIndex.DropKeyIndex(key);
            else
                throw ExceptionFactory.ClassIsNotIndexable(elementClass);
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                return _VertexKeyIndex.GetIndexedKeys();
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                return _EdgeKeyIndex.GetIndexedKeys();
            else
                throw ExceptionFactory.ClassIsNotIndexable(elementClass);
        }

        public virtual Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (_Indices.ContainsKey(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            TinkerIndex index = new TinkerIndex(indexName, indexClass);
            _Indices.Put(index.GetIndexName(), index);
            return index;
        }

        public virtual Index GetIndex(string indexName, Type indexClass)
        {
            Index index = _Indices.Get(indexName);
            if (null == index)
                return null;
            if (!indexClass.IsAssignableFrom(index.GetIndexClass()))
                throw ExceptionFactory.IndexDoesNotSupportClass(indexName, indexClass);
            else
                return index;
        }

        public virtual IEnumerable<Index> GetIndices()
        {
            return _Indices.Values.ToList();
        }

        public virtual void DropIndex(string indexName)
        {
            _Indices.Remove(indexName);
        }

        public virtual Vertex AddVertex(object id)
        {
            string idString = null;
            Vertex vertex;
            if (null != id)
            {
                idString = id.ToString();
                vertex = _Vertices.Get(idString);
                if (null != vertex)
                    throw ExceptionFactory.VertexWithIdAlreadyExists(id);
            }
            else
            {
                bool done = false;
                while (!done)
                {
                    idString = this.GetNextId();
                    vertex = _Vertices.Get(idString);
                    if (null == vertex)
                        done = true;
                }
            }

            vertex = new TinkerVertex(idString, this);
            _Vertices.Put(vertex.GetId().ToString(), vertex);
            return vertex;
        }

        public virtual Vertex GetVertex(object id)
        {
            if (null == id)
                throw ExceptionFactory.VertexIdCanNotBeNull();

            string idString = id.ToString();
            return _Vertices.Get(idString);
        }

        public virtual Edge GetEdge(object id)
        {
            if (null == id)
                throw ExceptionFactory.EdgeIdCanNotBeNull();

            string idString = id.ToString();
            return _Edges.Get(idString);
        }

        public virtual IEnumerable<Vertex> GetVertices()
        {
            return new List<Vertex>(_Vertices.Values);
        }

        public virtual IEnumerable<Edge> GetEdges()
        {
            return new List<Edge>(_Edges.Values);
        }

        public virtual void RemoveVertex(Vertex vertex)
        {
            foreach (Edge edge in vertex.GetEdges(Direction.BOTH))
                this.RemoveEdge(edge);

            _VertexKeyIndex.RemoveElement((TinkerVertex)vertex);
            foreach (var index in this.GetIndices().Where(t => t.GetIndexClass() == typeof(Vertex)))
            {
                TinkerIndex idx = (TinkerIndex)index;
                idx.RemoveElement(vertex);
            }

            _Vertices.Remove(vertex.GetId().ToString());
        }

        public virtual Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            string idString = null;
            Edge edge;
            if (null != id)
            {
                idString = id.ToString();
                edge = _Edges.Get(idString);
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
                    idString = this.GetNextId();
                    edge = _Edges.Get(idString);
                    if (null == edge)
                        done = true;
                }
            }

            edge = new TinkerEdge(idString, outVertex, inVertex, label, this);
            _Edges.Put(edge.GetId().ToString(), edge);
            TinkerVertex out_ = (TinkerVertex)outVertex;
            TinkerVertex in_ = (TinkerVertex)inVertex;
            out_.AddOutEdge(label, edge);
            in_.AddInEdge(label, edge);
            return edge;
        }

        public virtual void RemoveEdge(Edge edge)
        {
            TinkerVertex outVertex = (TinkerVertex)edge.GetVertex(Direction.OUT);
            TinkerVertex inVertex = (TinkerVertex)edge.GetVertex(Direction.IN);
            if (null != outVertex && null != outVertex._OutEdges)
            {
                HashSet<Edge> edges = outVertex._OutEdges.Get(edge.GetLabel());
                if (null != edges)
                    edges.Remove(edge);
            }
            if (null != inVertex && null != inVertex._InEdges)
            {
                HashSet<Edge> edges = inVertex._InEdges.Get(edge.GetLabel());
                if (null != edges)
                    edges.Remove(edge);
            }


            _EdgeKeyIndex.RemoveElement((TinkerEdge)edge);
            foreach (var index in this.GetIndices().Where(t => t.GetIndexClass() == typeof(Edge)))
            {
                TinkerIndex idx = (TinkerIndex)index;
                idx.RemoveElement(edge);
            }

            _Edges.Remove(edge.GetId().ToString());
        }

        public virtual GraphQuery Query()
        {
            return new DefaultGraphQuery(this);
        }

        public override string ToString()
        {
            if (null == _Directory)
                return StringFactory.GraphString(this, string.Concat("vertices:", _Vertices.LongCount().ToString(), " edges:", _Edges.LongCount().ToString()));
            else
                return StringFactory.GraphString(this, string.Concat("vertices:", _Vertices.LongCount().ToString(), " edges:", _Edges.LongCount().ToString(), " directory:", _Directory));
        }

        public void Clear()
        {
            _Vertices.Clear();
            _Edges.Clear();
            _Indices.Clear();
            _CurrentId = 0;
            _VertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            _EdgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
        }

        public virtual void Shutdown()
        {
            if (null != _Directory)
            {
                TinkerStorage tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(_FileType);
                tinkerStorage.Save(this, _Directory);
            }
        }

        string GetNextId()
        {
            string idString;
            while (true)
            {
                idString = _CurrentId.ToString();
                _CurrentId++;
                if (null == _Vertices.Get(idString) || null == _Edges.Get(idString) || _CurrentId == long.MaxValue)
                    break;
            }
            return idString;
        }

        public virtual Features GetFeatures()
        {
            if (null == _Directory)
                return FEATURES;
            else
                return PERSISTENT_FEATURES;
        }

        [Serializable]
        internal class TinkerKeyIndex : TinkerIndex
        {
            readonly HashSet<string> _IndexedKeys = new HashSet<string>();
            TinkerGraph _Graph;

            public TinkerKeyIndex(Type indexClass, TinkerGraph graph)
                : base(null, indexClass)
            {
                _Graph = graph;
            }

            public void AutoUpdate(string key, object newValue, object oldValue, TinkerElement element)
            {
                if (_IndexedKeys.Contains(key))
                {
                    if (oldValue != null)
                        this.Remove(key, oldValue, element);
                    this.Put(key, newValue, element);
                }
            }

            public void AutoRemove(string key, object oldValue, TinkerElement element)
            {
                if (_IndexedKeys.Contains(key))
                    this.Remove(key, oldValue, element);
            }

            public void CreateKeyIndex(string key)
            {
                if (_IndexedKeys.Contains(key))
                    return;

                _IndexedKeys.Add(key);

                if (typeof(TinkerVertex) == _IndexClass)
                    KeyIndexableGraphHelper.ReIndexElements(_Graph, _Graph.GetVertices(), new HashSet<string>(new string[] { key }));
                else
                    KeyIndexableGraphHelper.ReIndexElements(_Graph, _Graph.GetEdges(), new HashSet<string>(new string[] { key }));
            }

            public void DropKeyIndex(string key)
            {
                if (!_IndexedKeys.Contains(key))
                    return;

                _IndexedKeys.Remove(key);
                _Index.Remove(key);
            }

            public IEnumerable<string> GetIndexedKeys()
            {
                if (null != _IndexedKeys)
                    return new HashSet<string>(_IndexedKeys);
                else
                    return Enumerable.Empty<string>();
            }
        }
    }
}
