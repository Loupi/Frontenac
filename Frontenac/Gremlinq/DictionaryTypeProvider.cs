using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    public class DictionaryTypeProvider : ITypeProvider
    {
        public const string DefaulTypePropertyName = "__g_type__";

        private readonly string _typePropertyName;
        private readonly Dictionary<int, Type> _elementIdsToTypes;
        private readonly Dictionary<Type, int> _elementTypesToIds;

        public DictionaryTypeProvider(string typePropertyName, IDictionary<int, Type> elementTypes)
        {
            if (string.IsNullOrEmpty(typePropertyName))
                throw new ArgumentNullException(nameof(typePropertyName));
            if (elementTypes == null)
                throw new ArgumentNullException(nameof(elementTypes));

            _typePropertyName = typePropertyName;
            _elementIdsToTypes = new Dictionary<int, Type>(elementTypes);
            _elementTypesToIds = elementTypes.ToDictionary(t => t.Value, t => t.Key);
        }

        public virtual void SetType(IElement element, Type type)
        {
            TypeProviderContract.ValidateSetType(element, type);

            int id;
            if(!_elementTypesToIds.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            element.SetProperty(_typePropertyName, id);
        }

        public virtual bool TryGetType(IDictionary<string, object> element, IGraph graph, out Type type)
        {
            TypeProviderContract.ValidateTryGetType(element);

            object id;
            if (!element.TryGetValue(_typePropertyName, out id) || !GraphHelpers.IsNumber(id))
            {
                type = null;
                return false;
            }

            if (!_elementIdsToTypes.TryGetValue(Convert.ToInt32(id), out type))
                throw new KeyNotFoundException(id.ToString());

            return true;
        }

        public IEnumerable<IVertex> GetVerticesOfType(IGraph graph, Type type)
        {
            int id;
            if (!_elementTypesToIds.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            return graph.GetVertices(_typePropertyName, id);
        }

        public IEnumerable<IEdge> GetEdgesOfType(IGraph graph, Type type)
        {
            int id;
            if (!_elementTypesToIds.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            return graph.GetEdges(_typePropertyName, id);
        }

        public IEnumerable<Type> GetTypes(IGraph graph)
        {
            return _elementIdsToTypes.Values;
        }
    }
}