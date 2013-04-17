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

        readonly TransactionalGraph _BaseGraph;
        string _VertexIdKey = null;
        string _EdgeIdKey = null;
        bool _LoadingFromScratch = true;
        readonly VertexCache _Cache;
        long _BufferSize = DEFAULT_BUFFER_SIZE;
        long _RemainingBufferSize;
        BatchEdge _CurrentEdge = null;
        Edge _CurrentEdgeCached = null;
        object _PreviousOutVertexId = null;

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

            _BaseGraph = graph;
            _BufferSize = bufferSize;
            _VertexIdKey = null;
            _EdgeIdKey = null;
            _Cache = type.GetVertexCache();
            _RemainingBufferSize = _BufferSize;
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
        public static BatchGraph Wrap(Graph graph)
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
        public static BatchGraph Wrap(Graph graph, long buffer)
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
        public void SetVertexIdKey(string key)
        {
            if (!_LoadingFromScratch && key == null && _BaseGraph.GetFeatures().IgnoresSuppliedIds.Value)
                throw new InvalidOperationException("Cannot set vertex id key to null when not loading from scratch while ids are ignored.");
            _VertexIdKey = key;
        }

        /// <summary>
        /// Returns the key used to set the id on the vertices or null if such has not been set
        /// via setVertexIdKey
        /// </summary>
        /// <returns>The key used to set the id on the vertices or null if such has not been set</returns>
        public string GetVertexIdKey()
        {
            return _VertexIdKey;
        }

        /// <summary>
        /// Sets the key to be used when setting the edge id as a property on the respective edge.
        /// If the key is null, then no property will be set.
        /// If the loaded baseGraph should later be wrapped with IdGraphuse IdGraph.ID.
        /// </summary>
        /// <param name="key">Key to be used.</param>
        public void SetEdgeIdKey(string key)
        {
            _EdgeIdKey = key;
        }

        /// <summary>
        /// Returns the key used to set the id on the edges or null if such has not been set
        /// via setEdgeIdKey
        /// </summary>
        /// <returns>The key used to set the id on the edges or null if such has not been set</returns>
        public string GetEdgeIdKey()
        {
            return _EdgeIdKey;
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
        public void SetLoadingFromScratch(bool fromScratch)
        {
            if (fromScratch == false && _VertexIdKey == null && _BaseGraph.GetFeatures().IgnoresSuppliedIds.Value)
                throw new InvalidOperationException("Vertex id key is required to query existing vertices in wrapped graph.");
            _LoadingFromScratch = fromScratch;
        }

        /// <summary>
        /// Whether this BatchGraph is loading data from scratch or incrementally into an existing graph.
        /// By default, this returns true.
        /// see setLoadingFromScratch
        /// </summary>
        /// <returns>Whether this BatchGraph is loading data from scratch or incrementally into an existing graph.</returns>
        public bool IsLoadingFromScratch()
        {
            return _LoadingFromScratch;
        }

        void NextElement()
        {
            _CurrentEdge = null;
            _CurrentEdgeCached = null;
            if (_RemainingBufferSize <= 0)
            {
                _BaseGraph.Commit();
                _Cache.NewTransaction();
                _RemainingBufferSize = _BufferSize;
            }
            _RemainingBufferSize--;
        }

        /// <summary>
        /// Should only be invoked after loading is complete. Committing the transaction before will cause the loading to fail.
        /// </summary>
        public void Commit()
        {
            _CurrentEdge = null;
            _CurrentEdgeCached = null;
            _RemainingBufferSize = 0;
            _BaseGraph.Commit();
        }

        /// <summary>
        /// ot supported for batch loading, since data may have already been partially persisted.
        /// </summary>
        public void Rollback()
        {
            throw new InvalidOperationException("Can not rollback during batch loading");
        }

        public void Shutdown()
        {
            _BaseGraph.Commit();
            _BaseGraph.Shutdown();
            _CurrentEdge = null;
            _CurrentEdgeCached = null;
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public Features GetFeatures()
        {
            Features features = _BaseGraph.GetFeatures().CopyFeatures();
            features.IgnoresSuppliedIds = false;
            features.IsWrapper = true;
            features.SupportsEdgeIteration = false;
            features.SupportsThreadedTransactions = false;
            features.SupportsVertexIteration = false;
            return features;
        }

        Vertex RetrieveFromCache(object externalID)
        {
            object internal_ = _Cache.GetEntry(externalID);
            if (internal_ is Vertex)
                return (Vertex)internal_;
            else if (internal_ != null)
            {
                //its an internal id
                Vertex v = _BaseGraph.GetVertex(internal_);
                _Cache.Set(v, externalID);
                return v;
            }
            else
                return null;
        }

        Vertex GetCachedVertex(object externalID)
        {
            Vertex v = RetrieveFromCache(externalID);
            if (v == null) throw new ArgumentException(string.Concat("Vertex for given ID cannot be found: ", externalID));
            return v;
        }

        /// <note>
        /// If the input data are sorted, then out vertex will be repeated for several edges in a row.
        /// In this case, bypass cache and instead immediately return a new vertex using the known id.
        /// This gives a modest performance boost, especially when the cache is large or there are 
        /// on average many edges per vertex.
        /// </note>
        public Vertex GetVertex(object id)
        {
            if ((_PreviousOutVertexId != null) && (_PreviousOutVertexId == id))
                return new BatchVertex(_PreviousOutVertexId, this);
            else
            {
                Vertex v = RetrieveFromCache(id);
                if (v == null)
                {
                    if (_LoadingFromScratch) return null;
                    else
                    {
                        if (_BaseGraph.GetFeatures().IgnoresSuppliedIds.Value)
                        {
                            System.Diagnostics.Debug.Assert(_VertexIdKey != null);
                            IEnumerator<Vertex> iter = _BaseGraph.GetVertices(_VertexIdKey, id).GetEnumerator();
                            if (!iter.MoveNext()) return null;
                            v = iter.Current;
                            if (iter.MoveNext()) throw new ArgumentException(string.Concat("There are multiple vertices with the provided id in the database: ", id));
                        }
                        else
                        {
                            v = _BaseGraph.GetVertex(id);
                            if (v == null) return null;
                        }
                        _Cache.Set(v, id);
                    }
                }
                return new BatchVertex(id, this);
            }
        }

        public Vertex AddVertex(object id)
        {
            if (id == null) throw ExceptionFactory.VertexIdCanNotBeNull();
            if (RetrieveFromCache(id) != null) throw ExceptionFactory.VertexWithIdAlreadyExists(id);
            NextElement();

            Vertex v = _BaseGraph.AddVertex(id);
            if (_VertexIdKey != null)
                v.SetProperty(_VertexIdKey, id);

            _Cache.Set(v, id);
            return new BatchVertex(id, this);
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            if (!(outVertex is BatchVertex) || !(inVertex is BatchVertex))
                throw new ArgumentException("Given element was not created in this baseGraph");
            NextElement();

            Vertex ov = GetCachedVertex(outVertex.GetId());
            Vertex iv = GetCachedVertex(inVertex.GetId());

            _PreviousOutVertexId = outVertex.GetId(); //keep track of the previous out vertex id

            _CurrentEdgeCached = _BaseGraph.AddEdge(id, ov, iv, label);
            if (_EdgeIdKey != null && id != null)
                _CurrentEdgeCached.SetProperty(_EdgeIdKey, id);

            _CurrentEdge = new BatchEdge(this);
            return _CurrentEdge;
        }

        protected Edge AddEdgeSupport(Vertex outVertex, Vertex inVertex, string label)
        {
            return this.AddEdge(null, outVertex, inVertex, label);
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        // ################### Unsupported Graph Methods ####################

        public Edge GetEdge(object id)
        {
            throw RetrievalNotSupported();
        }

        public void RemoveVertex(Vertex vertex)
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<Vertex> GetVertices()
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            throw RetrievalNotSupported();
        }

        public void RemoveEdge(Edge edge)
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<Edge> GetEdges()
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            throw RetrievalNotSupported();
        }

        public GraphQuery Query()
        {
            throw RetrievalNotSupported();
        }

        class BatchVertex : Vertex
        {
            readonly object _ExternalID;
            readonly BatchGraph _BatchGraph;

            public BatchVertex(object id, BatchGraph batchGraph)
            {
                if (id == null) throw new ArgumentNullException("id");
                _ExternalID = id;
                _BatchGraph = batchGraph;
            }

            public IEnumerable<Edge> GetEdges(Direction direction, params string[] labels)
            {
                throw RetrievalNotSupported();
            }

            public IEnumerable<Vertex> GetVertices(Direction direction, params string[] labels)
            {
                throw RetrievalNotSupported();
            }

            public VertexQuery Query()
            {
                throw RetrievalNotSupported();
            }

            public Edge AddEdge(string label, Vertex inVertex)
            {
                return _BatchGraph.AddEdgeSupport(this, inVertex, label);
            }

            public void SetProperty(string key, object value)
            {
                _BatchGraph.GetCachedVertex(_ExternalID).SetProperty(key, value);
            }

            public object GetId()
            {
                return _ExternalID;
            }

            public object GetProperty(string key)
            {
                return _BatchGraph.GetCachedVertex(_ExternalID).GetProperty(key);
            }

            public IEnumerable<string> GetPropertyKeys()
            {
                return _BatchGraph.GetCachedVertex(_ExternalID).GetPropertyKeys();
            }

            public object RemoveProperty(string key)
            {
                return _BatchGraph.GetCachedVertex(_ExternalID).RemoveProperty(key);
            }

            public void Remove()
            {
                _BatchGraph.RemoveVertex(this);
            }

            public override string ToString()
            {
                return string.Concat("v[", _ExternalID, "]");
            }
        }

        class BatchEdge : Edge
        {
            readonly BatchGraph _BatchGraph;

            public BatchEdge(BatchGraph batchGraph)
            {
                _BatchGraph = batchGraph;
            }

            public Vertex GetVertex(Direction direction)
            {
                return GetWrappedEdge().GetVertex(direction);
            }

            public string GetLabel()
            {
                return GetWrappedEdge().GetLabel();
            }

            public void SetProperty(string key, object value)
            {
                GetWrappedEdge().SetProperty(key, value);
            }

            public object GetId()
            {
                return GetWrappedEdge().GetId();
            }

            public object GetProperty(string key)
            {
                return GetWrappedEdge().GetProperty(key);
            }

            public IEnumerable<string> GetPropertyKeys()
            {
                return GetWrappedEdge().GetPropertyKeys();
            }

            public object RemoveProperty(string key)
            {
                return GetWrappedEdge().RemoveProperty(key);
            }

            Edge GetWrappedEdge()
            {
                if (this != _BatchGraph._CurrentEdge)
                    throw new InvalidOperationException("This edge is no longer in scope");

                return _BatchGraph._CurrentEdgeCached;
            }

            public override string ToString()
            {
                return GetWrappedEdge().ToString();
            }

            public void Remove()
            {
                _BatchGraph.RemoveEdge(this);
            }
        }

        static InvalidOperationException RetrievalNotSupported()
        {
            return new InvalidOperationException("Retrieval operations are not supported during batch loading");
        }

        static InvalidOperationException RemovalNotSupported()
        {
            return new InvalidOperationException("Removal operations are not supported during batch loading");
        }
    }
}
