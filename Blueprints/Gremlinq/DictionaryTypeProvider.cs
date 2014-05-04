using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

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
            Contract.Requires(!string.IsNullOrEmpty(typePropertyName));
            Contract.Requires(elementTypes != null);

            _typePropertyName = typePropertyName;
            _elementIdsToTypes = new Dictionary<int, Type>(elementTypes);
            _elementTypesToIds = elementTypes.ToDictionary(t => t.Value, t => t.Key);
        }

        public virtual void SetType(IElement element, Type type)
        {
            int id;
            if(!_elementTypesToIds.TryGetValue(type, out id))
                throw new KeyNotFoundException(type.AssemblyQualifiedName);

            element.SetProperty(_typePropertyName, id);
        }

        public virtual bool TryGetType(IElement element, out Type type)
        {
            object id;
            if (!element.TryGetValue(_typePropertyName, out id) || !Portability.IsNumber(id))
            {
                type = null;
                return false;
            }

            if (!_elementIdsToTypes.TryGetValue(Convert.ToInt32(id), out type))
                throw new KeyNotFoundException(id.ToString());

            return true;
        }
    }
}