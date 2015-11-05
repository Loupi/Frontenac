using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RelationAttribute : Attribute
    {
        public readonly Direction Direction;
        private readonly string _label;

        public RelationAttribute(Direction direction)
        {
            Direction = direction;
        }

        public RelationAttribute(Direction direction, string label)
        {
            Direction = direction;
            _label = label;
        }

        public string AdjustKey(string key)
        {
            key = _label ?? key;

            /*switch (Direction)
            {
                case Direction.In:
                    return string.Concat("in", key);
                case Direction.Out:
                    return string.Concat("out", key);
                case Direction.Both:
                    return string.Concat("both", key);
                default:
                    throw new InvalidOperationException();
            }*/

            return key;
        }
    }
}