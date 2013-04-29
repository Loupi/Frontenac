using System;

namespace Frontenac.Blueprints
{
    public interface IGraphQuery : IQuery
    {
        new IGraphQuery Has(string key, object value);

        new IGraphQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>;

        new IGraphQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;

        new IGraphQuery Limit(long max);
    }
}
