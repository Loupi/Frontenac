using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    /// <summary>
    /// A Graph implementation which wraps another Graph implementation,
    /// enabling custom element IDs even for those graphs which don't otherwise support them.
    /// 
    /// The base Graph must be an instance of KeyIndexableGraph.
    /// It *may* be an instance of IndexableGraph, in which case its indices will be wrapped appropriately.
    /// It *may* be an instance of TransactionalGraph, in which case transaction operations will be passed through.
    /// For those graphs which support vertex indices but not edge indices (or vice versa),
    /// you may configure IdGraph to use custom IDs only for vertices or only for edges.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdGraph : KeyIndexableGraph, WrapperGraph, IndexableGraph, TransactionalGraph
    {
        // Note: using "__id" instead of "_id" avoids collision with Rexster's "_id"
        public const string ID = "__id";

        readonly KeyIndexableGraph _BaseGraph;
        IdFactory _VertexIdFactory;
        IdFactory _EdgeIdFactory;
        readonly Features _Features;
        readonly bool _SupportVertexIds, _SupportEdgeIds;
        bool _UniqueIds = true;

        /// <summary>
        /// Adds custom ID functionality to the given graph,
        /// supporting both custom vertex IDs and custom edge IDs.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        public IdGraph(KeyIndexableGraph baseGraph)
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
        public IdGraph(KeyIndexableGraph baseGraph, bool supportVertexIds, bool supportEdgeIds)
        {
            _BaseGraph = baseGraph;
            _Features = _BaseGraph.GetFeatures().CopyFeatures();
            _Features.IsWrapper = true;
            _Features.IgnoresSuppliedIds = false;

            _SupportVertexIds = supportVertexIds;
            _SupportEdgeIds = supportEdgeIds;

            if (!supportVertexIds && !supportEdgeIds)
                throw new ArgumentException("if neither custom vertex IDs nor custom edge IDs are supported, IdGraph can't help you!");

            CreateIndices();

            _VertexIdFactory = new DefaultIdFactory();
            _EdgeIdFactory = new DefaultIdFactory();
        }

        /// <summary>
        /// When vertices are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">A factory for new vertex IDs.</param>
        public void SetVertexIdFactory(IdFactory idFactory)
        {
            _VertexIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">a factory for new edge IDs.</param>
        public void SetEdgeIdFactory(IdFactory idFactory)
        {
            _EdgeIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new vertex IDs.</returns>
        public IdFactory GetVertexIdFactory()
        {
            return _VertexIdFactory;
        }


        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new edge IDs.</returns>
        public IdFactory GetEdgeIdFactory()
        {
            return _EdgeIdFactory;
        }

        public Features GetFeatures()
        {
            return _Features;
        }

        public Vertex AddVertex(object id)
        {
            if (_UniqueIds && null != id && null != GetVertex(id))
                throw new ArgumentException(string.Concat("vertex with given id already exists: '", id, "'"));

            Vertex base_ = _BaseGraph.AddVertex(null);

            if (_SupportVertexIds)
            {
                object v = null == id ? _VertexIdFactory.CreateId() : id;

                if (null != v)
                    base_.SetProperty(ID, v);
            }

            return new IdVertex(base_, this);
        }

        public Vertex GetVertex(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_SupportVertexIds)
            {
                IEnumerable<Vertex> i = _BaseGraph.GetVertices(ID, id);
                IEnumerator<Vertex> iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                else
                {
                    Vertex e = iter.Current;

                    if (iter.MoveNext())
                        throw new InvalidOperationException(string.Concat("multiple vertices exist with id '", id, "'"));

                    return new IdVertex(e, this);
                }
            }
            else
            {
                Vertex base_ = _BaseGraph.GetVertex(id);
                return null == base_ ? null : new IdVertex(base_, this);
            }
        }

        public void RemoveVertex(Vertex vertex)
        {
            VerifyNativeElement(vertex);
            _BaseGraph.RemoveVertex(((IdVertex)vertex).GetBaseVertex());
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return new IdVertexIterable(_BaseGraph.GetVertices(), this);
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            if (_SupportVertexIds && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                return new IdVertexIterable(_BaseGraph.GetVertices(key, value), this);
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            if (_UniqueIds && null != id && null != GetEdge(id))
                throw new ArgumentException(string.Concat("edge with given id already exists: ", id));

            VerifyNativeElement(outVertex);
            VerifyNativeElement(inVertex);

            Edge base_ = _BaseGraph.AddEdge(null, ((IdVertex)outVertex).GetBaseVertex(), ((IdVertex)inVertex).GetBaseVertex(), label);

            if (_SupportEdgeIds)
            {
                object v = null == id ? _EdgeIdFactory.CreateId() : id;

                if (null != v)
                    base_.SetProperty(ID, v);
            }

            return new IdEdge(base_, this);
        }

        public Edge GetEdge(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_SupportEdgeIds)
            {
                IEnumerable<Edge> i = _BaseGraph.GetEdges(ID, id);
                IEnumerator<Edge> iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                else
                {
                    Edge e = iter.Current;

                    if (iter.MoveNext())
                        throw new InvalidOperationException(string.Concat("multiple edges exist with id ", id));

                    return new IdEdge(e, this);
                }
            }
            else
            {
                Edge base_ = _BaseGraph.GetEdge(id);
                return null == base_ ? null : new IdEdge(base_, this);
            }
        }

        public void RemoveEdge(Edge edge)
        {
            VerifyNativeElement(edge);
            _BaseGraph.RemoveEdge(((IdEdge)edge).GetBaseEdge());
        }

        public IEnumerable<Edge> GetEdges()
        {
            return new IdEdgeIterable(_BaseGraph.GetEdges(), this);
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            if (_SupportEdgeIds && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                return new IdEdgeIterable(_BaseGraph.GetEdges(key, value), this);
        }

        public void Rollback()
        {
            if (this._BaseGraph is TransactionalGraph)
                (_BaseGraph as TransactionalGraph).Rollback();
        }

        public void Commit()
        {
            if (this._BaseGraph is TransactionalGraph)
                (_BaseGraph as TransactionalGraph).Commit();
        }

        public void Shutdown()
        {
            _BaseGraph.Shutdown();
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        public void DropKeyIndex(string key, Type elementClass)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _SupportVertexIds) || (!v && _SupportEdgeIds));

            if (supported && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                _BaseGraph.DropKeyIndex(key, elementClass);
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _SupportVertexIds) || (!v && _SupportEdgeIds));

            if (supported && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                _BaseGraph.CreateKeyIndex(key, elementClass, indexParameters);
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            bool v = IsVertexClass(elementClass);
            bool supported = ((v && _SupportVertexIds) || (!v && _SupportEdgeIds));

            if (supported)
            {
                ISet<string> keys = new HashSet<string>(_BaseGraph.GetIndexedKeys(elementClass));
                keys.Remove(ID);
                return keys;
            }
            else
                return _BaseGraph.GetIndexedKeys(elementClass);
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public void EnforceUniqueIds(bool enforceUniqueIds)
        {
            _UniqueIds = enforceUniqueIds;
        }

        public Index CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            VerifyBaseGraphIsIndexableGraph();

            if (IsVertexClass(indexClass))
                return new IdVertexIndex(((IndexableGraph)_BaseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
            else
                return new IdEdgeIndex(((IndexableGraph)_BaseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
        }

        public Index GetIndex(string indexName, Type indexClass)
        {
            VerifyBaseGraphIsIndexableGraph();

            Index baseIndex = ((IndexableGraph)_BaseGraph).GetIndex(indexName, indexClass);
            return null == baseIndex ? null : new IdVertexIndex(baseIndex, this);
        }

        public IEnumerable<Index> GetIndices()
        {
            throw new InvalidOperationException("sorry, you currently can't get a list of indexes through IdGraph");
        }

        public void DropIndex(string indexName)
        {
            VerifyBaseGraphIsIndexableGraph();

            ((IndexableGraph)_BaseGraph).DropIndex(indexName);
        }

        public GraphQuery Query()
        {
            return new WrappedGraphQuery(_BaseGraph.Query(),
                t => new IdEdgeIterable(t.Edges(), this),
                t => new IdVertexIterable(t.Vertices(), this));
        }

        public bool GetSupportVertexIds()
        {
            return _SupportVertexIds;
        }

        public bool GetSupportEdgeIds()
        {
            return _SupportEdgeIds;
        }

        /// <summary>
        /// A factory for IDs of newly-created vertices and edges (where an ID is not otherwise specified).
        /// </summary>
        public interface IdFactory
        {
            object CreateId();
        }

        class DefaultIdFactory : IdFactory
        {
            public object CreateId()
            {
                return Guid.NewGuid().ToString();
            }
        }

        void VerifyBaseGraphIsIndexableGraph()
        {
            if (!(_BaseGraph is IndexableGraph))
                throw new InvalidOperationException("base graph is not an indexable graph");
        }

        bool IsVertexClass(Type c)
        {
            return typeof(Vertex).IsAssignableFrom(c);
        }

        void CreateIndices()
        {
            if (_SupportVertexIds && !_BaseGraph.GetIndexedKeys(typeof(Vertex)).Contains(ID))
                _BaseGraph.CreateKeyIndex(ID, typeof(Vertex));

            if (_SupportEdgeIds && !_BaseGraph.GetIndexedKeys(typeof(Edge)).Contains(ID))
                _BaseGraph.CreateKeyIndex(ID, typeof(Edge));
        }

        static void VerifyNativeElement(Element e)
        {
            if (!(e is IdElement))
                throw new ArgumentException("given element was not created in this graph");
        }
    }
}
