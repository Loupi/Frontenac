using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class GraphBackedTypeProvider : ITypeProvider
    {
        private const string GremlinqVertexProperty = "Frontenac_Gremlinq";
        private const string TypesIndexName = "Types";
        private const string TypeLabelName = "__type__";
        private const string TypePropertyName = "__type__";

        private readonly Dictionary<Type, object> _typesBuffer = new Dictionary<Type, object>();
        private readonly string _typePropertyName;

        private IGraph _graph;
        private IVertex _typesVertex;

        public GraphBackedTypeProvider(string typePropertyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(typePropertyName));

            _typePropertyName = typePropertyName;
        }

        IVertex GetTypesVertex()
        {
            IVertex vertex = null;
            var indexableGraph = _graph as IIndexableGraph;
            var keyIndexableGraph = _graph as IKeyIndexableGraph;

            if (indexableGraph != null && _graph.Features.SupportsVertexIndex)
            {
                var index = indexableGraph.GetIndex(GremlinqVertexProperty, typeof(IVertex)) ??
                            indexableGraph.CreateIndex(GremlinqVertexProperty, typeof(IVertex));

                vertex = Enumerable.OfType<IVertex>(index.Get(TypesIndexName, _typePropertyName)).SingleOrDefault();
                if (vertex == null)
                {
                    vertex = _graph.AddVertex(null);
                    index.Put(TypesIndexName, _typePropertyName, vertex);
                }
            }
            else if (keyIndexableGraph != null && _graph.Features.SupportsVertexKeyIndex)
            {
                if (!keyIndexableGraph.GetIndexedKeys(typeof(IVertex)).Contains(GremlinqVertexProperty))
                    keyIndexableGraph.CreateKeyIndex(GremlinqVertexProperty, typeof(IVertex));
            }

            return vertex ?? (_graph.V(GremlinqVertexProperty, _typePropertyName).SingleOrDefault());
        }

        void LoadTypesVertex(IGraph graph)
        {
            if (ReferenceEquals(_graph, graph)) return;

            _graph = graph;
            var vertex = GetTypesVertex();

            if (vertex == null)
            {
                vertex = _graph.AddVertex(null);
                vertex.SetProperty(GremlinqVertexProperty, _typePropertyName);
            }
            else
            {
                foreach (var typeVertex in vertex.Out(TypeLabelName))
                {
                    var property = typeVertex.GetProperty(TypePropertyName);
                    if (property == null) continue;
                    var type = Type.GetType(property.ToString(), false);
                    if (type != null)
                    {
                        _typesBuffer.Add(type, typeVertex.Id);
                    }
                    else
                    {
                        Debug.WriteLine("Cannot load type from {0} {1}", typeVertex, property);
                    }
                }
            }

            _typesVertex = vertex;
        }

        public virtual void SetType(IElement element, Type type)
        {
            LoadTypesVertex(element.Graph);

            if (!_typesBuffer.ContainsKey(type))
            {
                var typeVertex = _graph.AddVertex(null);
                typeVertex.SetProperty(TypePropertyName, type.AssemblyQualifiedName);
                _typesVertex.AddEdge(TypeLabelName, typeVertex);
                _typesBuffer.Add(type, typeVertex.Id);
            }

            object id;
            if (!_typesBuffer.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            element.SetProperty(_typePropertyName, id);
        }

        public virtual bool TryGetType(IElement element, out Type type)
        {
            LoadTypesVertex(element.Graph);

            object id;
            if (!element.TryGetValue(_typePropertyName, out id) || !Portability.IsNumber(id))
            {
                type = null;
                return false;
            }

            var kp = _typesBuffer.SingleOrDefault(pair => Portability.IsNumber(pair.Value) && Portability.IsNumber(id)
                                                              ? Convert.ToDouble(pair.Value).CompareTo(Convert.ToDouble(id)) == 0
                                                              : pair.Value != null && pair.Value.Equals(id));
            
            if (kp.Value != null)
                type = kp.Key;
            else
                throw new KeyNotFoundException(id.ToString());

            return true;
        }
    }
}