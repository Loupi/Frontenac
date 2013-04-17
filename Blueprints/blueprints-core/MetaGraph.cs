using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// MetaGraph can be implemented as a way to access the underlying native graph engine.
    /// This is useful for those Graph implementations that are not native Blueprints implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface MetaGraph<T> : Graph
    {
        /// <summary>
        /// Get the raw underlying graph engine that exposes the Blueprints API.
        /// </summary>
        /// <returns>the raw underlying graph engine</returns>
        T GetRawGraph();
    }
}
