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
        internal long currentId = 0;
        protected Dictionary<string, Vertex> vertices = new Dictionary<string, Vertex>();
        protected Dictionary<string, Edge> edges = new Dictionary<string, Edge>();
        internal Dictionary<string, TinkerIndex> indices = new Dictionary<string, TinkerIndex>();

        internal TinkerKeyIndex vertexKeyIndex;
        internal TinkerKeyIndex edgeKeyIndex;

        readonly string _directory;
        readonly FileType _fileType;

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
            FEATURES.supportsDuplicateEdges = true;
            FEATURES.supportsSelfLoops = true;
            FEATURES.supportsSerializableObjectProperty = true;
            FEATURES.supportsBooleanProperty = true;
            FEATURES.supportsDoubleProperty = true;
            FEATURES.supportsFloatProperty = true;
            FEATURES.supportsIntegerProperty = true;
            FEATURES.supportsPrimitiveArrayProperty = true;
            FEATURES.supportsUniformListProperty = true;
            FEATURES.supportsMixedListProperty = true;
            FEATURES.supportsLongProperty = true;
            FEATURES.supportsMapProperty = true;
            FEATURES.supportsStringProperty = true;

            FEATURES.ignoresSuppliedIds = false;
            FEATURES.isPersistent = false;
            FEATURES.isRDFModel = false;
            FEATURES.isWrapper = false;

            FEATURES.supportsIndices = true;
            FEATURES.supportsKeyIndices = true;
            FEATURES.supportsVertexKeyIndex = true;
            FEATURES.supportsEdgeKeyIndex = true;
            FEATURES.supportsVertexIndex = true;
            FEATURES.supportsEdgeIndex = true;
            FEATURES.supportsTransactions = false;
            FEATURES.supportsVertexIteration = true;
            FEATURES.supportsEdgeIteration = true;
            FEATURES.supportsEdgeRetrieval = true;
            FEATURES.supportsVertexProperties = true;
            FEATURES.supportsEdgeProperties = true;
            FEATURES.supportsThreadedTransactions = false;

            PERSISTENT_FEATURES = FEATURES.copyFeatures();
            PERSISTENT_FEATURES.isPersistent = true;
        }

        public TinkerGraph(string directory, FileType fileType)
        {
            vertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            edgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);

            _directory = directory;
            _fileType = fileType;

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            else
            {
                TinkerStorage tinkerStorage = TinkerStorageFactory.getInstance().getTinkerStorage(fileType);
                TinkerGraph graph = tinkerStorage.load(directory);

                vertices = graph.vertices;
                edges = graph.edges;
                currentId = graph.currentId;
                indices = graph.indices;
                vertexKeyIndex = graph.vertexKeyIndex;
                edgeKeyIndex = graph.edgeKeyIndex;
            }
        }

        public TinkerGraph(string directory)
            : this(directory, FileType.JAVA)
        {

        }

        public TinkerGraph()
        {
            vertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            edgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
            _directory = null;
            _fileType = FileType.JAVA;
        }

        public virtual IEnumerable<Vertex> getVertices(string key, object value)
        {
            if (vertexKeyIndex.getIndexedKeys().Contains(key))
                return vertexKeyIndex.get(key, value).Cast<Vertex>();
            else
                return new PropertyFilteredIterable<Vertex>(key, value, this.getVertices());
        }

        public virtual IEnumerable<Edge> getEdges(string key, object value)
        {
            if (edgeKeyIndex.getIndexedKeys().Contains(key))
                return edgeKeyIndex.get(key, value).Cast<Edge>();
            else
                return new PropertyFilteredIterable<Edge>(key, value, this.getEdges());
        }

        public virtual void createKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                vertexKeyIndex.createKeyIndex(key);
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                edgeKeyIndex.createKeyIndex(key);
            else
                throw ExceptionFactory.classIsNotIndexable(elementClass);
        }

        public virtual void dropKeyIndex(string key, Type elementClass)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                vertexKeyIndex.dropKeyIndex(key);
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                edgeKeyIndex.dropKeyIndex(key);
            else
                throw ExceptionFactory.classIsNotIndexable(elementClass);
        }

        public virtual IEnumerable<string> getIndexedKeys(Type elementClass)
        {
            if (typeof(Vertex).IsAssignableFrom(elementClass))
                return vertexKeyIndex.getIndexedKeys();
            else if (typeof(Edge).IsAssignableFrom(elementClass))
                return edgeKeyIndex.getIndexedKeys();
            else
                throw ExceptionFactory.classIsNotIndexable(elementClass);
        }

        public virtual Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (indices.ContainsKey(indexName))
                throw ExceptionFactory.indexAlreadyExists(indexName);

            TinkerIndex index = new TinkerIndex(indexName, indexClass);
            indices.put(index.getIndexName(), index);
            return index;
        }

        public virtual Index getIndex(string indexName, Type indexClass)
        {
            Index index = indices.get(indexName);
            if (null == index)
                return null;
            if (!indexClass.IsAssignableFrom(index.getIndexClass()))
                throw ExceptionFactory.indexDoesNotSupportClass(indexName, indexClass);
            else
                return index;
        }

        public virtual IEnumerable<Index> getIndices()
        {
            return indices.Values.ToList();
        }

        public virtual void dropIndex(string indexName)
        {
            indices.Remove(indexName);
        }

        public virtual Vertex addVertex(object id)
        {
            string idString = null;
            Vertex vertex;
            if (null != id)
            {
                idString = id.ToString();
                vertex = vertices.get(idString);
                if (null != vertex)
                    throw ExceptionFactory.vertexWithIdAlreadyExists(id);
            }
            else
            {
                bool done = false;
                while (!done)
                {
                    idString = this.getNextId();
                    vertex = vertices.get(idString);
                    if (null == vertex)
                        done = true;
                }
            }

            vertex = new TinkerVertex(idString, this);
            vertices.put(vertex.getId().ToString(), vertex);
            return vertex;
        }

        public virtual Vertex getVertex(object id)
        {
            if (null == id)
                throw ExceptionFactory.vertexIdCanNotBeNull();

            string idString = id.ToString();
            return vertices.get(idString);
        }

        public virtual Edge getEdge(object id)
        {
            if (null == id)
                throw ExceptionFactory.edgeIdCanNotBeNull();

            string idString = id.ToString();
            return edges.get(idString);
        }

        public virtual IEnumerable<Vertex> getVertices()
        {
            return new List<Vertex>(vertices.Values);
        }

        public virtual IEnumerable<Edge> getEdges()
        {
            return new List<Edge>(edges.Values);
        }

        public virtual void removeVertex(Vertex vertex)
        {
            foreach (Edge edge in vertex.getEdges(Direction.BOTH))
                this.removeEdge(edge);

            vertexKeyIndex.removeElement((TinkerVertex)vertex);
            foreach (var index in this.getIndices().Where(t => t.getIndexClass() == typeof(Vertex)))
            {
                TinkerIndex idx = (TinkerIndex)index;
                idx.removeElement(vertex);
            }

            vertices.Remove(vertex.getId().ToString());
        }

        public virtual Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            string idString = null;
            Edge edge;
            if (null != id)
            {
                idString = id.ToString();
                edge = edges.get(idString);
                if (null != edge)
                {
                    throw ExceptionFactory.edgeWithIdAlreadyExist(id);
                }
            }
            else
            {
                bool done = false;
                while (!done)
                {
                    idString = this.getNextId();
                    edge = edges.get(idString);
                    if (null == edge)
                        done = true;
                }
            }

            edge = new TinkerEdge(idString, outVertex, inVertex, label, this);
            edges.put(edge.getId().ToString(), edge);
            TinkerVertex out_ = (TinkerVertex)outVertex;
            TinkerVertex in_ = (TinkerVertex)inVertex;
            out_.addOutEdge(label, edge);
            in_.addInEdge(label, edge);
            return edge;
        }

        public virtual void removeEdge(Edge edge)
        {
            TinkerVertex outVertex = (TinkerVertex)edge.getVertex(Direction.OUT);
            TinkerVertex inVertex = (TinkerVertex)edge.getVertex(Direction.IN);
            if (null != outVertex && null != outVertex.outEdges)
            {
                HashSet<Edge> e = outVertex.outEdges.get(edge.getLabel());
                if (null != e)
                    e.Remove(edge);
            }
            if (null != inVertex && null != inVertex.inEdges)
            {
                HashSet<Edge> e = inVertex.inEdges.get(edge.getLabel());
                if (null != e)
                    e.Remove(edge);
            }


            edgeKeyIndex.removeElement((TinkerEdge)edge);
            foreach (var index in this.getIndices().Where(t => t.getIndexClass() == typeof(Edge)))
            {
                TinkerIndex idx = (TinkerIndex)index;
                idx.removeElement(edge);
            }

            edges.Remove(edge.getId().ToString());
        }

        public virtual GraphQuery query()
        {
            return new DefaultGraphQuery(this);
        }

        public override string ToString()
        {
            if (null == _directory)
                return StringFactory.graphString(this, string.Concat("vertices:", vertices.LongCount().ToString(), " edges:", edges.LongCount().ToString()));
            else
                return StringFactory.graphString(this, string.Concat("vertices:", vertices.LongCount().ToString(), " edges:", edges.LongCount().ToString(), " directory:", _directory));
        }

        public void clear()
        {
            vertices.Clear();
            edges.Clear();
            indices.Clear();
            currentId = 0;
            vertexKeyIndex = new TinkerKeyIndex(typeof(TinkerVertex), this);
            edgeKeyIndex = new TinkerKeyIndex(typeof(TinkerEdge), this);
        }

        public virtual void shutdown()
        {
            if (null != _directory)
            {
                TinkerStorage tinkerStorage = TinkerStorageFactory.getInstance().getTinkerStorage(_fileType);
                tinkerStorage.save(this, _directory);
            }
        }

        string getNextId()
        {
            string idString;
            while (true)
            {
                idString = currentId.ToString();
                currentId++;
                if (null == vertices.get(idString) || null == edges.get(idString) || currentId == long.MaxValue)
                    break;
            }
            return idString;
        }

        public virtual Features getFeatures()
        {
            if (null == _directory)
                return FEATURES;
            else
                return PERSISTENT_FEATURES;
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

            public void autoUpdate(string key, object newValue, object oldValue, TinkerElement element)
            {
                if (_indexedKeys.Contains(key))
                {
                    if (oldValue != null)
                        this.remove(key, oldValue, element);
                    this.put(key, newValue, element);
                }
            }

            public void autoRemove(string key, object oldValue, TinkerElement element)
            {
                if (_indexedKeys.Contains(key))
                    this.remove(key, oldValue, element);
            }

            public void createKeyIndex(string key)
            {
                if (_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Add(key);

                if (typeof(TinkerVertex) == indexClass)
                    KeyIndexableGraphHelper.reIndexElements(_graph, _graph.getVertices(), new HashSet<string>(new string[] { key }));
                else
                    KeyIndexableGraphHelper.reIndexElements(_graph, _graph.getEdges(), new HashSet<string>(new string[] { key }));
            }

            public void dropKeyIndex(string key)
            {
                if (!_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Remove(key);
                index.Remove(key);
            }

            public IEnumerable<string> getIndexedKeys()
            {
                if (null != _indexedKeys)
                    return new HashSet<string>(_indexedKeys);
                else
                    return Enumerable.Empty<string>();
            }
        }
    }
}
