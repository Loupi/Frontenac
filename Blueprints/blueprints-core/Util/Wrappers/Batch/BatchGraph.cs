using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Batch.Cache;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    /// BatchGraph is a wrapper that enables batch loading of a large number of edges and vertices by chunking the entire
    /// load into smaller batches and maintaining a memory-efficient vertex cache so that the entire transactional state can
    /// be flushed after each chunk is loaded.
    /// <br />
    /// BatchGraph is ONLY meant for loading data and does not support any retrieval or removal operations.
    /// That is, BatchGraph only supports the following methods:
    /// - addVertex for adding vertices
    /// - addEdge for adding edges
    /// - getVertex to be used when adding edges
    /// - Property getter, setter and removal methods for vertices and edges.
    /// <br />
    /// An important limitation of BatchGraph is that edge properties can only be set immediately after the edge has been added.
    /// If other vertices or edges have been created in the meantime, setting, getting or removing properties will throw
    /// exceptions. This is done to avoid caching of edges which would require a great amount of memory.
    /// <br />
    /// BatchGraph wraps TransactionalGraph. To wrap arbitrary graphs, use wrap which will additionally wrap non-transactional.
    /// <br />
    /// BatchGraph can also automatically set the provided element ids as properties on the respective element. Use
    /// setVertexIdKey and setEdgeIdKey to set the keys for the vertex and edge properties
    /// respectively. This allows to make the loaded baseGraph compatible for later wrapping with IdGraph.
    /// </summary>
    public class BatchGraph : TransactionalGraph, WrapperGraph
    {
        /// <summary>
        /// Default buffer size
        /// </summary>
        public const long DEFAULT_BUFFER_SIZE = 100000;

        readonly TransactionalGraph _baseGraph;
        string _vertexIdKey = null;
        string _edgeIdKey = null;
        bool _loadingFromScratch = true;
        readonly VertexCache _cache;
        readonly long _bufferSize = DEFAULT_BUFFER_SIZE;
        long _remainingBufferSize;
        BatchEdge _currentEdge = null;
        Edge _currentEdgeCached = null;
        object _previousOutVertexId = null;

        /// <summary>
        /// Constructs a BatchGraph wrapping the provided baseGraph, using the specified buffer size and expecting vertex ids of
        ///  the specified IdType. Supplying vertex ids which do not match this type will throw exceptions.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <param name="type"> Type of vertex id expected. This information is used to optimize the vertex cache memory footprint.</param>
        /// <param name="bufferSize">Defines the number of vertices and edges loaded before starting a new transaction. The larger this value, the more memory is required but the faster the loading process.</param>
        public BatchGraph(TransactionalGraph graph, VertexIDType type, long bufferSize)
        {
            if (graph == null) throw new ArgumentNullException("graph");
            if (bufferSize <= 0) throw new ArgumentException("BufferSize must be positive");

            _baseGraph = graph;
            _bufferSize = bufferSize;
            _vertexIdKey = null;
            _edgeIdKey = null;
            _cache = type.getVertexCache();
            _remainingBufferSize = _bufferSize;
        }

        /// <summary>
        /// Constructs a BatchGraph wrapping the provided baseGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        public BatchGraph(TransactionalGraph graph)
            : this(graph, VertexIDType.OBJECT, DEFAULT_BUFFER_SIZE)
        {
        }

        /// <summary>
        /// Constructs a BatchGraph wrapping the provided baseGraph. Immediately returns the baseGraph if its a BatchGraph
        /// and wraps non-transactional graphs in an additional WritethroughGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <returns>a BatchGraph wrapping the provided baseGraph</returns>
        public static BatchGraph wrap(Graph graph)
        {
            if (graph is BatchGraph) return (BatchGraph)graph;
            else if (graph is TransactionalGraph) return new BatchGraph((TransactionalGraph)graph);
            else return new BatchGraph(new WritethroughGraph(graph));
        }

        /// <summary>
        /// Constructs a BatchGraph wrapping the provided baseGraph. Immediately returns the baseGraph if its a BatchGraph
        /// and wraps non-transactional graphs in an additional WritethroughGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <param name="buffer">Size of the buffer</param>
        /// <returns>a BatchGraph wrapping the provided baseGraph</returns>
        public static BatchGraph wrap(Graph graph, long buffer)
        {
            if (graph is BatchGraph) return (BatchGraph)graph;
            else if (graph is TransactionalGraph)
                return new BatchGraph((TransactionalGraph)graph, VertexIDType.OBJECT, buffer);
            else return new BatchGraph(new WritethroughGraph(graph), VertexIDType.OBJECT, buffer);
        }

        /// <summary>
        /// Sets the key to be used when setting the vertex id as a property on the respective vertex.
        /// If the key is null, then no property will be set.
        /// If the loaded baseGraph should later be wrapped with IdGraph use IdGraph.ID.
        /// </summary>
        /// <param name="key">key Key to be used.</param>
        public void setVertexIdKey(string key)
        {
            if (!_loadingFromScratch && key == null && _baseGraph.getFeatures().ignoresSuppliedIds.Value)
                throw new InvalidOperationException("Cannot set vertex id key to null when not loading from scratch while ids are ignored.");
            _vertexIdKey = key;
        }

        /// <summary>
        /// Returns the key used to set the id on the vertices or null if such has not been set
        /// via setVertexIdKey
        /// </summary>
        /// <returns>The key used to set the id on the vertices or null if such has not been set</returns>
        public string getVertexIdKey()
        {
            return _vertexIdKey;
        }

        /// <summary>
        /// Sets the key to be used when setting the edge id as a property on the respective edge.
        /// If the key is null, then no property will be set.
        /// If the loaded baseGraph should later be wrapped with IdGraphuse IdGraph.ID.
        /// </summary>
        /// <param name="key">Key to be used.</param>
        public void setEdgeIdKey(string key)
        {
            _edgeIdKey = key;
        }

        /// <summary>
        /// Returns the key used to set the id on the edges or null if such has not been set
        /// via setEdgeIdKey
        /// </summary>
        /// <returns>The key used to set the id on the edges or null if such has not been set</returns>
        public string getEdgeIdKey()
        {
            return _edgeIdKey;
        }

        /// <summary>
        /// Sets whether the graph loaded through this instance of BatchGraph is loaded from scratch
        /// (i.e. the wrapped graph is initially empty) or whether graph is loaded incrementally into an
        /// existing graph.
        /// 
        /// In the former case, BatchGraph does not need to check for the existence of vertices with the wrapped
        /// graph but only needs to consult its own cache which can be significantly faster. In the latter case,
        /// the cache is checked first but an additional check against the wrapped graph may be necessary if
        /// the vertex does not exist.
        /// 
        /// By default, BatchGraph assumes that the data is loaded from scratch.
        /// 
        /// When setting loading from scratch to false, a vertex id key must be specified first using
        /// setVertexIdKey - otherwise an exception is thrown.
        /// </summary>
        /// <param name="fromScratch">Sets whether the graph loaded through this instance of BatchGraph is loaded from scratch</param>
        public void setLoadingFromScratch(bool fromScratch)
        {
            if (fromScratch == false && _vertexIdKey == null && _baseGraph.getFeatures().ignoresSuppliedIds.Value)
                throw new InvalidOperationException("Vertex id key is required to query existing vertices in wrapped graph.");
            _loadingFromScratch = fromScratch;
        }

        /// <summary>
        /// Whether this BatchGraph is loading data from scratch or incrementally into an existing graph.
        /// By default, this returns true.
        /// see setLoadingFromScratch
        /// </summary>
        /// <returns>Whether this BatchGraph is loading data from scratch or incrementally into an existing graph.</returns>
        public bool isLoadingFromScratch()
        {
            return _loadingFromScratch;
        }

        void nextElement()
        {
            _currentEdge = null;
            _currentEdgeCached = null;
            if (_remainingBufferSize <= 0)
            {
                _baseGraph.commit();
                _cache.newTransaction();
                _remainingBufferSize = _bufferSize;
            }
            _remainingBufferSize--;
        }

        /// <summary>
        /// Should only be invoked after loading is complete. Committing the transaction before will cause the loading to fail.
        /// </summary>
        public void commit()
        {
            _currentEdge = null;
            _currentEdgeCached = null;
            _remainingBufferSize = 0;
            _baseGraph.commit();
        }

        /// <summary>
        /// ot supported for batch loading, since data may have already been partially persisted.
        /// </summary>
        public void rollback()
        {
            throw new InvalidOperationException("Can not rollback during batch loading");
        }

        public void shutdown()
        {
            _baseGraph.commit();
            _baseGraph.shutdown();
            _currentEdge = null;
            _currentEdgeCached = null;
        }

        public Graph getBaseGraph()
        {
            return _baseGraph;
        }

        public Features getFeatures()
        {
            Features features = _baseGraph.getFeatures().copyFeatures();
            features.ignoresSuppliedIds = false;
            features.isWrapper = true;
            features.supportsEdgeIteration = false;
            features.supportsThreadedTransactions = false;
            features.supportsVertexIteration = false;
            return features;
        }

        Vertex retrieveFromCache(object externalID)
        {
            object internal_ = _cache.getEntry(externalID);
            if (internal_ is Vertex)
                return (Vertex)internal_;
            else if (internal_ != null)
            {
                //its an internal id
                Vertex v = _baseGraph.getVertex(internal_);
                _cache.set(v, externalID);
                return v;
            }
            else
                return null;
        }

        Vertex getCachedVertex(object externalID)
        {
            Vertex v = retrieveFromCache(externalID);
            if (v == null) throw new ArgumentException(string.Concat("Vertex for given ID cannot be found: ", externalID));
            return v;
        }

        /// <note>
        /// If the input data are sorted, then out vertex will be repeated for several edges in a row.
        /// In this case, bypass cache and instead immediately return a new vertex using the known id.
        /// This gives a modest performance boost, especially when the cache is large or there are 
        /// on average many edges per vertex.
        /// </note>
        public Vertex getVertex(object id)
        {
            if ((_previousOutVertexId != null) && (_previousOutVertexId == id))
                return new BatchVertex(_previousOutVertexId, this);
            else
            {
                Vertex v = retrieveFromCache(id);
                if (v == null)
                {
                    if (_loadingFromScratch) return null;
                    else
                    {
                        if (_baseGraph.getFeatures().ignoresSuppliedIds.Value)
                        {
                            System.Diagnostics.Debug.Assert(_vertexIdKey != null);
                            IEnumerator<Vertex> iter = _baseGraph.getVertices(_vertexIdKey, id).GetEnumerator();
                            if (!iter.MoveNext()) return null;
                            v = iter.Current;
                            if (iter.MoveNext()) throw new ArgumentException(string.Concat("There are multiple vertices with the provided id in the database: ", id));
                        }
                        else
                        {
                            v = _baseGraph.getVertex(id);
                            if (v == null) return null;
                        }
                        _cache.set(v, id);
                    }
                }
                return new BatchVertex(id, this);
            }
        }

        public Vertex addVertex(object id)
        {
            if (id == null) throw ExceptionFactory.vertexIdCanNotBeNull();
            if (retrieveFromCache(id) != null) throw ExceptionFactory.vertexWithIdAlreadyExists(id);
            nextElement();

            Vertex v = _baseGraph.addVertex(id);
            if (_vertexIdKey != null)
                v.setProperty(_vertexIdKey, id);

            _cache.set(v, id);
            return new BatchVertex(id, this);
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            if (!(outVertex is BatchVertex) || !(inVertex is BatchVertex))
                throw new ArgumentException("Given element was not created in this baseGraph");
            nextElement();

            Vertex ov = getCachedVertex(outVertex.getId());
            Vertex iv = getCachedVertex(inVertex.getId());

            _previousOutVertexId = outVertex.getId(); //keep track of the previous out vertex id

            _currentEdgeCached = _baseGraph.addEdge(id, ov, iv, label);
            if (_edgeIdKey != null && id != null)
                _currentEdgeCached.setProperty(_edgeIdKey, id);

            _currentEdge = new BatchEdge(this);
            return _currentEdge;
        }

        protected Edge addEdgeSupport(Vertex outVertex, Vertex inVertex, string label)
        {
            return this.addEdge(null, outVertex, inVertex, label);
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, _baseGraph.ToString());
        }

        // ################### Unsupported Graph Methods ####################

        public Edge getEdge(object id)
        {
            throw retrievalNotSupported();
        }

        public void removeVertex(Vertex vertex)
        {
            throw retrievalNotSupported();
        }

        public IEnumerable<Vertex> getVertices()
        {
            throw retrievalNotSupported();
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            throw retrievalNotSupported();
        }

        public void removeEdge(Edge edge)
        {
            throw retrievalNotSupported();
        }

        public IEnumerable<Edge> getEdges()
        {
            throw retrievalNotSupported();
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            throw retrievalNotSupported();
        }

        public GraphQuery query()
        {
            throw retrievalNotSupported();
        }

        class BatchVertex : Vertex
        {
            readonly object _externalId;
            readonly BatchGraph _batchGraph;

            public BatchVertex(object id, BatchGraph batchGraph)
            {
                if (id == null) throw new ArgumentNullException("id");
                _externalId = id;
                _batchGraph = batchGraph;
            }

            public IEnumerable<Edge> getEdges(Direction direction, params string[] labels)
            {
                throw retrievalNotSupported();
            }

            public IEnumerable<Vertex> getVertices(Direction direction, params string[] labels)
            {
                throw retrievalNotSupported();
            }

            public VertexQuery query()
            {
                throw retrievalNotSupported();
            }

            public Edge addEdge(string label, Vertex inVertex)
            {
                return _batchGraph.addEdgeSupport(this, inVertex, label);
            }

            public void setProperty(string key, object value)
            {
                _batchGraph.getCachedVertex(_externalId).setProperty(key, value);
            }

            public object getId()
            {
                return _externalId;
            }

            public object getProperty(string key)
            {
                return _batchGraph.getCachedVertex(_externalId).getProperty(key);
            }

            public IEnumerable<string> getPropertyKeys()
            {
                return _batchGraph.getCachedVertex(_externalId).getPropertyKeys();
            }

            public object removeProperty(string key)
            {
                return _batchGraph.getCachedVertex(_externalId).removeProperty(key);
            }

            public void remove()
            {
                _batchGraph.removeVertex(this);
            }

            public override string ToString()
            {
                return string.Concat("v[", _externalId, "]");
            }
        }

        class BatchEdge : Edge
        {
            readonly BatchGraph _batchGraph;

            public BatchEdge(BatchGraph batchGraph)
            {
                _batchGraph = batchGraph;
            }

            public Vertex getVertex(Direction direction)
            {
                return getWrappedEdge().getVertex(direction);
            }

            public string getLabel()
            {
                return getWrappedEdge().getLabel();
            }

            public void setProperty(string key, object value)
            {
                getWrappedEdge().setProperty(key, value);
            }

            public object getId()
            {
                return getWrappedEdge().getId();
            }

            public object getProperty(string key)
            {
                return getWrappedEdge().getProperty(key);
            }

            public IEnumerable<string> getPropertyKeys()
            {
                return getWrappedEdge().getPropertyKeys();
            }

            public object removeProperty(string key)
            {
                return getWrappedEdge().removeProperty(key);
            }

            Edge getWrappedEdge()
            {
                if (this != _batchGraph._currentEdge)
                    throw new InvalidOperationException("This edge is no longer in scope");

                return _batchGraph._currentEdgeCached;
            }

            public override string ToString()
            {
                return getWrappedEdge().ToString();
            }

            public void remove()
            {
                _batchGraph.removeEdge(this);
            }
        }

        static InvalidOperationException retrievalNotSupported()
        {
            return new InvalidOperationException("Retrieval operations are not supported during batch loading");
        }

        static InvalidOperationException removalNotSupported()
        {
            return new InvalidOperationException("Removal operations are not supported during batch loading");
        }
    }
}
