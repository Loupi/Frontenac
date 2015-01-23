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

        private static object Convert<TModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
            where TModel : class
        {
            Contract.Requires(elements != null);

            var edges = elements.OfType<IEdge>();
            var models = isWrapped
                ? (IEnumerable<object>)edges.As<TModel>()
                : edges.Proxy<TModel>();

            return isEnumerable ? models : models.SingleOrDefault();
        }

        private static object CreateCollection<TModel>(IElement element, string key, RelationAccessor accessor)
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