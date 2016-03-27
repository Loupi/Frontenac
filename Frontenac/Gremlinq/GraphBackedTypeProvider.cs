using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class GraphBackedTypeProvider : ITypeProvider
    {
        private const string GremlinqVertexProperty = "Frontenac_Gremlinq";
        private const string TypesIndexName = "Types";
        private const string TypeLabelName = "__gtype__";
        private const string TypePropertyName = "__gtype__";
        private readonly string _typePropertyName;

        private readonly ConditionalWeakTable<IGraph, PerGraphInstanceTypes>  _perGraphInstanceTypes 
            = new ConditionalWeakTable<IGraph, PerGraphInstanceTypes>();
        
// ReSharper disable ClassNeverInstantiated.Local
        private class PerGraphInstanceTypes
// ReSharper restore ClassNeverInstantiated.Local
        {
            public readonly Dictionary<Type, object> TypesBuffer = new Dictionary<Type, object>();
            public IVertex TypesVertex;

            private static IVertex GetTypesVertex(IGraph graph, string typePropertyName)
            {
                IVertex vertex = null;
                var indexableGraph = graph as IIndexableGraph;
                var keyIndexableGraph = graph as IKeyIndexableGraph;

                if (indexableGraph != null && graph.Features.SupportsVertexIndex)
                {
                    var index = indexableGraph.GetIndex(GremlinqVertexProperty, typeof(IVertex)) ??
                                indexableGraph.CreateIndex(GremlinqVertexProperty, typeof(IVertex));

                    vertex = Enumerable.OfType<IVertex>(index.Get(TypesIndexName, typePropertyName)).FirstOrDefault();
                    if (vertex == null)
                    {
                        vertex = graph.AddVertex(null);
                        index.Put(TypesIndexName, typePropertyName, vertex);
                    }
                }
                else if (keyIndexableGraph != null && graph.Features.SupportsVertexKeyIndex)
                {
                    if (!keyIndexableGraph.GetIndexedKeys(typeof(IVertex)).Contains(GremlinqVertexProperty))
                        keyIndexableGraph.CreateKeyIndex(GremlinqVertexProperty, typeof(IVertex));
                }

                /*if (keyIndexableGraph != null)
                {
                    keyIndexableGraph.CreateKeyIndex(typePropertyName, typeof(IVertex));
                    keyIndexableGraph.CreateKeyIndex(typePropertyName, typeof(IEdge));
                }*/   

                return vertex ?? graph.V(GremlinqVertexProperty, typePropertyName).FirstOrDefault();
            }

            public void LoadTypesVertex(IGraph graph, string typePropertyName)
            {
                if (TypesVertex != null) return;

                var vertex = GetTypesVertex(graph, typePropertyName);

                if (vertex == null)
                {
                    vertex = graph.AddVertex(null);
                    vertex.SetProperty(GremlinqVertexProperty, typePropertyName);
                }
                else
                {
                    foreach (var typeVertex in vertex.Out(TypeLabelName))
                    {
                        var property = typeVertex.GetProperty(TypePropertyName);
                        if (property == null) continue;
                        var type = Type.GetType(property.ToString(), false);
                        if (type != null && !TypesBuffer.Keys.Contains(type))
                        {
                            TypesBuffer.Add(type, typeVertex.Id);
                        }
                        else
                        {
                            Debug.WriteLine("Cannot load type from {0} {1}", typeVertex, property);
                        }
                    }
                }

                TypesVertex = vertex;
            }
        }

        public GraphBackedTypeProvider(string typePropertyName)
        {
            Contract.Requires(!string.IsNullOrEmpty(typePropertyName));

            _typePropertyName = typePropertyName;
        }

        public virtual void SetType(IElement element, Type type)
        {
            var graph = element.Graph;
            var instanceTypes = _perGraphInstanceTypes.GetOrCreateValue(graph);
            instanceTypes.LoadTypesVertex(graph, _typePropertyName);

            if (!instanceTypes.TypesBuffer.ContainsKey(type))
            {
                var typeVertex = graph.AddVertex(null);
                typeVertex.SetProperty(TypePropertyName, type.AssemblyQualifiedName);
                instanceTypes.TypesVertex.AddEdge(null, TypeLabelName, typeVertex);
                instanceTypes.TypesBuffer.Add(type, typeVertex.Id);
            }

            object id;
            if (!instanceTypes.TypesBuffer.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            element.SetProperty(_typePropertyName, id);
        }

        public virtual bool TryGetType(IDictionary<string, object> element, IGraph graph, out Type type)
        {
            var instanceTypes = _perGraphInstanceTypes.GetOrCreateValue(graph);
            instanceTypes.LoadTypesVertex(graph, _typePropertyName);

            object id;
            if (!element.TryGetValue(_typePropertyName, out id))
            {
                type = null;
                return false;
            }

            var kp = instanceTypes.TypesBuffer
                .FirstOrDefault(pair => GraphHelpers.IsNumber(pair.Value) && GraphHelpers.IsNumber(id)
                    ? Convert.ToDouble(pair.Value).CompareTo(Convert.ToDouble(id)) == 0
                    : pair.Value != null && pair.Value.Equals(id));
            
            if (kp.Value != null)
                type = kp.Key;
            else
                throw new KeyNotFoundException(id.ToString());

            return true;
        }

        public IEnumerable<IVertex> GetVerticesOfType(IGraph graph, Type type)
        {
            var instanceTypes = _perGraphInstanceTypes.GetOrCreateValue(graph);
            instanceTypes.LoadTypesVertex(graph, _typePropertyName);

            object id;
            return !instanceTypes.TypesBuffer.TryGetValue(type, out id) 
                ? Enumerable.Empty<IVertex>() 
                : graph.GetVertices(_typePropertyName, id);
        }

        public IEnumerable<IEdge> GetEdgesOfType(IGraph graph, Type type)
        {
            var instanceTypes = _perGraphInstanceTypes.GetOrCreateValue(graph);
            instanceTypes.LoadTypesVertex(graph, _typePropertyName);

            object id;
            return !instanceTypes.TypesBuffer.TryGetValue(type, out id) 
                ? Enumerable.Empty<IEdge>() 
                : graph.GetEdges(_typePropertyName, id);
        }

        public IEnumerable<Type> GetTypes(IGraph graph)
        {
            var instanceTypes = _perGraphInstanceTypes.GetOrCreateValue(graph);
            instanceTypes.LoadTypesVertex(graph, _typePropertyName);

            return instanceTypes.TypesBuffer.Keys;
        }
    }
}