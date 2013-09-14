using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util
{
    public abstract class DefaultQuery : IQuery
    {
        private static readonly string[] EmptyLabels = new string[] {};
        protected readonly List<HasContainer> HasContainers = new List<HasContainer>();

        public Direction Direction = Direction.Both;
        internal long Innerlimit = long.MaxValue;
        public string[] Labels = EmptyLabels;

        public virtual IQuery Has(string key, object value)
        {
            HasContainers.Add(new HasContainer(key, value, Compare.Equal));
            return this;
        }

        public virtual IQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>
        {
            HasContainers.Add(new HasContainer(key, value, compare));
            return this;
        }

        public virtual IQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>
        {
            HasContainers.Add(new HasContainer(key, startValue, Compare.GreaterThanEqual));
            HasContainers.Add(new HasContainer(key, endValue, Compare.LessThan));
            return this;
        }

        public abstract IEnumerable<IEdge> Edges();
        public abstract IEnumerable<IVertex> Vertices();

        public virtual IQuery Limit(long max)
        {
            Innerlimit = max;
            return this;
        }

        public class HasContainer
        {
            public Compare Compare;
            public string Key;
            public object Value;

            public HasContainer(string key, object value, Compare compare)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(key));

                Key = key;
                Value = value;
                Compare = compare;
            }

            public bool IsLegal(IElement element)
            {
                Contract.Requires(element != null);

                var elementValue = element.GetProperty(Key);
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
                        return ((IComparable) elementValue).CompareTo(Value) >= 1;
                    case Compare.LessThan:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable) elementValue).CompareTo(Value) <= -1;
                    case Compare.GreaterThanEqual:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable) elementValue).CompareTo(Value) >= 0;
                    case Compare.LessThanEqual:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable) elementValue).CompareTo(Value) <= 0;
                    default:
                        throw new ArgumentException("Invalid state as no valid filter was provided");
                }
            }
        }
    }
}