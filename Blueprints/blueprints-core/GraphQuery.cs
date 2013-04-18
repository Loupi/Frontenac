using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public interface GraphQuery : Query
    {
        new GraphQuery has(string key, object value);

        new GraphQuery has<T>(string key, Compare compare, T value) where T : IComparable<T>;

        new GraphQuery interval<T>(string key, T startValue, T endValue) where T : IComparable<T>;

        new GraphQuery limit(long max);
    }
}
