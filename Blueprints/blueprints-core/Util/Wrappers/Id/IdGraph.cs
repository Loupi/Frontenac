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

        readonly KeyIndexableGraph _baseGraph;
        IdFactory _vertexIdFactory;
        IdFactory _edgeIdFactory;
        readonly Features _features;
        readonly bool _supportVertexIds, _supportEdgeIds;
        bool _uniqueIds = true;

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
            _baseGraph = baseGraph;
            _features = _baseGraph.getFeatures().copyFeatures();
            _features.isWrapper = true;
            _features.ignoresSuppliedIds = false;

            _supportVertexIds = supportVertexIds;
            _supportEdgeIds = supportEdgeIds;

            if (!supportVertexIds && !supportEdgeIds)
                throw new ArgumentException("if neither custom vertex IDs nor custom edge IDs are supported, IdGraph can't help you!");

            createIndices();

            _vertexIdFactory = new DefaultIdFactory();
            _edgeIdFactory = new DefaultIdFactory();
        }

        /// <summary>
        /// When vertices are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">A factory for new vertex IDs.</param>
        public void setVertexIdFactory(IdFactory idFactory)
        {
            _vertexIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <param name="idFactory">a factory for new edge IDs.</param>
        public void setEdgeIdFactory(IdFactory idFactory)
        {
            _edgeIdFactory = idFactory;
        }

        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new vertex IDs.</returns>
        public IdFactory getVertexIdFactory()
        {
            return _vertexIdFactory;
        }


        /// <summary>
        /// When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <returns>the factory for new edge IDs.</returns>
        public IdFactory getEdgeIdFactory()
        {
            return _edgeIdFactory;
        }

        public Features getFeatures()
        {
            return _features;
        }

        public Vertex addVertex(object id)
        {
            if (_uniqueIds && null != id && null != getVertex(id))
                throw new ArgumentException(string.Concat("vertex with given id already exists: '", id, "'"));

            Vertex base_ = _baseGraph.addVertex(null);

            if (_supportVertexIds)
            {
                object v = null == id ? _vertexIdFactory.createId() : id;

                if (null != v)
                    base_.setProperty(ID, v);
            }

            return new IdVertex(base_, this);
        }

        public Vertex getVertex(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_supportVertexIds)
            {
                IEnumerable<Vertex> i = _baseGraph.getVertices(ID, id);
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
                Vertex base_ = _baseGraph.getVertex(id);
                return null == base_ ? null : new IdVertex(base_, this);
            }
        }

        public void removeVertex(Vertex vertex)
        {
            verifyNativeElement(vertex);
            _baseGraph.removeVertex(((IdVertex)vertex).getBaseVertex());
        }

        public IEnumerable<Vertex> getVertices()
        {
            return new IdVertexIterable(_baseGraph.getVertices(), this);
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            if (_supportVertexIds && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                return new IdVertexIterable(_baseGraph.getVertices(key, value), this);
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            if (_uniqueIds && null != id && null != getEdge(id))
                throw new ArgumentException(string.Concat("edge with given id already exists: ", id));

            verifyNativeElement(outVertex);
            verifyNativeElement(inVertex);

            Edge base_ = _baseGraph.addEdge(null, ((IdVertex)outVertex).getBaseVertex(), ((IdVertex)inVertex).getBaseVertex(), label);

            if (_supportEdgeIds)
            {
                object v = null == id ? _edgeIdFactory.createId() : id;

                if (null != v)
                    base_.setProperty(ID, v);
            }

            return new IdEdge(base_, this);
        }

        public Edge getEdge(object id)
        {
            if (null == id)
                throw new ArgumentNullException("id");

            if (_supportEdgeIds)
            {
                IEnumerable<Edge> i = _baseGraph.getEdges(ID, id);
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
                Edge base_ = _baseGraph.getEdge(id);
                return null == base_ ? null : new IdEdge(base_, this);
            }
        }

        public void removeEdge(Edge edge)
        {
            verifyNativeElement(edge);
            _baseGraph.removeEdge(((IdEdge)edge).getBaseEdge());
        }

        public IEnumerable<Edge> getEdges()
        {
            return new IdEdgeIterable(_baseGraph.getEdges(), this);
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            if (_supportEdgeIds && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                return new IdEdgeIterable(_baseGraph.getEdges(key, value), this);
        }

        public void rollback()
        {
            if (this._baseGraph is TransactionalGraph)
                (_baseGraph as TransactionalGraph).rollback();
        }

        public void commit()
        {
            if (this._baseGraph is TransactionalGraph)
                (_baseGraph as TransactionalGraph).commit();
        }

        public void shutdown()
        {
            _baseGraph.shutdown();
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, _baseGraph.ToString());
        }

        public void dropKeyIndex(string key, Type elementClass)
        {
            bool v = isVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                _baseGraph.dropKeyIndex(key, elementClass);
        }

        public void createKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            bool v = isVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported && key == ID)
                throw new ArgumentException(string.Concat("index key ", ID, " is reserved by IdGraph"));
            else
                _baseGraph.createKeyIndex(key, elementClass, indexParameters);
        }

        public IEnumerable<string> getIndexedKeys(Type elementClass)
        {
            bool v = isVertexClass(elementClass);
            bool supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported)
            {
                ISet<string> keys = new HashSet<string>(_baseGraph.getIndexedKeys(elementClass));
                keys.Remove(ID);
                return keys;
            }
            else
                return _baseGraph.getIndexedKeys(elementClass);
        }

        public Graph getBaseGraph()
        {
            return _baseGraph;
        }

        public void enforceUniqueIds(bool enforceUniqueIds)
        {
            _uniqueIds = enforceUniqueIds;
        }

        public Index createIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            verifyBaseGraphIsIndexableGraph();

            if (isVertexClass(indexClass))
                return new IdVertexIndex(((IndexableGraph)_baseGraph).createIndex(indexName, indexClass, indexParameters), this);
            else
                return new IdEdgeIndex(((IndexableGraph)_baseGraph).createIndex(indexName, indexClass, indexParameters), this);
        }

        public Index getIndex(string indexName, Type indexClass)
        {
            verifyBaseGraphIsIndexableGraph();

            Index baseIndex = ((IndexableGraph)_baseGraph).getIndex(indexName, indexClass);
            return null == baseIndex ? null : new IdVertexIndex(baseIndex, this);
        }

        public IEnumerable<Index> getIndices()
        {
            throw new InvalidOperationException("sorry, you currently can't get a list of indexes through IdGraph");
        }

        public void dropIndex(string indexName)
        {
            verifyBaseGraphIsIndexableGraph();

            ((IndexableGraph)_baseGraph).dropIndex(indexName);
        }

        public GraphQuery query()
        {
            return new WrappedGraphQuery(_baseGraph.query(),
                t => new IdEdgeIterable(t.edges(), this),
                t => new IdVertexIterable(t.vertices(), this));
        }

        public bool getSupportVertexIds()
        {
            return _supportVertexIds;
        }

        public bool getSupportEdgeIds()
        {
            return _supportEdgeIds;
        }

        /// <summary>
        /// A factory for IDs of newly-created vertices and edges (where an ID is not otherwise specified).
        /// </summary>
        public interface IdFactory
        {
            object createId();
        }

        class DefaultIdFactory : IdFactory
        {
            public object createId()
            {
                return Guid.NewGuid().ToString();
            }
        }

        void verifyBaseGraphIsIndexableGraph()
        {
            if (!(_baseGraph is IndexableGraph))
                throw new InvalidOperationException("base graph is not an indexable graph");
        }

        bool isVertexClass(Type c)
        {
            return typeof(Vertex).IsAssignableFrom(c);
        }

        void createIndices()
        {
            if (_supportVertexIds && !_baseGraph.getIndexedKeys(typeof(Vertex)).Contains(ID))
                _baseGraph.createKeyIndex(ID, typeof(Vertex));

            if (_supportEdgeIds && !_baseGraph.getIndexedKeys(typeof(Edge)).Contains(ID))
                _baseGraph.createKeyIndex(ID, typeof(Edge));
        }

        static void verifyNativeElement(Element e)
        {
            if (!(e is IdElement))
                throw new ArgumentException("given element was not created in this graph");
        }
    }
}
