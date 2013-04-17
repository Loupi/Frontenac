using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public abstract class DefaultQuery : Query
    {
        public abstract Query Has(string key, object value);
        public abstract Query Has<T>(string key, Compare compare, T value) where T : IComparable<T>;
        public abstract Query Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;
        public abstract IEnumerable<Edge> Edges();
        public abstract IEnumerable<Vertex> Vertices();
        public abstract Query Limit(long max);

        static readonly string[] EMPTY_LABELS = new string[] { };

        public Direction _Direction = Direction.BOTH;
        public string[] _Labels = DefaultQuery.EMPTY_LABELS;
        public long _Limit = long.MaxValue;
        public List<HasContainer> HasContainers = new List<HasContainer>();

        public class HasContainer
        {
            public string Key;
            public object Value;
            public Compare Compare;

            public HasContainer(string key, object value, Compare compare)
            {
                this.Key = key;
                this.Value = value;
                this.Compare = compare;
            }

            public bool IsLegal(Element element)
            {
                object elementValue = element.GetProperty(Key);
                switch (Compare)
                {
                    case Compare.EQUAL:
                        if (null == elementValue)
                            return Value == null;
                        return elementValue.Equals(Value);
                    case Compare.NOT_EQUAL:
                        if (null == elementValue)
                            return Value != null;
                        return !elementValue.Equals(Value);
                    case Compare.GREATER_THAN:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) >= 1;
                    case Compare.LESS_THAN:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) <= -1;
                    case Compare.GREATER_THAN_EQUAL:
                        if (null == elementValue || Value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(Value) >= 0;
                    case Compare.LESS_THAN_EQUAL:
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
