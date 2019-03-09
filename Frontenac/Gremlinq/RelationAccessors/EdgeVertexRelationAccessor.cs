using System;
using System.Collections;
using System.Collections.Generic;
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

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
        private static object Convert<TModel>(IEnumerable elements, bool isWrapped, bool isEnumerable)
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedMember.Local
            where TModel : class
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            var vertices = elements.OfType<IVertex>();
            var models = isWrapped
                ? (IEnumerable<object>)vertices.As<TModel>()
                : vertices.Proxy<TModel>();

            return models;
        }

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedParameter.Local
        private static object CreateCollection<TModel>(IElement element, string key, RelationAccessor accessor, RelationAttribute rel)
// ReSharper restore UnusedParameter.Local
// ReSharper restore UnusedTypeParameter
// ReSharper restore UnusedMember.Local
        {
            throw new NotSupportedException();
        }

        public override object GetRelations(IElement element, string key, RelationAttribute rel)
        {
            var edge = element as IEdge;
            if (edge == null)
                throw new InvalidOperationException();

            IEnumerable<IVertex> vertices;
            string label;
            var direction = DirectionFromKey(key, out label);
            if (rel != null)
            {
                direction = rel.Direction;
            }

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

            return result.FirstOrDefault();
        }
    }
}