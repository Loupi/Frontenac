using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class EdgeVertexRelationAccessor : RelationAccessor
    {
        public EdgeVertexRelationAccessor(Type modelType, bool isEnumerable, bool isWrapped)
            : base(modelType, isEnumerable, false, isWrapped, new[] { modelType })
        {

        }

        private static object Convert<TModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
            where TModel : class
        {
            Contract.Requires(elements != null);

            var vertices = elements.OfType<IVertex>();
            var models = isWrapped
                ? (IEnumerable<object>)vertices.As<TModel>()
                : vertices.Proxy<TModel>();

            return models;
        }

        private static object CreateCollection<TModel>(IElement element, string key, RelationAccessor accessor)
        {
            throw new NotSupportedException();
        }

        public override object GetRelations(IElement element, string key)
        {
            var edge = element as IEdge;
            if (edge == null)
                throw new InvalidOperationException();

            IEnumerable<IVertex> vertices;
            string label;
            var direction = DirectionFromKey(key, out label);

            switch (direction)
            {
                case Direction.In:
                    vertices = new[] { edge.GetVertex(Direction.In) };
                    break;
                case Direction.Out:
                    vertices = new[] { edge.GetVertex(Direction.Out) };
                    break;
                case Direction.Both:
                    vertices = new[] { edge.GetVertex(Direction.In), edge.GetVertex(Direction.Out) };
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var result = (IEnumerable<object>)ConvertMethod(vertices, IsWrapped, IsEnumerable);
            if (IsEnumerable && direction == Direction.Both)
                return result;

            return result.SingleOrDefault();
        }
    }
}