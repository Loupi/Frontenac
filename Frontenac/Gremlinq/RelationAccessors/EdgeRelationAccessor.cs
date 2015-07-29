using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class EdgeRelationAccessor : RelationAccessor
    {
        public EdgeRelationAccessor(Type modelType, bool isEnumerable, bool isWrapped)
            : base(modelType, isEnumerable, false, isWrapped, new[] { modelType })
        {

        }

// ReSharper disable UnusedMember.Local
        private static object Convert<TModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
// ReSharper restore UnusedMember.Local
            where TModel : class
        {
            Contract.Requires(elements != null);

            var edges = elements.OfType<IEdge>();
            var models = isWrapped
                ? (IEnumerable<object>)edges.As<TModel>()
                : edges.Proxy<TModel>();

            return isEnumerable ? models : models.SingleOrDefault();
        }

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedParameter.Local
        private static object CreateCollection<TModel>(IElement element, string key, RelationAccessor accessor)
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedTypeParameter
// ReSharper restore UnusedMember.Local
        {
            throw new NotSupportedException();
        }

        public override object GetRelations(IElement element, string key)
        {
            var vertex = element as IVertex;
            IEnumerable<IEdge> edges;
            string label;
            var direction = DirectionFromKey(key, out label);

            switch (direction)
            {
                case Direction.In:
                    edges = vertex.InE(label);
                    break;
                case Direction.Out:
                    edges = vertex.OutE(label);
                    break;
                case Direction.Both:
                    edges = vertex.BothE(label);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return ConvertMethod(edges, IsWrapped, IsEnumerable);
        }
    }
}