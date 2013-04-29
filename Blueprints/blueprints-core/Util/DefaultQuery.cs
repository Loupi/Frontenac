using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    public abstract class DefaultQuery : IQuery
    {
        public abstract IQuery Has(string key, object value);
        public abstract IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>;
        public abstract IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;
        public abstract IEnumerable<IEdge> Edges();
        public abstract IEnumerable<IVertex> Vertices();
        public abstract IQuery Limit(long max);

        static readonly string[] EmptyLabels = new string[] { };

        public Direction Direction = Direction.Both;
        public string[] Labels = EmptyLabels;
        internal long Innerlimit = long.MaxValue;
        public List<HasContainer> HasContainers = new List<HasContainer>();

        public class HasContainer
        {
            public string Key;
            public object Value;
            public Compare Compare;

            public HasContainer(string key, object value, Compare compare)
            {
                Key = key;
                Value = value;
                Compare = compare;
            }

            public bool IsLegal(IElement element)
            {
                object elementValue = element.GetProperty(Key);
                switch (Compare)
                {
                    case Compare.Equal:
                        if (null == elementValue)
                            return Value == null;
                        return elementValue.Equals(Value);
                    case Compare.NotEqual:
                        if (null == elementValue)
                            return Value != null;
                        return !elementValue.Equals(Value);
                    case Compare.GreaterThan:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) >= 1;
                    case Compare.LessThan:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) <= -1;
                    case Compare.GreaterThanEqual:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) >= 0;
                    case Compare.LessThanEqual:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) <= 0;
                    default:
                        throw new ArgumentException("Invalid state as no valid filter was provided");
                }
            }
        }
    }
}
