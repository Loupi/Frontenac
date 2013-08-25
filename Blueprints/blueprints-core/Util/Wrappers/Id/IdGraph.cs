using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    /// <summary>
    /// A Graph implementation which wraps another Graph implementation,
    /// enabling custom element IDs even for those graphs which don't otherwise support them.
    /// 
    /// The base Graph must be an instance of KeyIndexableGraph.
    /// It *may* be an instance of IIndexableGraph, in which case its indices will be wrapped appropriately.
    /// It *may* be an instance of TransactionalGraph, in which case transaction operations will be passed through.
    /// For those graphs which support vertex indices but not edge indices (or vice versa),
    /// you may configure IdGraph to use custom IDs only for vertices or only for edges.
    /// </summary>
    public class IdGraph : IKeyIndexableGraph, IWrapperGraph, IIndexableGraph, ITransactionalGraph
    {
        // Note: using "__id" instead of "_id" avoids collision with Rexster's "_id"
        public const string Id = "__id";

        readonly IKeyIndexableGraph _baseGraph;
        IIdFactory _vertexIdFactory;
        IIdFactory _edgeIdFactory;
        readonly Features _features;
        readonly bool _supportVertexIds, _supportEdgeIds;
        bool _uniqueIds = true;

        /// <summary>
        /// Adds custom ID functionality to the given graph,
        /// supporting both custom vertex IDs and custom edge IDs.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph)
            : this(baseGraph, true, true)
        {

        }

        /// <summary>
        /// Adds custom ID functionality to the given graph,
        /// supporting either custom vertex IDs, custom edge IDs, or both.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        /// <param name="supportVertexIds">whether to support custom vertex IDs</param>
        /// <param name="supportEdgeIds">whether to support custom edge IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph, bool supportVertexIds, bool supportEdgeIds)
        {
            _baseGraph = baseGraph;
            _features = _baseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
            _features.IgnoresSuppliedIds = false;

            _supportVertexIds = supportVertexIds;
            _supportEdgeIds = supportEdgeIds;

            if (!supportVertexIds && !supportEdgeIds)
                throw new ArgumentException("if neither custom vertex IDs nor custom edge IDs are supported, IdGraph can't help you!");

            CreateIndices();

            _vertexIdFactory = new DefaultIdFactory();
            _edgeIdFactory = new DefaultIdFactory();
        }

        /// <summary>
        /// When vertices are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">A factory for new vertex IDs.</param>
        public void SetVertexIdFactory(IIdFactory idFactory)
        {
            _vertexIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">a factory for new edge IDs.</param>
        public void SetEdgeIdFactory(IIdFactory idFactory)
        {
            _edgeIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new vertex IDs.</returns>
        public IIdFactory GetVertexIdFactory()
        {
            return _vertexIdFactory;
        }


        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new edge IDs.</returns>
        public IIdFactory GetEdgeIdFactory()
        {
            return _edgeIdFactory;
        }

        public Features Features
        {
            get { return _features; }
        }

        public IVertex AddVertex(object id)
        {
            if (_uniqueIds && null != id && null != GetVertex(id))
                throw new ArgumentException(string.Concat("vertex with given id already exists: '", id, "'"));

            IVertex base_ = _baseGraph.AddVertex(null);

            if (_supportVertexIds)
            {
                object v = id ?? _vertexIdFactory.CreateId();

                if (null != v)
                    base_.SetProperty(Id, v);
            }

            return new IdVertex(base_, this);
        }

        public IVertex GetVertex(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_supportVertexIds)
            {
                IEnumerable<IVertex> i = _baseGraph.GetVertices(Id, id);
                IEnumerator<IVertex> iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                var e = iter.Current;

                if (iter.MoveNext())
                    throw new InvalidOperationException(string.Concat("multiple vertices exist with id '", id, "'"));

                return new IdVertex(e, this);
            }
            var base_ = _baseGraph.GetVertex(id);
            return null == base_ ? null : new IdVertex(base_, this);
        }

        public void RemoveVertex(IVertex vertex)
        {
            VerifyNativeElement(vertex);
            _baseGraph.RemoveVertex(((IdVertex)vertex).GetBaseVertex());
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new IdVertexIterable(_baseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            if (_supportVertexIds && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by IdGraph"));
            return new IdVertexIterable(_baseGraph.GetVertices(key, value), this);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            if (_uniqueIds && null != id && null != GetEdge(id))
                throw new ArgumentException(string.Concat("edge with given id already exists: ", id));

            VerifyNativeElement(outVertex);
            VerifyNativeElement(inVertex);

            IEdge base_ = _baseGraph.AddEdge(null, ((IdVertex)outVertex).GetBaseVertex(), ((IdVertex)inVertex).GetBaseVertex(), label);

            if (_supportEdgeIds)
            {
                object v = id ?? _edgeIdFactory.CreateId();

                if (null != v)
                    base_.SetProperty(Id, v);
            }

            return new IdEdge(base_, this);
        }

        public IEdge GetEdge(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_supportEdgeIds)
            {
                IEnumerable<IEdge> i = _baseGraph.GetEdges(Id, id);
                IEnumerator<IEdge> iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                var e = iter.Current;

                if (iter.MoveNext())
                    throw new InvalidOperationException(string.Concat("multiple edges exist with id ", id));

                return new IdEdge(e, this);
            }
            var base_ = _baseGraph.GetEdge(id);
            return null == base_ ? null : new IdEdge(base_, this);
        }

        public void RemoveEdge(IEdge edge)
        {
            VerifyNativeElement(edge);
            _baseGraph.RemoveEdge(((IdEdge)edge).GetBaseEdge());
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new IdEdgeIterable(_baseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (_supportEdgeIds && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by IdGraph"));
            return new IdEdgeIterable(_baseGraph.GetEdges(key, value), this);
        }

        public void Rollback()
        {
            if (_baseGraph is ITransactionalGraph)
                (_baseGraph as ITransactionalGraph).Rollback();
        }

        public void Commit()
        {
            if (_baseGraph is ITransactionalGraph)
                (_baseGraph as ITransactionalGraph).Commit();
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _baseGraph.ToString());
        }

        public void DropKeyIndex(string key, Type elementClass)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by IdGraph"));
            _baseGraph.DropKeyIndex(key, elementClass);
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by IdGraph"));
            _baseGraph.CreateKeyIndex(key, elementClass, indexParameters);
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported)
            {
                ISet<string> keys = new HashSet<string>(_baseGraph.GetIndexedKeys(elementClass));
                keys.Remove(Id);
                return keys;
            }
            return _baseGraph.GetIndexedKeys(elementClass);
        }

        public IGraph GetBaseGraph()
        {
            return _baseGraph;
        }

        public void EnforceUniqueIds(bool enforceUniqueIds)
        {
            _uniqueIds = enforceUniqueIds;
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            VerifyBaseGraphIsIndexableGraph();

            if (IsVertexClass(indexClass))
                return new IdVertexIndex(((IIndexableGraph)_baseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
            return new IdEdgeIndex(((IIndexableGraph)_baseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            VerifyBaseGraphIsIndexableGraph();

            IIndex baseIndex = ((IIndexableGraph)_baseGraph).GetIndex(indexName, indexClass);
            return null == baseIndex ? null : new IdVertexIndex(baseIndex, this);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            throw new InvalidOperationException("sorry, you currently can't get a list of indexes through IdGraph");
        }

        public void DropIndex(string indexName)
        {
            VerifyBaseGraphIsIndexableGraph();

            ((IIndexableGraph)_baseGraph).DropIndex(indexName);
        }

        public IGraphQuery Query()
        {
            return new WrappedGraphQuery(_baseGraph.Query(),
                t => new IdEdgeIterable(t.Edges(), this),
                t => new IdVertexIterable(t.Vertices(), this));
        }

        public bool GetSupportVertexIds()
        {
            return _supportVertexIds;
        }

        public bool GetSupportEdgeIds()
        {
            return _supportEdgeIds;
        }

        /// <summary>
        /// A factory for IDs of newly-created vertices and edges (where an ID is not otherwise specified).
        /// </summary>
        public interface IIdFactory
        {
            object CreateId();
        }

        class DefaultIdFactory : IIdFactory
        {
            public object CreateId()
            {
                return Guid.NewGuid().ToString();
            }
        }

        void VerifyBaseGraphIsIndexableGraph()
        {
            if (!(_baseGraph is IIndexableGraph))
                throw new InvalidOperationException("base graph is not an indexable graph");
        }

        static bool IsVertexClass(Type c)
        {
            return typeof(IVertex).IsAssignableFrom(c);
        }

        void CreateIndices()
        {
            if (_supportVertexIds && !_baseGraph.GetIndexedKeys(typeof(IVertex)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof(IVertex));

            if (_supportEdgeIds && !_baseGraph.GetIndexedKeys(typeof(IEdge)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof(IEdge));
        }

        static void VerifyNativeElement(IElement e)
        {
            if (e == null) throw new ArgumentNullException("e");
            if (!(e is IdElement))
                throw new ArgumentException("given element was not created in this graph");
        }

        public virtual void Shutdown()
        {
            _baseGraph.Shutdown();
        }
    }
}
