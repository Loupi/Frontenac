using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;
using PropertyDescriptor = Castle.Components.DictionaryAdapter.PropertyDescriptor;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class ElementPropertyGetterBehavior : IDictionaryPropertyGetter, IDictionaryPropertySetter
    {
        readonly ConcurrentDictionary<Type, object> _ignoredTypes = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, RelationAccessor> _vertexAccessors = new ConcurrentDictionary<Type, RelationAccessor>();
        readonly ConcurrentDictionary<Type, RelationAccessor> _edgeAccessors = new ConcurrentDictionary<Type, RelationAccessor>();
        readonly ConcurrentDictionary<string, string> _accessorOverrides = new ConcurrentDictionary<string, string>();

        readonly ConditionalWeakTable<IDictionaryAdapter, ConcurrentDictionary<string, object>> _proxyRelationsBuffer = 
            new ConditionalWeakTable<IDictionaryAdapter, ConcurrentDictionary<string, object>>();
                    

        public ElementPropertyGetterBehavior()
        {
            ExecutionOrder = 0;
        }

        public IDictionaryBehavior Copy()
        {
            return null;
        }

        public int ExecutionOrder { get; private set; }

        public bool SetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, ref object value, PropertyDescriptor property)
        {
            var element = dictionaryAdapter.This.Dictionary as IElement;
            if (element != null)
            {
                RelationAccessor accessor;

                if (TryGetAccessor(element, property.Property, property.PropertyType, out accessor))
                {
                    if (!accessor.IsEnumerable)
                    {
                        var rel = accessor as VertexRelationAccessor;
                        if (rel != null)
                        {
                            rel.SetRelation(dictionaryAdapter, element, property.Property, key, value, null);
                        }
                        else
                        {
                            var relPair = accessor as EdgeVertexPairRelationAccessor;
                            if (relPair != null)
                            {
                                relPair.SetRelation(dictionaryAdapter, element, property.Property, key, value, null);
                            }
                        }
                    }

                    return false;
                }
            }
            
            dictionaryAdapter.This.Dictionary.Add(key, value);
            

            return true;
        }

        public object GetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, object storedValue, 
                                       PropertyDescriptor property, bool ifExists)
        {
            //Get the edges data for these kinds or properties:
            //IEnumerable<KeyValuePair<IEdge<TEdgeModel>, IVertex<TModel>>>
            //IEnumerable<KeyValuePair<TEdgeModel, TModel>>
            //IEnumerable<IEdge<TModel>>
            //IEnumerable<IVertex<TModel>>
            //IEnumerable<TModel> (as vertex)
            //KeyValuePair<IEdge<TEdgeModel>, IVertex<TModel>>
            //KeyValuePair<TEdgeModel, TModel>
            //IEdge<TModel>
            //IVertex<TModel>
            //TModel (as vertex)

            var element = dictionaryAdapter.This.Dictionary as IElement;
            if (element != null)
            {
                RelationAccessor accessor;

                if (!ifExists && TryGetAccessor(element, property.Property, property.PropertyType, out accessor))
                {
                    ConcurrentDictionary<string, object> relations = null;
                    object result;

                    if (accessor.IsEnumerable)
                    {
                        relations = _proxyRelationsBuffer.GetOrCreateValue(dictionaryAdapter);
                        if (relations.TryGetValue(key, out result))
                            return result;
                    }

                    var keyName = string.Concat(dictionaryAdapter.Meta.Type.Name, property.PropertyName);
                    string newKey;
                    if (!_accessorOverrides.TryGetValue(keyName, out newKey))
                    {
                        var rel = (RelationAttribute)Attribute.GetCustomAttribute(property.Property, typeof(RelationAttribute));
                        newKey = rel != null ? rel.AdjustKey(key) : null;
                        _accessorOverrides.TryAdd(keyName, newKey);
                    }

                    if (newKey == null)
                        newKey = key;
                    
                    result = accessor.IsCollection
                        ? accessor.CreateCollectionMethod(element, newKey, accessor)
                        : accessor.GetRelations(element, newKey);

                    if (relations != null && accessor.IsEnumerable && key != null)
                        relations.TryAdd(key, result);

                    return result;
                }

                if (key == "Id")
                {
                    if (property.PropertyType.IsPrimitive && property.PropertyType != element.Id.GetType())
                    {
                        var tc = TypeDescriptor.GetConverter(property.PropertyType);
                        return tc.ConvertFromString(element.Id.ToString());
                    }
                    
                    return element.Id;
                }
            }

            if (null == storedValue || property.PropertyType.IsInstanceOfType(storedValue))
                return storedValue;

            object convertedValue;
            if (property.PropertyType.IsPrimitive)
            {
                var tc = TypeDescriptor.GetConverter(property.PropertyType);
                convertedValue = tc.ConvertFromString(storedValue.ToString());
            }
            else
                convertedValue = Activator.CreateInstance(property.PropertyType, storedValue);

            return convertedValue;
        }

        public bool TryGetAccessor(IElement element, PropertyInfo propertyInfo, Type propertyType, out RelationAccessor accessor)
        {
            //We only support relations defined with interfaces
            if (_ignoredTypes.ContainsKey(propertyType) ||
                (!propertyType.IsInterface &&
                 (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof (KeyValuePair<,>))))
            {
                accessor = null;
                return false;
            }

            ConcurrentDictionary<Type, RelationAccessor> accessors = null;
            var elementIsEdge = false;

            if (element is IVertex)
                accessors = _vertexAccessors;
            else if (element is IEdge)
            {
                elementIsEdge = true;
                accessors = _edgeAccessors;
            }

            if (accessors == null)
                throw new InvalidOperationException();

            //look in known types so that we do not have call this reflection code every time
            if (accessors.TryGetValue(propertyType, out accessor))
                return true;

            if (!TryCreateAccessor(propertyType, elementIsEdge, out accessor))
                return false;

            accessors.TryAdd(propertyType, accessor);
            return true;
        }

        bool TryCreateAccessor(Type propertyType, bool elementIsEdge, out RelationAccessor accessor)
        {
            Contract.Requires(propertyType != null);

            Type modelType;
            Type edgeType;
            var isModelWrapped = false;
            var isEdgeWrapped = false;
            bool isCollection;
            var wrapModelAsEdge = false;

            var isEnumerable = GetModels(propertyType, out modelType, out edgeType, out isCollection);
            GetWrapperTypes(ref modelType, ref edgeType, ref isModelWrapped, ref isEdgeWrapped, ref wrapModelAsEdge);

            //From that point, we scanned for all possibilities.
            //Now make sure we got a valid model type to continue.
            //It needs to be an interface, and cannot be IEnumerable.
            //Wrapped and non wrapped elements cannot be mixed on generics.
            if (modelType == null || !modelType.IsInterface ||
                typeof(IEnumerable).IsAssignableFrom(modelType) ||
                (edgeType != null && (elementIsEdge || !edgeType.IsInterface ||
                 isEdgeWrapped != isModelWrapped || typeof(IEnumerable).IsAssignableFrom(edgeType))))
            {
                _ignoredTypes.TryAdd(propertyType, null);
                accessor = null;
                return false;
            }

            //Create edge accessor object
            if (elementIsEdge)
                accessor = new EdgeVertexRelationAccessor(modelType, isEnumerable, isModelWrapped);
            else if (wrapModelAsEdge)
                accessor = new EdgeRelationAccessor(modelType, isEnumerable, true);
            else
            {
                if (edgeType == null)
                    accessor = new VertexRelationAccessor(modelType, isEnumerable, isModelWrapped, isCollection);
                else
                    accessor = new EdgeVertexPairRelationAccessor(modelType, edgeType, isEnumerable, isModelWrapped, isCollection);
            }

            return true;
        }

        static bool GetModels(Type propertyType, out Type modelType, out Type edgeType, out bool isCollection)
        {
            Contract.Requires(propertyType != null);

            var isEnumerable = false;

            if (propertyType.IsGenericType)
            {
                //If IEnumerable, get underlying types 
                var genericDef = propertyType.GetGenericTypeDefinition();
                isCollection = genericDef == typeof (ICollection<>);
                if (genericDef == typeof(IEnumerable<>) || genericDef == typeof(ICollection<>))
                {
                    isEnumerable = true;

                    //If IEnumerable<KeyValuePair<,>>, get model and edge types
                    var arg = propertyType.GetGenericArguments()[0];
                    if (arg.IsGenericType && arg.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        var kvArgs = arg.GetGenericArguments();
                        edgeType = kvArgs[0];
                        modelType = kvArgs[1];
                    }    
                    else
                    {
                        //Otherwise only get model type
                        modelType = arg;
                        edgeType = null;
                    }
                }
                //If KeyValuePair<,>, get model and edge types, single entity
                else if (genericDef == typeof(KeyValuePair<,>))
                {
                    var kvArgs = propertyType.GetGenericArguments();
                    edgeType = kvArgs[0];
                    modelType = kvArgs[1];
                }
                else
                {
                    //Otherwise only get model type, single generic entity
                    modelType = propertyType;
                    edgeType = null;
                }
            }
            else
            {
                //get model type, single entity
                modelType = propertyType;
                isCollection = false;
                edgeType = null;
            }

            return isEnumerable;
        }

        static void GetWrapperTypes(ref Type modelType, ref Type edgeType, ref bool isModelWrapped, ref bool isEdgeWrapped, ref bool wrapModelAsEdge)
        {
            //Determine if model is wrapped, and if so, if it is a wrapped edge or vertex
            //also reassign with model
            if (modelType != null && modelType.IsInterface && modelType.IsGenericType)
            {
                var generidDef = modelType.GetGenericTypeDefinition();
                if (generidDef == typeof(IVertex<>))
                {
                    isModelWrapped = true;
                    modelType = modelType.GetGenericArguments()[0];
                }
                else if (edgeType == null && generidDef == typeof(IEdge<>))
                {
                    isModelWrapped = true;
                    wrapModelAsEdge = true;
                    modelType = modelType.GetGenericArguments()[0];
                }
            }

            //Determine if edge is wrapped
            if (edgeType != null && edgeType.IsInterface && edgeType.IsGenericType &&
                edgeType.GetGenericTypeDefinition() == typeof(IEdge<>))
            {
                isEdgeWrapped = true;
                edgeType = edgeType.GetGenericArguments()[0];
            }
        }
    }
}
