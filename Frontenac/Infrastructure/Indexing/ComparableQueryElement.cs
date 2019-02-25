using System;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    public class ComparableQueryElement : QueryElement
    {
        public ComparableQueryElement(string key, Compare comparison, object value)
            : base(key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            Comparison = comparison;
            Value = value;
        }

        public Compare Comparison { get; private set; }
        public object Value { get; private set; }
    }
}