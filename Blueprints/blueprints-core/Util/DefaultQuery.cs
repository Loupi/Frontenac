using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public abstract class DefaultQuery : Query
    {
        public abstract Query has(string key, object value);
        public abstract Query has<T>(string key, Compare compare, T value) where T : IComparable<T>;
        public abstract Query interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;
        public abstract IEnumerable<Edge> edges();
        public abstract IEnumerable<Vertex> vertices();
        public abstract Query limit(long max);

        static readonly string[] EMPTY_LABELS = new string[] { };

        public Direction direction = Direction.BOTH;
        public string[] _labels = DefaultQuery.EMPTY_LABELS;
        public long _limit = long.MaxValue;
        public List<HasContainer> hasContainers = new List<HasContainer>();

        public class HasContainer
        {
            public string key;
            public object value;
            public Compare compare;

            public HasContainer(string key, object value, Compare compare)
            {
                this.key = key;
                this.value = value;
                this.compare = compare;
            }

            public bool isLegal(Element element)
            {
                object elementValue = element.getProperty(key);
                switch (compare)
                {
                    case Compare.EQUAL:
                        if (null == elementValue)
                            return value == null;
                        return elementValue.Equals(value);
                    case Compare.NOT_EQUAL:
                        if (null == elementValue)
                            return value != null;
                        return !elementValue.Equals(value);
                    case Compare.GREATER_THAN:
                        if (null == elementValue || value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(value) >= 1;
                    case Compare.LESS_THAN:
                        if (null == elementValue || value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(value) <= -1;
                    case Compare.GREATER_THAN_EQUAL:
                        if (null == elementValue || value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(value) >= 0;
                    case Compare.LESS_THAN_EQUAL:
                        if (null == elementValue || value == null)
                            return false;
                        return ((IComparable)elementValue).CompareTo(value) <= 0;
                    default:
                        throw new ArgumentException("Invalid state as no valid filter was provided");
                }
            }
        }
    }
}
