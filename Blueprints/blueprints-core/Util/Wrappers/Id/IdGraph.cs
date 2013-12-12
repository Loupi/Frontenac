using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    /// <summary>
    ///     A Graph implementation which wraps another Graph implementation,
    ///     enabling custom element IDs even for those graphs which don't otherwise support them.
    ///     The base Graph must be an instance of KeyIndexableGraph.
    ///     It *may* be an instance of IIndexableGraph, in which case its indices will be wrapped appropriately.
    ///     It *may* be an instance of TransactionalGraph, in which case transaction operations will be passed through.
    ///     For those graphs which support vertex indices but not edge indices (or vice versa),
    ///     you may configure IdGraph to use custom IDs only for vertices or only for edges.
    /// </summary>
    public class IdGraph : IKeyIndexableGraph, IWrapperGraph, IIndexableGraph, ITransactionalGraph
    {
        // Note: using "__id" instead of "_id" avoids collision with Rexster's "_id"
        public const string Id = "__id";

        private readonly IKeyIndexableGraph _baseGraph;
        private readonly Features _features;
        private readonly bool _supportEdgeIds;
        private readonly bool _supportVertexIds;
        private IIdFactory _edgeIdFactory;
        private bool _uniqueIds = true;
        private IIdFactory _vertexIdFactory;

        /// <summary>
        ///     Adds custom ID functionality to the given graph,
        ///     supporting both custom vertex IDs and custom edge IDs.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph)
            : this(baseGraph, true, true)
        {
        }

        /// <summary>
        ///     Adds custom ID functionality to the given graph,
        ///     supporting either custom vertex IDs, custom edge IDs, or both.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        /// <param name="supportVertexIds">whether to support custom vertex IDs</param>
        /// <param name="supportEdgeIds">whether to support custom edge IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph, bool supportVertexIds, bool supportEdgeIds)
        {
            Contract.Requires(baseGraph != null);
            Contract.Requires(supportVertexIds || supportEdgeIds);

            _baseGraph = baseGraph;
            _features = _baseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
            _features.IgnoresSuppliedIds = false;

            _supportVertexIds = supportVertexIds;
            _supportEdgeIds = supportEdgeIds;

            CreateIndices();

            _vertexIdFactory = new DefaultIdFactory();
            _edgeIdFactory = new DefaultIdFactory();
        }

        /// <summary>
        ///     When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <value>the factory for new vertex IDs.</value>
        public IIdFactory VertexIdFactory
        {
            get
            {
                Contract.Ensures(Contract.Result<IIdFactory>() != null);
                return _vertexIdFactory;
            }
            set
            {
                Contract.Requires(value != null);
                _vertexIdFactory = value;
            }
        }


        /// <summary>
        ///     When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <value>the factory for new edge IDs.</value>
        public IIdFactory EdgeIdFactory
        {
            get
            {
                Contract.Ensures(Contract.Result<IIdFactory>() != null);
                return _edgeIdFactory;
            }
            set
            {
                Contract.Requires(value != null);
                _edgeIdFactory = value;
            }
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            VerifyBaseGraphIsIndexableGraph();

            if (IsVertex(indexClass))
                return
                    new IdVertexIndex(
                        ((IIndexableGraph) _baseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
            return new IdEdgeIndex(((IIndexableGraph) _baseGraph).CreateIndex(indexName, indexClass, indexParameters),
                                   this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            VerifyBaseGraphIsIndexableGraph();

            var baseIndex = ((IIndexableGraph) _baseGraph).GetIndex(indexName, indexClass);
            return null == baseIndex ? null : new IdVertexIndex(baseIndex, this);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            throw new NotImplementedException("sorry, you currently can't get a list of indexes through IdGraph");
        }

        public void DropIndex(string indexName)
        {
            VerifyBaseGraphIsIndexableGraph();

            ((IIndexableGraph) _baseGraph).DropIndex(indexName);
        }

        public Features Features
        {
            get { return _features; }
        }

        public IVertex AddVertex(object id)
        {
            if (_uniqueIds && null != id && null != GetVertex(id))
                throw new ArgumentException(string.Concat("vertex with given id already exists: '", id, "'"));

            var base_ = _baseGraph.AddVertex(null);

            if (_supportVertexIds)
            {
                var v = id ?? _vertexIdFactory.CreateId();

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
                var i = _baseGraph.GetVertices(Id, id);
                var iter = i.GetEnumerator();
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
            _baseGraph.RemoveVertex(((IdVertex) vertex).GetBaseVertex());
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

            var base_ = _baseGraph.AddEdge(null, ((IdVertex) outVertex).GetBaseVertex(),
                                           ((IdVertex) inVertex).GetBaseVertex(), label);

            if (_supportEdgeIds)
            {
                var v = id ?? _edgeIdFactory.CreateId();

                if (null != v)
                    base_.SetProperty(Id, v);
            }

            return new IdEdge(base_, this);
        }

        public IEdge GetEdge(object id)
        {
            if (_supportEdgeIds)
            {
                var i = _baseGraph.GetEdges(Id, id);
                var iter = i.GetEnumerator();
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
            _baseGraph.RemoveEdge(((IdEdge) edge).GetBaseEdge());
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

        public void DropKeyIndex(string key, Type elementClass)
        {
            Contract.Ensures(key == Id);
            Contract.Ensures(IsVertex(elementClass) && _supportVertexIds || IsEdge(elementClass) && _supportEdgeIds);
            _baseGraph.DropKeyIndex(key, elementClass);
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            Contract.Ensures(key == Id);
            Contract.Ensures(IsVertex(elementClass) && _supportVertexIds || IsEdge(elementClass) && _supportEdgeIds);
            _baseGraph.CreateKeyIndex(key, elementClass, indexParameters);
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            var v = IsVertex(elementClass);
            var supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported)
            {
                ISet<string> keys = new HashSet<string>(_baseGraph.GetIndexedKeys(elementClass));
                keys.Remove(Id);
                return keys;
            }
            return _baseGraph.GetIndexedKeys(elementClass);
        }

        public IQuery Query()
        {
            return new WrappedQuery(_baseGraph.Query(),
                                    t => new IdEdgeIterable(t.Edges(), this),
                                    t => new IdVertexIterable(t.Vertices(), this));
        }

        public virtual void Shutdown()
        {
            _baseGraph.Shutdown();
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

        public IGraph GetBaseGraph()
        {
            return _baseGraph;
        }

        public override string ToString()
        {
            return this.GraphString(_baseGraph.ToString());
        }

        public void EnforceUniqueIds(bool enforceUniqueIds)
        {
            _uniqueIds = enforceUniqueIds;
        }

        public bool GetSupportVertexIds()
        {
            return _supportVertexIds;
        }

        public bool GetSupportEdgeIds()
        {
            return _supportEdgeIds;
        }

        private void VerifyBaseGraphIsIndexableGraph()
        {
            Contract.Requires(_baseGraph is IIndexableGraph);
        }

        [Pure]
        private static bool IsVertex(Type c)
        {
            return typeof (IVertex).IsAssignableFrom(c);
        }

        [Pure]
        private static bool IsEdge(Type c)
        {
            return typeof (IEdge).IsAssignableFrom(c);
        }

        private void CreateIndices()
        {
            if (_supportVertexIds && !_baseGraph.GetIndexedKeys(typeof (IVertex)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof (IVertex));

            if (_supportEdgeIds && !_baseGraph.GetIndexedKeys(typeof (IEdge)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof (IEdge));
        }

        private static void VerifyNativeElement(IElement e)
        {
            Contract.Requires(e is IdElement);
        }

        private class DefaultIdFactory : IIdFactory
        {
            public object CreateId()
            {
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        ///     A factory for IDs of newly-created vertices and edges (where an ID is not otherwise specified).
        /// </summary>
        [ContractClass(typeof (IdFactoryContract))]
        public interface IIdFactory
        {
            object CreateId();
        }

        [ContractClassFor(typeof (IIdFactory))]
        public abstract class IdFactoryContract : IIdFactory
        {
            public object CreateId()
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return null;
            }
        }
    }
}