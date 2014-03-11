using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class GraphBackedElementTypeProvider : IElementTypeProvider
    {
        private readonly IDictionaryAdapterFactory _dictionaryAdapterFactory = new DictionaryAdapterFactory();
        private const string GremlinqVertexProperty = "Frontenac_Gremlinq";
        private const string TypesVertexName = "Types";
        private const string TypeLabelName = "__type__";
        private const string TypePropertyName = "__type__";

        private readonly Dictionary<Type, object> _typesBuffer = new Dictionary<Type, object>();
        private readonly string _typePropertyName;
        private readonly IGraph _graph;
        private readonly IVertex _typesVertex;

        public GraphBackedElementTypeProvider(string typePropertyName, IGraph graph)
        {
            Contract.Requires(!string.IsNullOrEmpty(typePropertyName));
            Contract.Requires(graph != null);

            _typePropertyName = typePropertyName;
            _graph = graph;

            _typesVertex = _graph.V(GremlinqVertexProperty, TypesVertexName).SingleOrDefault();
            if (_typesVertex == null)
            {
                _typesVertex = _graph.AddVertex(null);
                _typesVertex.SetProperty(GremlinqVertexProperty, TypesVertexName);
            }
            else
            {
                foreach (var typeVertex in _typesVertex.Out(TypeLabelName))
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
        }

        public void SetType(IElement element, Type type)
        {
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

        public bool TryGetType(IElement element, out Type type)
        {
            object id;
            if (!element.TryGetValue(_typePropertyName, out id) || !Portability.IsNumber(id))
            {
                type = null;
                return false;
            }

            var kp = _typesBuffer.SingleOrDefault(pair =>
                {
                    if (Portability.IsNumber(pair.Value) && Portability.IsNumber(id))
                        return Convert.ToDouble(pair.Value).CompareTo(Convert.ToDouble(id)) == 0;
                    return pair.Value != null && pair.Value.Equals(id);
                });
            
            if (kp.Value != null)
                type = kp.Key;
            else
                throw new KeyNotFoundException(id.ToString());

            return true;
        }

        public object Proxy(IElement element, Type type)
        {
            var propsDesc = new PropertyDescriptor();
            propsDesc.AddBehavior(new DictionaryPropertyConverter());
            return _dictionaryAdapterFactory.GetAdapter(type, element, propsDesc);
        }
    }
}