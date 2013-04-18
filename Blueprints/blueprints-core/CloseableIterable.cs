using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// A CloseableIterable is required where it is necessary to deallocate resources from an IEnumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface CloseableIterable<out T> : IDisposable, IEnumerable<T>
    {
        
    }
}
