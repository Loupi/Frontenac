using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    public class ComparableQueryElement : QueryElement
    {
        public ComparableQueryElement(string key, Compare comparison, object value)
            : base(key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            Comparison = comparison;
            Value = value;
        }

        public Compare Comparison { get; private set; }
        public object Value { get; private set; }
    }
}