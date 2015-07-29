using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Frontenac.Blueprints;
using Castle.Components.DictionaryAdapter;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class VertexRelationAccessor : RelationAccessor
    {
        delegate void AccessCollectionDelegate2(IElement element, string key, RelationAccessor accessor, object id, object value);
        delegate void AccessCollectionDelegate(IElement element, string key, RelationAccessor accessor, object value);
        readonly AccessCollectionDelegate2 _addMethod;
        readonly AccessCollectionDelegate _removeMethod;

        public VertexRelationAccessor(Type modelType, bool isEnumerable, bool isWrapped, bool isCollection)
            : base(modelType, isEnumerable, isCollection, isWrapped, new[] { modelType })
        {
            _addMethod = (AccessCollectionDelegate2)CreateMagicMethod("Add", typeof(AccessCollectionDelegate2), modelType);
            _removeMethod = (AccessCollectionDelegate)CreateMagicMethod("Remove", typeof(AccessCollectionDelegate), modelType);
        }

// ReSharper disable UnusedMember.Local
        private static object Convert<TModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
// ReSharper restore UnusedMember.Local
            where TModel : class
        {
            Contract.Requires(elements != null);

            var vertices = elements.OfType<IVertex>();
            var models = isWrapped
                ? (IEnumerable<object>)vertices.As<TModel>()
                : vertices.Proxy<TModel>();

            return isEnumerable ? models : models.SingleOrDefault();
        }

// ReSharper disable UnusedMember.Local
        private static object CreateCollection<TModel>(IElement element, string key, RelationAccessor accessor)
// ReSharper restore UnusedMember.Local
        {
            Contract.Requires(!string.IsNullOrEmpty(key));

            return new VertexRelationCollection<TModel>((IVertex)element, key, accessor);
        }

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
        private static void Add<TModel>(IElement element, string key, RelationAccessor accessor, object id, object newValue)
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedMember.Local
            where TModel : class
        {
            Contract.Requires(!string.IsNullOrEmpty(key));

            new VertexRelationCollection<TModel>((IVertex)element, key, accessor).Add((TModel)newValue);
        }

// ReSharper disable UnusedMember.Local
        private static void Remove<TModel>(IElement element, string key, RelationAccessor accessor, object newValue)
// ReSharper restore UnusedMember.Local
            where TModel : class
        {
            Contract.Requires(!string.IsNullOrEmpty(key));

            new VertexRelationCollection<TModel>((IVertex)element, key, accessor).Remove((TModel)newValue);
        }

        public override object GetRelations(IElement element, string key)
        {
            var vertex = element as IVertex;
            IEnumerable<IVertex> vertices;
            string label;
            var direction = DirectionFromKey(key, out label);

            switch (direction)
            {
                case Direction.In:
                    vertices = vertex.In(label);
                    break;
                case Direction.Out:
                    vertices = vertex.Out(label);
                    break;
                case Direction.Both:
                    vertices = vertex.Both(label);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return ConvertMethod(vertices, IsWrapped, IsEnumerable);
        }

        public void SetRelation(IDictionaryAdapter dictionaryAdapter, IElement element, PropertyInfo property, string key, object value, object id)
        {
            Contract.Requires(dictionaryAdapter != null);
            Contract.Requires(!string.IsNullOrEmpty(key));

            var vertex = element as IVertex;
            if(vertex == null)
                throw new InvalidOperationException();

            var oldValue = dictionaryAdapter.GetProperty(key, false) as IDictionaryAdapter;
            if (oldValue != null)
                _removeMethod(element, key, this, oldValue);

            string label;
            var direction = DirectionFromKey(key, out label);
            var rel = (RelationAttribute)Attribute.GetCustomAttribute(property, typeof(RelationAttribute));
            if (rel != null)
            {
                label = rel.AdjustKey(key);
                direction = rel.Direction;
            }

            IEnumerable<IEdge> edges;

            switch (direction)
            {
                case Direction.In:
                    edges = vertex.InE(label);
                    break;
                case Direction.Out:
                    edges = vertex.OutE(label);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            foreach (var edge in edges)
            {
                vertex.Graph.RemoveEdge(edge);
            }

            if(value != null)
                _addMethod(element, key, this, id, value);
        }
    }
}