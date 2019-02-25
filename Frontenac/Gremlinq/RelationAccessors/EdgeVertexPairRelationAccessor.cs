using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class EdgeVertexPairRelationAccessor : RelationAccessor
    {
        delegate void AccessCollectionDelegate(IElement element, string key, RelationAccessor accessor, object value);
        delegate void AccessCollectionDelegate2(IElement element, string key, RelationAccessor accessor, object id, object value);
        readonly AccessCollectionDelegate2 _addMethod;
        readonly AccessCollectionDelegate _removeMethod;

        public EdgeVertexPairRelationAccessor(Type modelType, Type edgeType, bool isEnumerable, bool isWrapped, bool isCollection)
            : base(modelType, isEnumerable, isCollection, isWrapped, new[] { edgeType, modelType })
        {
            var models = new[] {edgeType, modelType};
            _addMethod = (AccessCollectionDelegate2)CreateMagicMethod("Add", typeof(AccessCollectionDelegate2), models);
            _removeMethod = (AccessCollectionDelegate)CreateMagicMethod("Remove", typeof(AccessCollectionDelegate), models);
        }

// ReSharper disable UnusedMember.Local
        private static object Convert<TEdgeModel, TVertexModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
// ReSharper restore UnusedMember.Local
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            var edges = elements.OfType<KeyValuePair<IEdge, IVertex>>();

            if (isWrapped)
            {
                var models = edges.Select(pair => new KeyValuePair<IEdge<TEdgeModel>, IVertex<TVertexModel>>(pair.Key.As<TEdgeModel>(), pair.Value.As<TVertexModel>()));
                return isEnumerable ? (object)models : models.SingleOrDefault();
            }
            else
            {
                var models = edges.Select(pair => new KeyValuePair<TEdgeModel, TVertexModel>(pair.Key.Proxy<TEdgeModel>(), pair.Value.Proxy<TVertexModel>()));
                return isEnumerable ? (object)models : models.SingleOrDefault();
            }
        }

// ReSharper disable UnusedMember.Local
        private static object CreateCollection<TEdgeModel, TVertexModel>(IElement element, string key, RelationAccessor accessor)
// ReSharper restore UnusedMember.Local
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return new EdgeVertexRelationCollection<TEdgeModel, TVertexModel>((IVertex)element, key, accessor);
        }

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
        private static void Add<TEdgeModel, TVertexModel>(IElement element, string key, RelationAccessor accessor, object id, object newValue)
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedMember.Local
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            new EdgeVertexRelationCollection<TEdgeModel, TVertexModel>((IVertex)element, key, accessor).Add((KeyValuePair<TEdgeModel, TVertexModel>)newValue);
        }

// ReSharper disable UnusedMember.Local
        private static void Remove<TEdgeModel, TVertexModel>(IElement element, string key, RelationAccessor accessor, object newValue)
// ReSharper restore UnusedMember.Local
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            new EdgeVertexRelationCollection<TEdgeModel, TVertexModel>((IVertex)element, key, accessor).Remove((KeyValuePair<TEdgeModel, TVertexModel>)newValue);
        }

        public override object GetRelations(IElement element, string key)
        {
            var vertex = element as IVertex;
            IEnumerable<KeyValuePair<IEdge, IVertex>> rawEdges;
            string label;
            var direction = DirectionFromKey(key, out label);

            switch (direction)
            {
                case Direction.In:
                    rawEdges = vertex.InE(label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.Out)));
                    break;
                case Direction.Out:
                    rawEdges = vertex.OutE(label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.In)));
                    break;
                case Direction.Both:
                    rawEdges = vertex.InE(label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.Out)))
                                     .Concat(vertex.OutE(label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.In))));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return ConvertMethod(rawEdges, IsWrapped, IsEnumerable);
        }

        public void SetRelation(IDictionaryAdapter dictionaryAdapter, IElement element, PropertyInfo property, string key, object value, object id)
        {
            if (dictionaryAdapter == null)
                throw new ArgumentNullException(nameof(dictionaryAdapter));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var vertex = element as IVertex;
            if (vertex == null)
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

            _addMethod(element, key, this, id, value);
        }
    }
}