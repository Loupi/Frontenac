using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public interface GraphQuery : Query
    {
        new GraphQuery Has(string key, object value);

        new GraphQuery Has<T>(string key, Compare compare, T value) where T : IComparable<T>;

        new GraphQuery Interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;

        new GraphQuery Limit(long max);
    }
}
